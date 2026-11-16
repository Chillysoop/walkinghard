using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    // Presentation only. Own noise generator and wall clock never consume combat RNG/time.
    sealed class CombatAudio
    {
        internal const int Capacity=12;
        internal sealed class Cue {public string name;public AudioClip clip;public int priority;public float gain,gap;}
        sealed class Voice {public AudioSource source;public float until,gain;public int priority;}
        readonly Voice[] voices=new Voice[Capacity];
        readonly Dictionary<string,Cue> cues=new Dictionary<string,Cue>();
        readonly Dictionary<string,float> next=new Dictionary<string,float>();
        float duckUntil;int variant;
        internal int Played {get;private set;}
        internal int Suppressed {get;private set;}
        internal int Peak {get;private set;}
        internal int Active {get {int n=0;foreach(var v in voices)if(v.until>Time.unscaledTime)n++;return n;}}
        internal IEnumerable<Cue> Cues=>cues.Values;
        internal CombatAudio(Transform parent)
        {
            var root=new GameObject("Combat sound / twelve voices");root.transform.SetParent(parent,false);
            for(int i=0;i<Capacity;i++){var s=root.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=0;voices[i]=new Voice{source=s};}
            Add("fist",.16f,112,.52f,.13f,1,.48f,.065f);
            Add("slash",.23f,680,.15f,.76f,1,.37f,.075f);
            Add("hammer",.38f,54,.75f,.24f,1,.59f,.09f);
            Add("electric",.25f,960,.22f,.69f,1,.34f,.09f);
            Add("gun",.18f,185,.35f,.71f,1,.42f,.06f);
            Add("force",.44f,145,.55f,.38f,1,.38f,.15f);
            Add("armor",.19f,90,.6f,.34f,2,.5f,.12f);
            Add("block",.22f,420,.64f,.12f,2,.4f,.14f);
            Add("collision",.28f,70,.7f,.28f,1,.42f,.15f);
            Add("exposed",.42f,560,.65f,.09f,3,.47f,.4f);
            Add("press-warning",.65f,155,.78f,.06f,4,.55f,.55f);
            Add("bell-warning",.85f,440,.8f,.02f,4,.48f,.7f);
            Add("sweep-warning",.7f,240,.63f,.18f,4,.52f,.6f);
            foreach(string id in new[]{"singularity","tempest","cyclone","redline","harvest"})
            {
                float hz=id=="singularity"?80:id=="tempest"?720:id=="cyclone"?510:id=="redline"?180:310;
                Add(id+"-charge",.6f,hz,.58f,.12f,3,.32f,.3f);
                Add(id+"-release",id=="redline"?.48f:.85f,hz,.65f,id=="tempest"?.5f:.27f,3,.6f,.35f);
            }
        }
        void Add(string name,float length,float hz,float body,float grit,int priority,float gain,float gap)
        {
            int count=Mathf.CeilToInt(length*44100);var data=new float[count];uint seed=2166136261;foreach(char c in name)seed=(seed^c)*16777619;
            var noise=new System.Random(unchecked((int)seed));float filtered=0,phase=0;bool warning=name.EndsWith("warning"),charge=name.EndsWith("charge"),release=name.EndsWith("release");
            for(int i=0;i<count;i++)
            {
                float t=i/44100f,u=t/length;filtered=Mathf.Lerp(filtered,(float)noise.NextDouble()*2-1,.32f);
                float bend=charge?.65f+u:warning?1:1-.5f*u;
                phase+=2*Mathf.PI*hz*bend/44100;
                float tone=Mathf.Sin(phase)+.24f*Mathf.Sin(phase*2.31f)+.12f*Mathf.Sin(phase*3.7f);
                float env=charge?Mathf.Sin(u*Mathf.PI*.8f)*Mathf.Min(1,(1-u)*12):Mathf.Exp(-u*(warning?3:release?5:8));
                if(warning){float pulse=Mathf.Repeat(t,.22f);env*=.25f+.75f*Mathf.Exp(-pulse*25);}
                float transient=filtered*Mathf.Exp(-t*75)*.32f;
                float attack=Mathf.Min(1,t*600),tail=Mathf.Min(1,(1-u)*100);
                data[i]=Mathf.Clamp((tone*body*.48f+filtered*grit+transient)*env*attack*tail,-.85f,.85f);
            }
            var clip=AudioClip.Create(name,count,1,44100,false);clip.SetData(data,0);
            cues.Add(name,new Cue{name=name,clip=clip,priority=priority,gain=gain,gap=gap});
        }
        internal bool Play(string name,bool muted,Action<AudioClip,float> capture=null)
        {
            if(!cues.TryGetValue(name,out var cue))return false;
            float now=Time.unscaledTime;if(muted){capture?.Invoke(cue.clip,cue.gain*.55f);Stop();return false;}
            if(next.TryGetValue(name,out float due)&&now<due){Suppressed++;return false;}
            // Two slots are reserved for warnings and ultimates. A warning can replace a quiet impact.
            Voice chosen=null;int limit=cue.priority>=3?Capacity:Capacity-2;
            for(int i=0;i<limit;i++)if(voices[i].until<=now){chosen=voices[i];break;}
            if(chosen==null)for(int i=0;i<limit;i++)if(voices[i].priority<cue.priority&&(chosen==null||voices[i].priority<chosen.priority))chosen=voices[i];
            if(chosen==null){Suppressed++;return false;}
            next[name]=now+cue.gap;if(cue.priority==4)duckUntil=Mathf.Max(duckUntil,now+cue.clip.length);
            chosen.source.Stop();chosen.priority=cue.priority;chosen.gain=cue.gain;
            chosen.source.pitch=cue.priority>=3?1:1+((variant++%5)-2)*.022f;
            chosen.until=now+cue.clip.length/chosen.source.pitch;chosen.source.clip=cue.clip;
            Mix();chosen.source.Play();Played++;Peak=Mathf.Max(Peak,Active);capture?.Invoke(cue.clip,chosen.source.volume);return true;
        }
        internal void Mix()
        {float now=Time.unscaledTime;foreach(var v in voices)v.source.volume=v.gain*.55f*(v.priority<3&&now<duckUntil?.28f:1);}
        internal void Stop(){foreach(var v in voices){v.source.Stop();v.until=0;}duckUntil=0;}
        internal void Clear(){Stop();next.Clear();}
    }
    public sealed partial class CombatEffects
    {
        CombatAudio sound;
        bool soundMuted;
        public bool muted {get=>soundMuted;set{soundMuted=value;if(value)sound?.Stop();}}
        internal CombatAudio Sound=>sound;
        public void Cue(string name){if(!disabled)sound?.Play(name,muted,CaptureSound);}
        public void MixSound(){if(muted)sound?.Stop();else sound?.Mix();}
        public void Play(int kind)=>Cue(kind==0?"fist":kind==1?"collision":kind==2?"force":"armor");
        public void PlayImpact(ImpactKind kind)=>Cue(kind==ImpactKind.Slash?"slash":kind==ImpactKind.Crush?"hammer":kind==ImpactKind.Electric?"electric":kind==ImpactKind.Bullet?"gun":kind==ImpactKind.Force||kind==ImpactKind.Pull?"force":"fist");
    }
}
