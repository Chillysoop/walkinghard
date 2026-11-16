using System;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class FighterPhysics
    {
        // These layers express the existing corpse/walker policy before broad-phase
        // pairs become thousands of individual IgnoreCollision entries.
        internal static readonly bool OriginalBodyFilters=Array.IndexOf(Environment.GetCommandLineArgs(),"-originalBodyFilters")>=0;
        const int LivingLayer=12,RestingBodyLayer=13,FlyingBodyLayer=14,HeroLayer=15;
        static bool bodyLayersReady;
        int bodyLayer=-1;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetBodyLayerPolicy(){bodyLayersReady=false;}
        static void ConfigureBodyLayers()
        {
            if(bodyLayersReady)return;
            Physics.IgnoreLayerCollision(LivingLayer,RestingBodyLayer,true);
            Physics.IgnoreLayerCollision(HeroLayer,RestingBodyLayer,true);
            Physics.IgnoreLayerCollision(HeroLayer,FlyingBodyLayer,true);
            bodyLayersReady=true;
        }
        public bool SetProjectileFilter(bool projectile)
        {
            if(fast||OriginalBodyFilters)return false;
            int layer=projectile?FlyingBodyLayer:RestingBodyLayer;
            if(layer!=bodyLayer){foreach(var collider in colliders)collider.gameObject.layer=layer;bodyLayer=layer;}
            return true;
        }
    }
}
