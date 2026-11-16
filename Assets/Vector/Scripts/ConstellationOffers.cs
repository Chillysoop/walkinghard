using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorGame
{
    public sealed partial class AbilityDeck
    {
        // A larger unlocked pool must still offer a usable first attack and build support.
        // No extra picks, fixed set, rarity change, or removal of unlocked discoveries.
        static readonly bool originalPatternOffers=Array.IndexOf(Environment.GetCommandLineArgs(),"-originalPatternOffers")>=0;
        bool ExpandedPatternPool=>!originalPatternOffers&&ConstellationContent.All.Any(RewardAvailable);
        static bool FoundationAttack(ItemDefinition def)=>def.id=="SteelJab"||def.id=="Breaker"||def.id=="ArcBolt"||def.id=="ReapingCut";
        void CuratePatternAbilities(List<RewardOffer> choices)
        {
            if(!ExpandedPatternPool||choices.Count<3||choices.Take(3).Any(o=>FoundationAttack(o.def)))return;
            int index=choices.FindIndex(o=>FoundationAttack(o.def));if(index<3)return;
            var swap=choices[2];choices[2]=choices[index];choices[index]=swap;
        }
        ItemDefinition PatternSupport(List<ItemDefinition> support)
        {
            if(!ExpandedPatternPool)return null;
            return support.FirstOrDefault(d=>BuildSynergies.Find(this,d).Any(link=>!link.text.StartsWith("Needs",StringComparison.Ordinal)));
        }
    }
}
