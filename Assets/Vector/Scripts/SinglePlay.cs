using System;
using System.IO;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        bool legacyReviewAccess,retiredCheckpoint;
        public void Play()=>OpenOffice();
        public void RestartJourney(){if(CombatPreviewActive){Begin();return;}StartChapter();}
        public void HandleJourneyKey(KeyCode key)
        {
            if(OfficeOpen)return;
            if(key==KeyCode.Return&&(State==RunState.Ready||State==RunState.Complete||State==RunState.Defeated))Play();
            else if(key==KeyCode.R){if(State==RunState.Ready)Play();else RestartJourney();}
            else if(key==KeyCode.T&&State==RunState.Ready)BeginTutorial();
        }
        void RetireOldCheckpoint()
        {
            retiredCheckpoint=false;if(legacyReviewAccess||!File.Exists(SavePath))return;
            try
            {
                var saved=ReadCheckpoint();if(saved.chapterMode&&saved.chapter?.definition==LongJourney.Id)return;
                string retired=Path.Combine(SaveDirectory,"RetiredRuns");Directory.CreateDirectory(retired);
                string name="checkpoint-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N");
                File.Move(SavePath,Path.Combine(retired,name+".json"));
                if(File.Exists(SavePath+".bak"))File.Move(SavePath+".bak",Path.Combine(retired,name+".bak"));
                SaveMessage="Earlier run archived. Records and Office progress are kept.";
            }
            catch(Exception){retiredCheckpoint=true;SaveMessage="Earlier save is preserved. Start a new journey in the Office.";}
        }
    }
    public sealed partial class PrototypeHUD
    {
        internal Rect PlayButtonRect(float w,float h)=>new Rect(w/2-250,Mathf.Max(30,(h-600)/2)+280,500,55);
        internal bool HandlePlayPointer(EventType type,Vector2 point,float w,float h)
        {if(type!=EventType.MouseDown||game.State!=VectorEncounter.RunState.Ready||game.OfficeOpen||!PlayButtonRect(w,h).Contains(point))return false;game.Play();return true;}
    }
}
