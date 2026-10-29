using System;
using System.Collections.Generic;

namespace VectorGame
{
    public enum UpgradeId { FluxBracers, FaultLine, ServoTuning, DenseKnuckles, FieldCapacitor, RepairWeave }

    public sealed class UpgradeCard
    {
        public readonly UpgradeId id;
        public readonly string title, family, description, preview;
        public UpgradeCard(UpgradeId id,string title,string family,string description,string preview)
        {this.id=id;this.title=title;this.family=family;this.description=description;this.preview=preview;}
    }

    // Draft randomness is separate from the encounter: browsing offers never changes a wave's seed.
    public sealed class UpgradeDraft
    {
        public static readonly UpgradeCard[] Catalog={
            new UpgradeCard(UpgradeId.FluxBracers,"Flux Bracers","TECH / LAUNCH",
                "Your third strike launches one enemy through the crowd, even if the strike kills them.",
                "BEFORE: a short shove.\nAFTER: a fast, long launch.\nUnlocks Fault Line at the next checkpoint."),
            new UpgradeCard(UpgradeId.FaultLine,"Fault Line","BRIDGE / HUMAN PINBALL",
                "A launched body deals 26 damage and staggers the first two enemies it hits for 1 second.",
                "Finisher > launch > collision.\nGold trails show an armed body.\nCollisions cannot trigger more launches."),
            new UpgradeCard(UpgradeId.ServoTuning,"Servo Tuning","STAT / TEMPO",
                "+20% strike speed. Finish all three hits sooner and spend less time recovering.",
                "Speeds up the full combo.\nEach copy adds another 20%.\nMore finishers also means more launches."),
            new UpgradeCard(UpgradeId.DenseKnuckles,"Dense Knuckles","STAT / IMPACT",
                "+25% damage to jabs, crosses, and finishers. Fists remain your main weapon.",
                "Each copy adds another 25%.\nDoes not change field or collision damage.\nKill sooner to prevent a backlog."),
            new UpgradeCard(UpgradeId.FieldCapacitor,"Field Capacitor","TECH / CONTROL",
                "Force push recharges 20% faster. Keep creating space as enemies close in.",
                "6.0s recharge becomes 4.8s.\nA second copy brings it to 3.6s.\nThe field still fires automatically."),
            new UpgradeCard(UpgradeId.RepairWeave,"Repair Weave","RECOVERY / INTEGRITY",
                "Restore 55 integrity now, up to your maximum. Trade a new offensive tool for breathing room.",
                "Healing is immediate on installation.\nNo passive regeneration.\nThe remaining crowd carries forward.")
        };
        readonly Dictionary<UpgradeId,int> owned=new Dictionary<UpgradeId,int>();
        readonly Random random;
        public readonly List<UpgradeId> History=new List<UpgradeId>();
        public UpgradeCard[] Offers {get;private set;}=Array.Empty<UpgradeCard>();
        public int Rerolls {get;private set;}
        public int UsedRerolls {get;private set;}
        public int Selected {get;private set;}=-1;
        public float Speed=>1+.2f*Count(UpgradeId.ServoTuning);
        public float Damage=>1+.25f*Count(UpgradeId.DenseKnuckles);
        public float FieldCooldown=>1-.2f*Count(UpgradeId.FieldCapacitor);
        public bool Launch=>Count(UpgradeId.FluxBracers)>0;
        public bool Pinball=>Count(UpgradeId.FaultLine)>0;
        public UpgradeDraft(int seed){random=new Random(seed^0x5f3759df);}
        public int Count(UpgradeId id)=>owned.TryGetValue(id,out int count)?count:0;
        public string Loadout=>History.Count==0?"FISTS + KINETIC FIELD":string.Join("  /  ",History.ConvertAll(id=>Catalog[(int)id].title));
        public void Open(){Rerolls++;Roll();}
        void Roll()
        {
            var previous=Offers;
            var candidates=new List<UpgradeCard>();
            foreach(var card in Catalog)
                if(card.id!=UpgradeId.FluxBracers&&card.id!=UpgradeId.FaultLine&&Count(card.id)<2)candidates.Add(card);
            for(int i=candidates.Count-1;i>0;i--){int j=random.Next(i+1);var temp=candidates[i];candidates[i]=candidates[j];candidates[j]=temp;}
            // Keep the featured path available in this guided slice; refresh supporting offers.
            if(previous.Length==3&&candidates.Count>2&&
                ((candidates[0].id==previous[1].id&&candidates[1].id==previous[2].id)||
                 (candidates[0].id==previous[2].id&&candidates[1].id==previous[1].id)))
            {var temp=candidates[0];candidates[0]=candidates[2];candidates[2]=temp;}
            Offers=new[]{Catalog[(int)(Launch?UpgradeId.FaultLine:UpgradeId.FluxBracers)],candidates[0],candidates[1]};Selected=-1;
        }
        public bool Reroll(){if(Rerolls<=0)return false;Rerolls--;UsedRerolls++;Roll();return true;}
        public void Select(int index){if(index>=0&&index<Offers.Length)Selected=index;}
        public UpgradeCard Install()
        {
            if(Selected<0||Selected>=Offers.Length)return null;
            var card=Offers[Selected];owned[card.id]=Count(card.id)+1;History.Add(card.id);Selected=-1;Offers=Array.Empty<UpgradeCard>();return card;
        }
    }
}
