using UnityEngine;

namespace VectorGame
{
    [CreateAssetMenu(menuName = "VECTOR/Combat tuning")]
    public sealed class CombatTuning : ScriptableObject
    {
        [Header("Encounter")]
        public float duration = 60f;
        public float walkSpeed = .72f;
        public float heroHealth = 180f;
        public int seed = 1741;
        public int maximumEnemies = 42;
        [Header("Melee")]
        public float meleeReach = 2.2f;
        public float jabDamage = 17f;
        public float crossDamage = 22f;
        public float finisherDamage = 34f;
        public float comboRecovery = .25f;
        [Header("Force field")]
        public float pushCooldown = 6f;
        public float pushRange = 5.1f;
        public float pushDamage = 23f;
        public float pushImpulse = 12f;
        public int pushCrowdThreshold = 4;
        [Header("Swarm")]
        public float enemyHealth = 40f;
        public float enemySpeed = 2.75f;
        public float enemyDamage = 4f;
        public float attackRadius = 1.65f;
        public int simultaneousAttackers = 4;
        public float firstSpawnInterval = 1.15f;
        public float finalSpawnInterval = .48f;
        [Header("Camera")]
        public Vector3 cameraOffset = new Vector3(0, 8.6f, -10.2f);
        public float lookAhead = 4.2f;
        public float cameraFov = 47f;
    }
}
