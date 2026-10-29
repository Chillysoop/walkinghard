using System.Collections.Generic;

namespace VectorGame
{
    // Only combat simulation time advances this meter. Checkpoints and pauses cannot expire a combo.
    public sealed class ComboScore
    {
        public const float Window=3f;
        public int Banked {get;private set;}
        public int Base {get;private set;}
        public int Kills {get;private set;}
        public int Bulk {get;private set;}
        public int ChainDepth {get;private set;}
        public int Best {get;private set;}
        public int BestMultiplier {get;private set;}=1;
        public float Remaining {get;private set;}
        public int Multiplier=>System.Math.Min(10,1+Bulk+ChainDepth+System.Math.Max(0,Kills-1)/3);
        public int Pending=>Base*Multiplier;
        public string Reason {get;private set;}="";
        readonly HashSet<int> events=new HashSet<int>();
        public void Kill(int points,int action,int chainHop=0)
        {
            Kills++;Base+=points;Remaining=Window;
            bool bulk=!events.Add(action);if(bulk)Bulk++;
            ChainDepth=System.Math.Max(ChainDepth,chainHop);
            Reason=chainHop>0?"CHAIN KILL":bulk?"MULTI-KILL":Kills>1?"KILL STREAK":"COMBO START";
            BestMultiplier=System.Math.Max(BestMultiplier,Multiplier);
        }
        public void Step(float dt){if(Kills==0)return;Remaining=System.Math.Max(0,Remaining-dt);if(Remaining<=0)Bank();}
        public void Bank(){if(Kills==0)return;Best=System.Math.Max(Best,Pending);Banked+=Pending;Base=Kills=Bulk=ChainDepth=0;Remaining=0;events.Clear();}
    }
}
