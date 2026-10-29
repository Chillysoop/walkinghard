using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void ResolveBasic(LoadoutItem weapon,AbilityCard card,Enemy target,bool left,bool followup=false)
        {
            if(target==null||target.deathAge>=0||!Reachable(target,card.range+.3f))target=Target(card.range);
            if(target==null)return;
            int rank=weapon.rank;WeaponKind kind=weapon.def.weapon;
            float damage=card.damage*Deck.Damage;Vector3 origin=left?hero.LeftHand.position:hero.RightHand.position;
            Color color=kind==WeaponKind.Hammer?new Color(1,.72f,.25f):kind==WeaponKind.Sword?new Color(.7f,1,.9f):VisualFactory.Cyan;
            Vector3 aim=target.physics.Center-(HeroPosition+Vector3.up);aim.y=0;aim.Normalize();
            if(kind==WeaponKind.Fists)
            {
                fx.Arc(origin,target.physics.Center,color);fx.Burst(target.physics.Center,color,6,3);fx.Play(0);
                Vector3 point=target.physics.Center;Hurt(target,damage,aim*1.2f,.5f);WeaponHits[(int)kind]++;
                if(rank>=3&&followup)
                {
                    foreach(var e in enemies.ToArray())if(e!=target&&Vector3.Distance(e.physics.Center,point)<2.4f)
                        Hurt(e,damage*.5f,(e.physics.Center-point).normalized*4,.8f);
                    fx.Ring(point,2.4f,color);
                }
                ImpactPause(.025f);return;
            }
            if(kind==WeaponKind.Sword)
            {
                var swordChain=new Cascade();
                foreach(var e in enemies.ToArray())
                {
                    if(!Reachable(e,card.range))continue;
                    Vector3 delta=e.physics.Center-HeroPosition;delta.y=0;
                    if(Vector3.Angle(aim,delta)>45+(rank-1)*22)continue;
                    bool lethal=e.health<=damage;Hurt(e,damage,aim*3,.5f,true);WeaponHits[(int)kind]++;
                    fx.Arc(origin,e.physics.Center,color);fx.Burst(e.physics.Center,color,5,3);
                    if(lethal&&rank>=3)Launch(e,swordChain);
                }
                fx.Play(1);ImpactPause(.055f);return;
            }
            if(kind==WeaponKind.Hammer)
            {
                var victims=new List<Enemy>{target};
                if(rank>=3)foreach(var e in enemies)if(e!=target&&victims.Count<3&&Reachable(e,card.range)&&Vector3.Distance(e.physics.Center,target.physics.Center)<2.5f)victims.Add(e);
                var chain=new Cascade();
                foreach(var e in victims){Hurt(e,damage,Vector3.zero,.8f);Launch(e,chain);WeaponHits[(int)kind]++;fx.Burst(e.physics.Center,color,10,4);}
                if(rank>=2)foreach(var e in enemies.ToArray())if(Reachable(e,4.3f)){e.stun=Mathf.Max(e.stun,1);e.attack=-1;}
                fx.Ring(HeroPosition,rank>=2?4.3f:2,color);fx.Play(1);ImpactPause(.075f);shake=.25f;return;
            }
            if(Ammo(weapon)<=0)return;ammunition[weapon]--;fx.Play(0);
            if(kind==WeaponKind.Pistol)
            {
                var hit=new HashSet<int>();Vector3 from=origin;var current=target;
                for(int i=0;i<Mathf.Min(rank,3)&&current!=null;i++)
                {
                    var point=current.physics.Center;hit.Add(current.id);fx.Arc(from,point,color);fx.Burst(point,color,4,2);
                    Hurt(current,damage*(i==0?1:.65f),aim*.7f,rank>=3?.8f:.2f);WeaponHits[(int)kind]++;from=point;
                    current=null;float distance=4.2f;
                    foreach(var e in enemies)if(!hit.Contains(e.id)&&Reachable(e,card.range+3)){float d=Vector3.Distance(from,e.physics.Center);if(d<distance){distance=d;current=e;}}
                }
            }
            else
            {
                var lineTargets=new List<Enemy>();Vector3 start=HeroPosition+Vector3.up;
                foreach(var e in enemies)
                {
                    if(!Reachable(e,card.range))continue;Vector3 d=e.physics.Center-start;d.y=0;float along=Vector3.Dot(d,aim);
                    if(along>=0&&(d-aim*along).magnitude<.7f)lineTargets.Add(e);
                }
                lineTargets.Sort((a,b)=>Vector3.Distance(a.physics.Center,start).CompareTo(Vector3.Distance(b.physics.Center,start)));
                int count=Mathf.Min(lineTargets.Count,Mathf.Min(rank+1,4));
                for(int i=0;i<count;i++){var e=lineTargets[i];Hurt(e,damage,aim*2,.3f);WeaponHits[(int)kind]++;if(rank>=3&&i==count-1)Launch(e);}
                fx.Arc(origin,start+aim*card.range,color);fx.Burst(origin,color,7,2);ImpactPause(.035f);
            }
        }
        void PushCorpses(float range,Cascade shared=null)
        {
            var chain=shared??new Cascade();int count=0;
            foreach(var e in corpses)
                if(count<6&&Vector3.Distance(e.physics.Center,HeroPosition)<range){Launch(e,chain);count++;}
        }
        void ResolveUltimate()
        {
            if(Deck.Ultimate.def.id=="tempest")
            {
                var hit=new HashSet<int>();var target=Target(12);Vector3 origin=hero.RightHand.position;
                for(int i=0;i<7&&target!=null;i++)
                {
                    Vector3 point=target.physics.Center;hit.Add(target.id);TempestHits++;fx.Arc(origin,point,new Color(.8f,.6f,1));
                    Hurt(target,(38+(target.stun>0?12:0))*Deck.Damage*ItemRarity.Power[Deck.Ultimate.rank],Vector3.zero,.45f);
                    target=null;float near=5;foreach(var e in enemies)if(!hit.Contains(e.id)&&Reachable(e,16)){float d=Vector3.Distance(e.physics.Center,point);if(d<near){near=d;target=e;}}origin=point;
                }
                fx.Play(2);ImpactPause(.07f);return;
            }
            var chain=new Cascade();int launched=0;
            foreach(var e in enemies.ToArray())
            {
                if(!Reachable(e,LoadoutCatalog.Ultimate.range))continue;
                var direction=e.physics.Center-HeroPosition;direction.y=0;direction.Normalize();
                Hurt(e,LoadoutCatalog.Ultimate.damage*Deck.Damage*ItemRarity.Power[Deck.Ultimate.rank],direction*5,1.2f);
                if(launched++<6)Launch(e,chain);
            }
            PushCorpses(LoadoutCatalog.Ultimate.range,chain);
            fx.Ring(HeroPosition,8.5f,new Color(.7f,.6f,1),.75f);fx.Play(2);shake=.3f;ImpactPause(.085f);
        }
    }
}
