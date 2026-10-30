using UnityEngine;
using FSM;
using Steering;

namespace Hunter.States
{
    public class HunterPatrolState : IState
    {
        private readonly HunterNPC hunter;
        private float spawnTimer = 0f;
        private const float SPAWN_INTERVAL = 5f;

        private const float WAYPOINT_THRESHOLD_SQR = 1.0f;

        public HunterPatrolState(HunterNPC hunter)
        {
            this.hunter = hunter;
        }

        public void Enter()
        {
            spawnTimer = 0f; 
        }

        public void Update()
        {
            
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= SPAWN_INTERVAL)
            {
                spawnTimer = 0f;
                if (hunter.spawner != null && hunter.spawner.CanSpawn())
                {
                    Vector3 spawnPos = hunter.transform.position + hunter.transform.forward * 2f;
                    spawnPos.y = 0f; 
                    hunter.spawner.SpawnInterestObject(spawnPos);
                    Debug.Log($"El HunterNPC, dejo un objeto.");
                }
            }

         
            var destroyedBoid = hunter.FindDestroyedBoidInVision();
            if (destroyedBoid != null)
            {
                hunter.DestroyedTarget = destroyedBoid;
                hunter.FSM.ChangeState(hunter.GatherState);
                return;
            }

          
            if (hunter.TBATimer <= 0f)
            {
                var validBoid = hunter.FindValidBoidInVision();
                if (validBoid != null)
                {
                    hunter.CurrentTarget = validBoid;
                    hunter.FSM.ChangeState(hunter.AttackState);
                    return;
                }
            }

            
            if (hunter.path != null)
            {
                Vector3 targetWaypoint = hunter.path.GetCurrentWaypoint();
                hunter.AddForce(SteeringBehaviors.Seek(hunter, targetWaypoint));

                if ((hunter.transform.position - targetWaypoint).sqrMagnitude < WAYPOINT_THRESHOLD_SQR)
                {
                    hunter.path.AdvanceToNext();
                }
            }
        }

        public void Exit() { }
    }
}