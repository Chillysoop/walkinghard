using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        int followMarks;
        AbilityId followKind;
        public string FollowThroughReady=>followOwner==null?"":followMarks>0?(followKind==AbilityId.DrainBrand?"SIPHON READY":"MARKS READY"):followBank>0?"STAGGER READY":"";
        public string FollowThroughKey=>followMarks==0?"stagger":followKind==AbilityId.DrainBrand?"siphon":"mark";
        public bool FollowThroughDestination(int slot)
        {
            if(FollowThroughReady==""||slot<0||slot>=5)return false;
            for(int n=0;n<5;n++){int i=(NextSlot+n)%5;if(Deck.Slots[i]?.def.kind!=ItemKind.Basic)continue;return slot==i||slot==Deck.PairAfter(i);}
            return false;
        }
        void PrepareFollowThrough(LoadoutItem host)
        {
            followOwner=host;followKind=host.def.ability.id;followBank=0;followMarks=0;
            if(followKind==AbilityId.ConductiveBrand)followMarks=3;
            else if(followKind==AbilityId.DrainBrand)followMarks=2+(host.rank-1)/2;
            else followBank=.4f*Effect(host);
            Note("Ability prepared a weapon payoff");
            fx.Symbol(HeroPosition+Vector3.up*1.6f,followKind==AbilityId.DrainBrand?SiphonColor:VisualFactory.Cyan,.4f,true);
        }
        bool ClearLine(Vector3 from,Enemy enemy)=>!enemy.entering&&enemy.deathAge<0&&(FastSimulation||!Physics.Linecast(from+Vector3.up*.8f,enemy.rig.Root.position+Vector3.up*.8f,1<<EntryLayout.WorldLayer));
        void SpendFollowThrough(Vector3 point)
        {
            if(followOwner==null||CurrentTurn.procs.Contains("follow-release"))return;
            var candidates=enemies.Where(e=>Vector3.Distance(e.physics.Center,point)<2.2f&&ClearLine(point-Vector3.up*.8f,e));
            if(followMarks>0&&followKind==AbilityId.ConductiveBrand)candidates=candidates.Where(e=>!marks.TryGetValue(e.id,out var mark)||mark.consumed!=(currentCredit?.action??actionSerial));
            var targets=candidates.OrderBy(e=>followMarks==0?0:followKind==AbilityId.DrainBrand?(drains.TryGetValue(e.id,out var drain)&&drain.until>Elapsed?1:0):(marks.TryGetValue(e.id,out var mark)&&mark.until>Elapsed?1:0)).ThenBy(e=>Vector3.Distance(e.physics.Center,point)).Take(followMarks>0?followMarks:5).ToArray();
            if(targets.Length==0)return;
            Once("follow-release");
            Color color=followKind==AbilityId.DrainBrand?SiphonColor:VisualFactory.Cyan;
            fx.Arc(hero.RightHand.position,point,color);fx.Shape(point,Vector3.forward,2.2f,180,color,.35f);
            foreach(var e in targets)
            {
                if(followMarks==0)Control(e,followBank,Vector3.zero);
                else if(followKind==AbilityId.DrainBrand)DrainMark(e,followOwner);
                else MarkEnemy(e,followOwner);
            }
            ContributionFor(followOwner).assists++;
            Note(followMarks>0?(followKind==AbilityId.DrainBrand?"Weapon contact delivered Siphon":"Weapon contact delivered Conductive"):"Follow-through pulse spent",true);
            followBank=0;followMarks=0;followOwner=null;
        }
        void SpendLivewire(Enemy enemy)
        {
            var host=currentCredit?.weapon;
            if(host?.HasAugment(AugmentId.Livewire)!=true||currentCredit.derived||currentCredit.hop>0||enemy.deathAge>=0||CurrentTurn.livewire.Contains(host))return;
            bool previous=currentCredit.augment;currentCredit.augment=true;float before=enemy.health;
            if(ConsumeMark(enemy,false))
            {
                CurrentTurn.livewire.Add(host);float dealt=Mathf.Max(0,before-enemy.health);
                if(dealt>0&&Deck.Equipped(Signature.Grounding) is LoadoutItem ground&&Once("ground"))
                    AddBarrier(ground,Mathf.Min(ground.def.effect.Power(ground.rank),dealt*.2f*Effect(ground)),4);
                fx.Lightning(hero.RightHand.position,enemy.physics.Center,VisualFactory.Cyan);
                Note("Livewire discharged Conductive",true);
            }
            currentCredit.augment=previous;
        }
        void TryRetrieveBody(Vector3 point)
        {
            if(salvagerOwner==null)return;
            if(Deck.Equipped(Signature.Salvager)!=salvagerOwner){salvagerOwner=null;return;}
            if(CurrentTurn.procs.Contains("retrieval"))return;
            var body=corpses.Where(e=>Vector3.Distance(e.physics.Center,point)>.25f&&Vector3.Distance(e.physics.Center,point)<salvagerOwner.def.effect.Radius(salvagerOwner.rank))
                .OrderBy(e=>Vector3.Distance(e.physics.Center,point)).FirstOrDefault();
            if(body==null)return;
            Once("retrieval");var source=salvagerOwner;salvagerOwner=null;
            var direction=point-body.physics.Center;direction.y=0;
            Launch(body,null,0,.6f*Effect(source),direction.normalized);
            // Retrieval itself must not refill the preparation via OnLaunch.
            salvagerOwner=null;
            fx.Arc(body.physics.Center,point,VisualFactory.Orange);ContributionFor(source).assists++;
            Note("Prepared retrieval pulled a body",true);
        }
        void SurveyorEcho()
        {
            var turn=CurrentTurn;var last=turn.orderedHits.LastOrDefault(e=>e.deathAge<0);
            if(turn.hits.Count<2||last==null||!(Deck.Equipped(Signature.Surveyor) is LoadoutItem survey)||!Once("surveyor"))return;
            fx.Arc(hero.Root.position+Vector3.up,last.physics.Center,Color.white);var point=last.physics.Center;
            DerivedHit(last,survey.def.effect.Power(survey.rank),Vector3.zero,.1f);
            float radius=survey.def.effect.Radius(survey.rank);fx.Shape(point,Vector3.forward,radius,180,Color.white,.25f);
            foreach(var e in enemies.Where(e=>e!=last&&Vector3.Distance(e.physics.Center,point)<radius).Take(2).ToArray())
                DerivedHit(e,survey.def.effect.Power(survey.rank)*.35f,Vector3.zero,0);
            ContributionFor(survey).assists++;
            Note("Multi-target action released Surveyor echo");
        }
        void LeaveAfterfield(LoadoutItem host,AbilityCard card,Vector3 origin,Vector3 aim,Vector3 point)
        {
            if(!host.HasAugment(AugmentId.Afterfield))return;
            fields.RemoveAll(f=>f.host==host);
            fields.Add(new Field{host=host,credit=currentCredit,origin=origin,aim=aim,point=point,
                range=card.range,width=AugmentCatalog.TetherWidth(host),damage=card.damage*Deck.Damage});
        }
        bool InField(Field field,Enemy enemy)
        {
            var id=field.host.def.ability.id;
            bool inside=id==AbilityId.VectorTether?InCorridor(enemy,field.origin,field.aim,field.range,field.width):
                id==AbilityId.ReapingCut?InCone(enemy,field.origin,field.aim,field.range,70+5*(field.host.rank-1)):
                Vector3.Distance(enemy.rig.Root.position,field.point)<2.8f;
            return inside&&ClearLine(id==AbilityId.VectorTether||id==AbilityId.ReapingCut?field.origin:field.point,enemy);
        }
        void StepAfterfields(float dt)
        {
            foreach(var f in fields.ToArray())
            {
                f.left-=dt;f.tick-=dt;if(f.left<=0){fields.Remove(f);continue;}
                var id=f.host.def.ability.id;bool damaging=id==AbilityId.ArcBolt||id==AbilityId.ReapingCut;
                bool pulse=damaging&&f.pulses<2&&2-f.left>=(f.pulses+1)*.65f;
                if(f.tick>0&&!pulse)continue;
                currentCredit=f.credit;
                Color color=id==AbilityId.DrainBrand?SiphonColor:id==AbilityId.ReapingCut?Color.white:VisualFactory.Cyan;
                if(f.tick<=0)
                {
                    f.tick=.4f;
                    if(id==AbilityId.VectorTether)fx.Corridor(f.origin,f.aim,f.range,f.width,color,.4f);
                    else if(id==AbilityId.ReapingCut)fx.Shape(f.origin,f.aim,f.range,70+5*(f.host.rank-1),color,.4f);
                    else fx.Shape(f.point,Vector3.forward,2.8f,180,color,.4f);
                    if(id==AbilityId.GravityWell||id==AbilityId.VectorTether)GatherMotes(f.host,f.origin,f.aim,f.point,id==AbilityId.VectorTether?f.range:2.8f,f.width,id==AbilityId.VectorTether,true);
                }
                if(damaging&&!pulse)continue;
                if(pulse){f.pulses++;if(id==AbilityId.ReapingCut)fx.Sweep(f.origin,f.aim,f.range,70+5*(f.host.rank-1),Color.white,.3f,f.pulses==2);}
                var targets=enemies.Where(e=>(f.hit.Contains(e.id)||f.hit.Count<6)&&InField(f,e)).OrderBy(e=>e.id).ToArray();
                int contacts=0;
                foreach(var e in targets)
                {
                    if(!f.hit.Contains(e.id)&&f.hit.Count>=6)continue;
                    if(damaging&&(CurrentTurn.extra<=0||contacts>=(id==AbilityId.ArcBolt?3:6)))break;
                    f.hit.Add(e.id);
                    if(id==AbilityId.ConductiveBrand)MarkEnemy(e,f.host,.65f);
                    else if(id==AbilityId.DrainBrand)DrainMark(e,f.host,.65f);
                    else if(damaging)
                    {
                        contacts++;float amount=f.damage*.175f;
                        if(id==AbilityId.ArcBolt){ConsumeMark(e);fx.Lightning(f.point+Vector3.up,e.physics.Center,color);}
                        else if(e.health<e.maxHealth*(.5f+.025f*(f.host.rank-1)))amount*=1.4f+.05f*(f.host.rank-1);
                        float before=e.health;
                        DerivedHit(e,amount,Vector3.zero,0,id==AbilityId.ReapingCut,true,id==AbilityId.ArcBolt?ImpactKind.Electric:(ImpactKind?)null);
                        if(e.health<before)
                        {
                            if(CurrentTurn.hits.Add(e.id))CurrentTurn.orderedHits.Add(e);
                            if(id==AbilityId.ArcBolt&&Deck.Equipped(Signature.Grounding) is LoadoutItem ground&&Once("ground"))AddBarrier(ground,Mathf.Min(ground.def.effect.Power(ground.rank),(before-e.health)*.2f*Effect(ground)),4);
                        }
                    }
                    else
                    {
                        var point=id==AbilityId.VectorTether?f.origin+f.aim*Mathf.Clamp(Vector3.Dot(e.rig.Root.position-f.origin,f.aim),1,f.range):f.point;
                        Control(e,0,(point-e.rig.Root.position).normalized*2*Effect(f.host));SetSetup(e,f.host);
                    }
                }
                if(damaging){SurveyorEcho();if(contacts>0)Note(id==AbilityId.ArcBolt?"Storm afterfield hit enemies":"Cutting afterfield hit enemies");}
            }
        }
        void GatherMotes(LoadoutItem host,Vector3 origin,Vector3 aim,Vector3 point,float range,float width,bool corridor,bool field=false)
        {
            foreach(var mote in motes)
            {
                var delta=mote.point-origin;delta.y=0;float along=Vector3.Dot(delta,aim);
                bool inside=corridor?along>=0&&along<=range&&(delta-aim*along).magnitude<=width:
                    Vector3.Distance(mote.point,field?point:origin)<range;
                if(!inside||(!FastSimulation&&Physics.Linecast((field&&!corridor?point:origin)+Vector3.up*.5f,mote.point+Vector3.up*.5f,1<<EntryLayout.WorldLayer)))continue;
                mote.pullTarget=corridor?origin+aim*Mathf.Clamp(along,1,range):point;mote.pullTarget.y=mote.point.y;
                mote.pullLeft=.5f;mote.pullOwner=host;
                fx.Arc(mote.point,mote.pullTarget,Color.green);
            }
        }
    }
}
