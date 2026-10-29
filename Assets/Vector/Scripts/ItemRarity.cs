using System;

namespace VectorGame
{
    public static class ItemRarity
    {
        public static readonly string[] Names={"","Common","Uncommon","Rare","Epic","Legendary"};
        public static readonly int[] Odds={55,27,12,5,1};
        public static readonly float[] Power={0,1,1.2f,1.5f,1.9f,2.5f};
        public static int Roll(Random random)
        {
            int value=random.Next(100),total=0;
            for(int i=0;i<Odds.Length;i++){total+=Odds[i];if(value<total)return i+1;}
            return 5;
        }
        public static string StatBonus(int rarity)=>rarity==1?"baseline stats":"+"+Math.Round((Power[rarity]-1)*100)+"% power";
    }
}
