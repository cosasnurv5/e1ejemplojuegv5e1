using UnityEngine;
using FSM;
using Steering;
using Boids;
using Hunter.States;
using Environment;
using Managers;

namespace Hunter
{
    public class HunterNPC : SteeringAgent
    {
        [Header("Requerimientos.")]
        public float TBA = 3f;
        public float RangeAttackRadius = 8f;
        public float MeleeAttackRadius = 2f;

        [Header("Energía.")]
        public float maxEnergy = 100f;
        public float Energy = 100f;
        public float energyD = 10f;
        public float energyR = 20f;

        [Header("Configuración.")]
        public float visionRadius = 10f;
        public WaypointPath path;
        public ObjectSpawner spawner;
        public float mapLimitX = 18f;
        public float mapLimitZ = 18f;

        [Header("DebgR.")]
        public bool showDebugVisuals = true;
        [SerializeField] private Renderer hunterRenderer;

        public FiniteStateMachine FSM { get; private set; }
        public HunterPatrolState PatrolState { get; private set; }
        public HunterAttackState AttackState { get; private set; }
        public HunterGatherState GatherState { get; private set; }
        public HunterRvState RvState { get; private set; }

        public float TBATimer { get; set; }
        public BoidAgent CurrentTarget { get; set; }
        public BoidAgent DestroyedTarget { get; set; }

        protected void Awake()
        {
            FSM = new FiniteStateMachine();

            PatrolState = new HunterPatrolState(this);
            AttackState = new HunterAttackState(this);
            GatherState = new HunterGatherState(this);
            RvState = new HunterRvState(this);

            if (hunterRenderer == null)
                hunterRenderer = GetComponentInChildren<Renderer>();
        }

        private void Start()
        {
            TBATimer = 0f;
            FSM.ChangeState(PatrolState);
       
        }

        protected override void Update()
        {
            if (TBATimer > 0)
            {
                TBATimer -= Time.deltaTime;
            }

            FSM.Update();
            base.Update();
        }

        public bool IsTargetOutOfBounds(Vector3 position)
        {
            return Mathf.Abs(position.x) > mapLimitX || Mathf.Abs(position.z) > mapLimitZ;
        }


        public BoidAgent FindValidBoidInVision()
        {
            if (GameManager.Instance == null) return null;

            BoidAgent bestTarget = null;
            float visionRadiusSqr = visionRadius * visionRadius;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var boid in GameManager.Instance.ActiveBoids)
            {
                if (boid == null || boid.IsDestroyed || !boid.gameObject.activeInHierarchy) continue;

              
                if (IsTargetOutOfBounds(boid.transform.position)) continue;

                float dSqr = (boid.transform.position - transform.position).sqrMagnitude;

                if (dSqr <= visionRadiusSqr && dSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqr;
                    bestTarget = boid;
                }
            }

            if (bestTarget != null && CurrentTarget != bestTarget)
            {
                Debug.Log($"El HunterNPC, detecto el Boid {bestTarget.name}");
            }

            return bestTarget;
        }

        public BoidAgent FindDestroyedBoidInVision()
        {
            if (GameManager.Instance == null) return null;

            float visionRadiusSqr = visionRadius * visionRadius;

            foreach (var boid in GameManager.Instance.ActiveBoids)
            {
                if (boid == null || !boid.IsDestroyed) continue;

                float dSqr = (boid.transform.position - transform.position).sqrMagnitude;
                if (dSqr <= visionRadiusSqr)
                {
                    return boid;
                }
            }
            return null;
        }

        public void Stop()
        {
            Velocity = Vector3.zero;
        }

        
       
       
        private void OnDrawGizmos()
        {
            if (!showDebugVisuals) return;

      
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRadius);

          
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, RangeAttackRadius);

          
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, MeleeAttackRadius);

           
            if (CurrentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, CurrentTarget.transform.position);
            }
        }
    }
}