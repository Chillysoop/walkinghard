using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        // Short effect copy is shared by offers and both sequence views. Detailed
        // rules, scaling and tradeoffs remain in the existing hover descriptions.
        string ItemSummary(LoadoutItem item)
        {
            if(item==null)return "Empty; skipped in combat.";
            if(item.def.kind==ItemKind.Basic)item=game.Deck.WeaponFor(item);
            if(item.augment!=null)switch(item.augment.def.module.id)
            {
                case AugmentId.FollowThrough:return "Strike; next basic gains stagger.";
                case AugmentId.Focused:return item.def.ability.id==AbilityId.ArcBolt?"Stronger lightning; fewer targets.":item.def.ability.id==AbilityId.ConductiveBrand?"Stronger marks; fewer targets.":item.def.ability.id==AbilityId.ReapingCut?"Stronger cut; narrower arc.":"Stronger stagger; narrower arc.";
                case AugmentId.Echo:return item.def.ability.id==AbilityId.ArcBolt?"Chain lightning, then repeat a hit.":item.def.ability.id==AbilityId.ReapingCut?"Cleave, then repeat a weaker cut.":"Punch, then repeat a weaker hit.";
                case AugmentId.Afterfield:return item.def.ability.id==AbilityId.ConductiveBrand?"Mark foes in a lingering field.":item.def.ability.id==AbilityId.VectorTether?"Line foes up; leave a pull field.":"Gather foes in a lingering field.";
                case AugmentId.SplitPayload:return "Launch two foes with less force.";
                case AugmentId.Backwash:return "Push foes, then pull survivors back.";
                case AugmentId.Discharge:return "Absorb hits, then burst forward.";
                case AugmentId.Anchor:return "Pull foes; moved targets stagger.";
                case AugmentId.Flurry:return "Punch, then two weaker follow-ups.";
                case AugmentId.PalmDrive:return "Punch and shove the first target.";
                case AugmentId.Crosscut:return "Cut twice; killing blows sever.";
                case AugmentId.HookedEdge:return "Cut and pull the first survivor.";
                case AugmentId.Groundbreaker:return "Smash; ground wave staggers foes.";
                case AugmentId.Piledriver:return "Launch one foe with extra force.";
                case AugmentId.BoreRounds:return "Aim longer; shots pierce a line.";
                case AugmentId.BreachLoad:return "Shoot; reload arms a stagger burst.";
            }
            switch(item.def.id)
            {
                case "SteelJab":return "Quick punch; interrupts attacks.";
                case "Breaker":return "Palm strike staggers a wide cone.";
                case "ForcePush":return "Push enemies and bodies away.";
                case "MassDriver":return "Launch a foe through the crowd.";
                case "GravityWell":return "Pull nearby foes into a cluster.";
                case "ArcBolt":return "Chain lightning between enemies.";
                case "ConductiveBrand":return "Mark foes for bonus lightning damage.";
                case "VectorTether":return "Pull enemies into a straight line.";
                case "ReapingCut":return "Slash weak foes; kills sever.";
                case "Counterguard":return "Absorb hits; empower the next basic.";
                case "fists":return item.rank>=3?"Double punch; stagger shockwave.":item.rank>=2?"Two quick punches that stagger.":"Quick close punch that staggers.";
                case "wraps":return "Punch; mark foes for lightning.";
                case "sword":return item.rank>=3?"Cleave; kills sever and launch.":"Cleave; killing blows sever.";
                case "greatsword":return "Heavy cleave; kills sever and shove.";
                case "breachhammer":return "Smash, stagger and knock forward.";
                case "hammer":return item.rank>=3?"Smash and launch up to three foes.":"Smash and launch a foe into others.";
                case "pistol":return item.rank>=2?"Quick shots ricochet between foes.":"Quick shots; reload after six.";
                case "rifle":return "Pierce a line; reload after four.";
                case "scattergun":return "Short-range blast pushes foes back.";
                case "singularity":return "Launch foes into collision chains.";
                case "tempest":return "Chain lightning; consume marks.";
                case "cyclone":return "Full-circle cut; killing blows sever.";
                case "redline":return "Refill guns; haste for three turns.";
                case "visor":return "Hits spread stagger to a neighbor.";
                case "surveyor":return "Multi-target hits echo extra damage.";
                case "salvager":return "Launch/sever primes a corpse pull.";
                case "capacitor":return "Hitting staggered foes marks them.";
                case "plate":return "Finish a sequence to arm a guard.";
                case "reactive":return "Crowds trigger a repelling barrier.";
                case "grounding":return "Lightning/body hits build a barrier.";
                case "duelist":return "Reload or sever to guard one hit.";
                case "treads":return "First kill each turn drops healing.";
                case "momentum":return "Launching speeds your next ability.";
                case "loader":return "Faster reloads stagger nearby foes.";
                case "arcstep":return "Double kills mark nearby survivors.";
                case "faultline":return "Body impacts trigger stagger waves.";
                case "overclock":return "Speed ramps up through the sequence.";
                case "amplifier":return "Abilities charge the next basic burst.";
                case "relay":return "Body hits discharge marked foes.";
                case "magnet":return "Severed bodies launch into others.";
                case "aug-FollowThrough":return "Trade stagger for a next-basic pulse.";
                case "aug-Focused":return "Stronger effect on fewer targets.";
                case "aug-Echo":return "Weaker opening; one delayed repeat.";
                case "aug-Afterfield":return "Weaker opening leaves a lasting field.";
                case "aug-SplitPayload":return "Launch two bodies with less force.";
                case "aug-Backwash":return "Weaker push, then pull survivors back.";
                case "aug-Discharge":return "Barrier bursts instead of boosting a basic.";
                case "aug-Anchor":return "Weaker pulls also stagger moved foes.";
                case "aug-Flurry":return "Weaker opening; two follow-up punches.";
                case "aug-PalmDrive":return "Trade punch damage for a forward shove.";
                case "aug-Crosscut":return "Weaker swing adds a return cut.";
                case "aug-HookedEdge":return "Narrower cut pulls the first survivor.";
                case "aug-Groundbreaker":return "Less launch; add a ground stagger wave.";
                case "aug-Piledriver":return "Trade splash for one stronger launch.";
                case "aug-BoreRounds":return "Aim longer to pierce lined-up foes.";
                case "aug-BreachLoad":return "Longer reload arms a stagger burst.";
                default:return item.def.description;
            }
        }
        void EffectLine(Rect r,LoadoutItem item,int size=11)
        {
            string text=ItemSummary(item);
            while(size>9&&Style(size).CalcSize(new GUIContent(text)).x>r.width)size--;
            var style=Style(size);bool wrap=style.wordWrap;style.wordWrap=false;
            style.normal.textColor=item==null?muted:white;GUI.Label(r,text,style);style.wordWrap=wrap;
        }
    }
}
