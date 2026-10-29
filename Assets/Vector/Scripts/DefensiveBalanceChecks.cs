using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckDefensiveBalance()
        {
            foreach(int rank in new[]{1,2,3,4,5})
            {
                ProbeReset();var guard=ProbeHost(LoadoutCatalog.Find("Counterguard"),null,rank);Resolve(activeCard);
                float until=Elapsed+2.5f*Effect(guard);
                MechanicRequire(Mathf.Abs(barriers[0].expires-until)<.001f,"Counterguard shortened lifetime scales with rarity");
                Elapsed=until-.01f;StepSignatures(.01f);MechanicRequire(BarrierAmount>0,"Counterguard remains active before expiry");
                Elapsed=until+.01f;StepSignatures(.02f);MechanicRequire(BarrierAmount==0&&counterBank==0,"unused Counterguard expires without free bank damage");

                ProbeReset();var gun=ProbeHost(LoadoutCatalog.Find("pistol"));var jacket=Deck.Acquire(LoadoutCatalog.Find("duelist"),rank);Deck.Move(jacket,PlaceKind.Armor);
                ammunition[gun]=1;Replenish(gun);
                MechanicRequire(DuelistHitsRemaining==3&&Mathf.Abs(duelistUntil-Elapsed-6*(1+.1f*(rank-1)))<.001f,"real reload arms three rarity-scaled Duelist charges");
                var first=ProbeEnemy(0,2);var second=ProbeEnemy(1,2);float health=Health,mitigation=Mathf.Min(.65f,.45f*(1+.15f*(rank-1)));
                ReceiveDamage(first,20);ReceiveDamage(second,20);ReceiveDamage(second,20);
                MechanicRequire(Mathf.Abs(health-Health-60*(1-mitigation))<.001f&&DuelistHitsRemaining==0,"three ward hits retain mitigation and cap");
                MechanicRequire(first.stun>0&&second.stun==0,"only the first mitigated hit staggers");
                health=Health;ReceiveDamage(second,20);MechanicRequire(Mathf.Abs(health-Health-20)<.001f,"fourth hit is not warded");
            }
            ProbeReset();var weapon=ProbeHost(LoadoutCatalog.Find("pistol"));var armor=Deck.Acquire(LoadoutCatalog.Find("duelist"));Deck.Move(armor,PlaceKind.Armor);ammunition[weapon]=0;Replenish(weapon);
            var attacker=ProbeEnemy(0,2);var barrier=Deck.Acquire(LoadoutCatalog.Find("Counterguard"));AddBarrier(barrier,20,1);
            ReceiveDamage(attacker,20);MechanicRequire(DuelistHitsRemaining==3&&attacker.stun==0,"fully absorbed hit preserves charges and first-hit stagger");
            ReceiveDamage(attacker,10);MechanicRequire(DuelistHitsRemaining==2,"first damaging hit consumes one charge");
            float expires=duelistUntil;Replenish(weapon);MechanicRequire(DuelistHitsRemaining==2&&duelistUntil==expires,"full magazine does not refresh ward");
            ammunition[weapon]=0;Replenish(weapon);MechanicRequire(DuelistHitsRemaining==3,"qualifying refresh replaces charges rather than adding them");
            // Isolate the sever trigger from the earlier Counterguard bank's area damage.
            counterBank=0;var sword=ProbeHost(LoadoutCatalog.Find("sword"));var victim=ProbeEnemy(0,2,5);ReceiveDamage(attacker,10);ResolveBasic(sword,activeCard,victim,false);
            MechanicRequire(victim.physics.Severed&&DuelistHitsRemaining==3,"lethal sever also refreshes three charges");
            Elapsed=duelistUntil+.01f;float before=Health;ReceiveDamage(attacker,10);
            MechanicRequire(DuelistHitsRemaining==0&&Mathf.Abs(before-Health-10)<.001f,"unused ward expires after six seconds");
            ProbeReset();MechanicRequire(DuelistHitsRemaining==0,"new run cannot retain Duelist charges");
        }
    }
}
