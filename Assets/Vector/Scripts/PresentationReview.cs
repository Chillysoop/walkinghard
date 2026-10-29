using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter { internal bool PresentationFixtureReady=>qaHolding; }
    public sealed partial class PrototypeHUD
    {
        LoadoutItem presentationHover;
        RewardOffer presentationOffer;
        void ApplyPresentationReviewHover()
        {if(presentationHover!=null){hoveredItem=presentationHover;hoveredOffer=presentationOffer;detailHovered=false;}}
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-presentationReview")<0)yield break;
            string directory=Path.Combine(Application.dataPath,"..","PresentationReview");Directory.CreateDirectory(directory);
            foreach(var def in LoadoutCatalog.All.Concat(AugmentCatalog.All))if(!ItemArtwork.Has(def.id))throw new InvalidOperationException("Missing artwork mapping: "+def.id);
            foreach(string name in new[]{"Equipment","Actions","Augments"})if(Resources.Load<Texture2D>("ItemArt/"+name)==null)throw new InvalidOperationException("Missing atlas: "+name);
            while(!game.PresentationFixtureReady){game.SetCombatSpeed(4);yield return null;}
            // QADraft configures the held UI fixture on its next frame.
            for(int i=0;i<8;i++)yield return null;
            var d=game.Deck;d.Acquire(LoadoutCatalog.Find("visor"),3);d.Acquire(LoadoutCatalog.Find("capacitor"),5);
            foreach(string id in new[]{"wraps","greatsword","scattergun","grounding","loader","arcstep","relay","redline","Counterguard","ConductiveBrand"})d.Acquire(LoadoutCatalog.Find(id),2);
            d.Acquire(AugmentCatalog.Get(AugmentId.Echo));d.Acquire(AugmentCatalog.Get(AugmentId.Crosscut));
            yield return CapturePresentation(directory,"01-checkpoint");
            presentationHover=d.Items.First(i=>i.def.id=="visor"&&i.rank==1);
            yield return CapturePresentation(directory,"02-rarity-mismatch");
            presentationOffer=d.Offers[0];presentationHover=new LoadoutItem(presentationOffer.def,presentationOffer.rank);
            yield return CapturePresentation(directory,"03-fusion-preview");
            presentationOffer=null;presentationHover=d.Items.First(i=>i.def.kind==ItemKind.Augment);
            yield return CapturePresentation(directory,"04-augment-details");
            presentationHover=null;detailItem=null;iconGuide=true;
            yield return CapturePresentation(directory,"05-icon-key");
            iconGuide=false;game.ClaimReward(0,PlaceKind.Helmet,0,false);fusionPinned=true;
            yield return CapturePresentation(directory,"06-fused");
            fusionPinned=false;shownFusion=null;seenFusions=game.FusionHistory.Count;
            game.FusionHistory.Clear();game.ConfirmDraft();game.SetCombatSpeed(1);
            fusionDeck=game.Deck;seenFusions=0;shownFusion=null;
            yield return CapturePresentation(directory,"07-combat");
            Debug.Log("PRESENTATION_REVIEW_SUCCESS: 56 mapped definitions; native UI captures saved.");Application.Quit(0);
        }
        IEnumerator CapturePresentation(string directory,string name)
        {
            for(int i=0;i<12;i++)yield return null;
            string path=Path.Combine(directory,name+".png");ScreenCapture.CaptureScreenshot(path);
            for(int i=0;i<60&&!File.Exists(path);i++)yield return null;
            if(!File.Exists(path))throw new InvalidOperationException("Native UI screenshot was not saved: "+name);
        }
    }
}
