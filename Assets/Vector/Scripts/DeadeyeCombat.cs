using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        LoadoutItem recoilOwner;
        float recoilUntil,recoilGuard,recoilShot;
        public string RecoilState=>Deck.Equipped(Signature.Recoil)!=recoilOwner?"":recoilShot>0?"RECOIL SHOT READY":recoilGuard>0&&Elapsed<recoilUntil?"RECOIL GUARD":"";
        void ResetDeadeye(){recoilOwner=null;recoilUntil=recoilGuard=recoilShot=0;}
        void ArmRecoil()
        {
            var host=Deck.Equipped(Signature.Recoil);if(host==null||!Once("recoil-reload"))return;
            if(recoilOwner!=host)recoilShot=0;
            recoilOwner=host;recoilGuard=LoadoutCatalog.RecoilBlock(host);recoilUntil=Elapsed+host.def.effect.Seconds(host.rank);
            Note("Reload armed Recoil guard");
        }
        float BlockRecoil(float damage)
        {
            if(damage<=0||recoilGuard<=0||Elapsed>=recoilUntil||Deck.Equipped(Signature.Recoil)!=recoilOwner)return damage;
            float blocked=Mathf.Min(damage,recoilGuard);recoilGuard=0;
            ContributionFor(recoilOwner).absorption+=blocked;recoilShot=LoadoutCatalog.RecoilDamage(recoilOwner);
            Note("Recoil guard blocked damage");SynergyCue("guard","RELOAD GUARD","Block → empowered shot");
            return damage-blocked;
        }
        void SpendRecoil(Enemy target)
        {
            // A lethal base hit leaves both the charge and this turn's proc available.
            // A later piercing/ricochet contact or the other hand can spend it once.
            if(target.deathAge>=0||recoilShot<=0||LoadoutCatalog.Magazine(currentCredit?.weapon)<=0||Deck.Equipped(Signature.Recoil)!=recoilOwner||!Once("recoil-shot"))return;
            float bonus=recoilShot;recoilShot=0;
            var saved=currentCredit;currentCredit=new KillCredit{item=recoilOwner,action=saved.action,derived=true};
            float before=target.health;
            DerivedHit(target,bonus,Vector3.zero,0,false,false,ImpactKind.Bullet);
            currentCredit=saved;
            if(target.health<before){Note("Recoil empowered a gun hit");SynergyCue("burst","RECOIL SHOT","Stored guard → bonus damage");fx.Arc(hero.RightHand.position,target.physics.Center,Color.white);}
        }
        void ResolveSuppressiveVolley(LoadoutItem host,AbilityCard card,Enemy primary,float damage)
        {
            if(primary==null)return;
            Vector3 aim=primary.rig.Root.position-HeroPosition;aim.y=0;aim.Normalize();
            int cap=host.HasAugment(AugmentId.Focused)?1:3;
            float stagger=LoadoutCatalog.VolleyStagger(host)*(cap==1?1.5f:1);
            foreach(var e in enemies.Where(e=>e.deathAge<0&&Reachable(e,card.range)&&InCone(e,HeroPosition,aim,card.range,12+2*(host.rank-1))).OrderBy(e=>e==primary?-1:Vector3.Distance(e.rig.Root.position,HeroPosition)).Take(cap).ToArray())
            {
                bool interrupted=e.attack>=0;
                fx.Arc(hero.RightHand.position,e.physics.Center,Color.white);fx.Burst(e.physics.Center,VisualFactory.Cyan,4,1);
                Hurt(e,damage,Vector3.zero,stagger,false,ImpactKind.Bullet);
                if(interrupted)Note("Suppressive Volley interrupted an attack");
            }
            fx.Play(1);
        }
    }
}
