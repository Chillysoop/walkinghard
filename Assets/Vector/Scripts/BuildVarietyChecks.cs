using System;
using System.Linq;

namespace VectorGame
{
    public static class BuildVarietyChecks
    {
        static void Check(bool value,string rule){if(!value)throw new InvalidOperationException("Build variety: "+rule);}
        public static void Run()
        {
            Check(LoadoutCatalog.All.Count()==40&&LoadoutCatalog.All.Select(d=>d.id).Distinct().Count()==40,"40 unique identities");
            Check(AugmentCatalog.All.Length==16,"16 modules");
            Check(Enumerable.Range(1,7).SelectMany(VectorEncounter.RewardsAfter).Count(d=>d!=DraftKind.Augment)+2==13,"13 item choices");
            Check(Enumerable.Range(1,7).SelectMany(VectorEncounter.RewardsAfter).Count(d=>d==DraftKind.Augment)==2,"2 augment choices");
            foreach(var module in AugmentCatalog.All)
            {
                var candidates=LoadoutCatalog.All.Where(module.module.Fits).ToArray();Check(candidates.Length>0,"each module has hosts");
                Check(candidates.All(d=>d.kind==ItemKind.Ability||d.kind==ItemKind.Weapon),"no armor/ultimate sockets");
                Check(module.tradeoff.Length>0&&module.description.Length>0,"each module declares benefit and cost");
                var deck=new AbilityDeck(44);var host=deck.Acquire(candidates[0],2);var mod=deck.Acquire(module);
                Check(deck.Attach(mod,host)&&!deck.Inventory.Contains(mod),"attached module is not duplicated in storage");
                var replacement=deck.Acquire(module);Check(deck.Attach(replacement,host)&&deck.Inventory.Contains(mod),"replacement returns old module");
                var spare=deck.Acquire(candidates[0],2);Check(deck.Attach(mod,spare)&&host.augment==replacement,"separate copies have independent sockets");
                if(host.def.kind==ItemKind.Weapon)deck.Move(host,PlaceKind.Weapon,0);else deck.Move(host,PlaceKind.Sequence,2);
                deck.Acquire(candidates[0],2);Check(deck.Items.Contains(host)&&host.rank==3&&host.augment==replacement&&deck.Inventory.Contains(mod),"fusion retains keeper module and returns consumed host module");
                Check(deck.Move(replacement,PlaceKind.Inventory)&&host.augment==null,"remove module freely");
                Check(deck.Attach(mod,host)&&deck.ModuleHost(mod)==host,"transfer module freely");
                var count=deck.Items.Count(i=>i.def==module);deck.Acquire(module);deck.Acquire(module);Check(deck.Items.Count(i=>i.def==module)==count+2,"modules never fuse");
            }
            var fists=new AbilityDeck(8);var flurry=fists.Acquire(AugmentCatalog.Get(AugmentId.Flurry));fists.Attach(flurry,fists.Fists);
            Check(fists.WeaponFor(fists.Basics[0])==fists.WeaponFor(fists.Basics[1])&&fists.Hosts.Count(h=>h.augment==flurry)==1,"shared starter fists one host/socket");
            for(int seed=1;seed<=100;seed++)
            {
                var d=new AbilityDeck(seed);foreach(var def in LoadoutCatalog.Abilities.Take(3))d.Acquire(def);
                for(int draft=0;draft<2;draft++)
                {
                    d.Open(DraftKind.Augment,false);Check(d.Offers.Length==3&&d.Offers.Select(o=>o.def).Distinct().Count()==3,"three distinct compatible module offers");
                    Check(d.Offers.Any(o=>o.def.module.family<0)&&d.Offers.Any(o=>o.def.module.family>=0),"ability + weapon + either");
                    foreach(var offer in d.Offers)Check(d.CompatibleHosts(offer.def).Any(),"no unusable offer");d.ClaimTo(draft,PlaceKind.Inventory);
                }
            }
            var two=new AbilityDeck(3);var hammer=two.Acquire(LoadoutCatalog.Find("hammer"));two.Move(hammer,PlaceKind.Weapon,0);var pile=two.Acquire(AugmentCatalog.Get(AugmentId.Piledriver));two.Attach(pile,hammer);
            Check(two.Weapons[0]==two.Weapons[1]&&two.Hosts.Count(h=>h.augment==pile)==1,"2H one socket");
        }
    }
}
