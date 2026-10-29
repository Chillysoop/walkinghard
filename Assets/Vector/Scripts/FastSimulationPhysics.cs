using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        // Screening model only: the authored 60 Hz combat/draft rules still execute,
        // but articulated bodies, fragment contacts and rigid-body constraints do not.
        // Each enemy identity has one moving proxy; flights use swept-circle contacts.
        void StepFastPhysics(float dt)
        {
            heroPhysics.IntegrateFast(dt);
            foreach(var e in enemies)
            {
                e.physics.IntegrateFast(dt);
                var p=e.rig.Root.position;
                if(!e.entering)p.x=Mathf.Clamp(p.x,-8.9f+e.physics.FastRadius,8.9f-e.physics.FastRadius);
                var delta=p-HeroPosition;delta.y=0;
                float separation=heroPhysics.FastRadius+e.physics.FastRadius;
                if(delta.sqrMagnitude<separation*separation)
                {
                    var away=delta.sqrMagnitude>.0001f?delta.normalized:new Vector3(e.id%2==0?1:-1,0,0);
                    var adjusted=HeroPosition+away*separation;p.x=adjusted.x;p.z=adjusted.z;
                }
                e.rig.Root.position=p;
            }
            foreach(var e in corpses)e.physics.IntegrateFast(dt);
            foreach(var flight in flights)
            {
                Vector3 from=flight.previous,to=flight.body.physics.Center,segment=to-from;
                segment.y=0;float squared=segment.sqrMagnitude;
                foreach(var target in enemies)
                {
                    if(target==flight.body||flight.chain.hit.Contains(target.id))continue;
                    var point=target.physics.Center;var delta=point-from;delta.y=0;
                    float fraction=squared>.000001f?Mathf.Clamp01(Vector3.Dot(delta,segment)/squared):0;
                    var closest=Vector3.Lerp(from,to,fraction);var offset=point-closest;
                    if(Mathf.Abs(offset.y)>1.5f)continue;offset.y=0;
                    float radius=flight.body.physics.FastRadius+target.physics.FastRadius;
                    if(offset.sqrMagnitude<=radius*radius)
                        QueueContact(flight.body.id,target.id,(flight.body.physics.Velocity-target.physics.Velocity).magnitude,closest);
                }
            }
        }
    }
}
