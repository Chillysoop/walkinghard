using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void ResolveBasic(LoadoutItem weapon,AbilityCard card,Enemy target,bool left,bool followup=false)=>ResolveAuthoredBasic(weapon,card,target,left,followup);
        void PushCorpses(float range,Cascade shared=null)
        {
            var chain=shared??new Cascade();int count=0;
            foreach(var e in corpses)
                if(count<6&&Vector3.Distance(e.physics.Center,HeroPosition)<range){Launch(e,chain);count++;}
        }
        void ResolveUltimate()
        {
            var ult=Deck.Ultimate;
            if(ult.def.id=="redline"){foreach(var w in new[]{Deck.WeaponFor(Deck.Basics[0]),Deck.WeaponFor(Deck.Basics[1])})Replenish(w);redlineTurns=3;fx.Shape(HeroPosition,Vector3.forward,1.5f,180,Color.magenta,.6f);return;}
            if(ult.def.id=="cyclone"){float range=LoadoutCatalog.Action(ult).range;fx.Sweep(HeroPosition,Vector3.forward,range,180,Color.white,.5f,true);foreach(var e in enemies.ToArray())if(Reachable(e,range)){var outward=e.rig.Root.position-HeroPosition;outward.y=0;Hurt(e,LoadoutCatalog.Action(ult).damage*Deck.Damage,outward.normalized*4*Effect(ult),.4f,true);}return;}

            if(Deck.Ultimate.def.id=="tempest")
            {
                var hit=new HashSet<int>();var target=Target(12);Vector3 origin=hero.RightHand.position;
                for(int i=0;i<7+(ult.rank-1)/2&&target!=null;i++)
                {
                    Vector3 point=target.physics.Center;hit.Add(target.id);TempestHits++;fx.Arc(origin,point,new Color(.8f,.6f,1));
                    ElectricHit(target,(LoadoutCatalog.Action(ult).damage+(target.stun>0?12*Effect(ult):0))*Deck.Damage,.45f);
                    target=null;float near=6+.3f*(ult.rank-1);foreach(var e in enemies)if(!hit.Contains(e.id)&&Reachable(e,16)){float d=Vector3.Distance(e.physics.Center,point);if(d<near){near=d;target=e;}}origin=point;
                }
                fx.Play(2);ImpactPause(.07f);return;
            }
            var chain=new Cascade();int launched=0;
            foreach(var e in enemies.ToArray())
            {
                if(!Reachable(e,LoadoutCatalog.Action(ult).range))continue;
                var direction=e.physics.Center-HeroPosition;direction.y=0;direction.Normalize();
                Hurt(e,LoadoutCatalog.Action(ult).damage*Deck.Damage,direction*5,1.2f);
                if(launched++<6)Launch(e,chain,0,Effect(ult));
            }
            PushCorpses(LoadoutCatalog.Action(ult).range,chain);
            fx.Ring(HeroPosition,LoadoutCatalog.Action(ult).range,new Color(.7f,.6f,1),.75f);fx.Play(2);shake=.3f;ImpactPause(.085f);
        }
    }
}
