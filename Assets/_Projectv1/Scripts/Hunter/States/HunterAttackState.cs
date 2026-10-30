using UnityEngine;
using FSM;
using Steering;

namespace Hunter.States
{
    public class HunterAttackState : IState
    {
        private readonly HunterNPC hunter;

        public HunterAttackState(HunterNPC hunter) => this.hunter = hunter;

        public void Enter()
        {
            Debug.Log($"El {hunter.gameObject.name} entró en modo ataque.");
        }

        public void Update()
        {
          
            hunter.Energy -= hunter.energyD * Time.deltaTime;

            if (hunter.Energy <= 0f)
            {
                hunter.Energy = 0f;
                Debug.Log($"El {hunter.gameObject.name} cambiando de modo para recuperar energía.");
                hunter.FSM.ChangeState(hunter.RvState);
                return;
            }

          
            var destroyedBoid = hunter.FindDestroyedBoidInVision();
            if (destroyedBoid != null)
            {
                Debug.Log($"El {hunter.gameObject.name} cambiando de modo para recoletar el boid destruido.");
                hunter.DestroyedTarget = destroyedBoid;
                hunter.FSM.ChangeState(hunter.GatherState);
                return;
            }

            var target = hunter.CurrentTarget;

            
            if (target == null || target.IsDestroyed)
            {
                var newTarget = hunter.FindValidBoidInVision();
                if (newTarget != null)
                {
                    hunter.CurrentTarget = newTarget;
                    target = newTarget;
                    Debug.Log($"En busqueda del Boid: {target.name}");
                }
                else
                {
                   
                    return;
                }
            }

          
            if (hunter.IsTargetOutOfBounds(target.transform.position))
            {
                Debug.Log($"El Boid {target.name} salió de los límites del mapa, volviendo al modo busqueda de Boid.");
                hunter.CurrentTarget = null;
                hunter.FSM.ChangeState(hunter.PatrolState);
                return;
            }

            float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

         
            hunter.AddForce(SteeringBehaviors.Pursuit(hunter, target));

           
            if (distance <= hunter.MeleeAttackRadius)
            {
                if (hunter.TBATimer <= 0f)
                {
                    Debug.Log($"El HunterNPC ataco cuerpo a cuerpo a {target.name}.");
                    ExeAttack(target, 2); 
                }
            }
            else if (distance <= hunter.RangeAttackRadius)
            {
                if (hunter.TBATimer <= 0f)
                {
                    Debug.Log($"El HunterNPC ataco a distancia a {target.name}.");
                    ExeAttack(target, 1);
                }
            }
        }

        private void ExeAttack(Boids.BoidAgent target, int hits)
        {
            target.ReceiveHit(hits);
            hunter.TBATimer = hunter.TBA;

            if (target.IsDestroyed)
            {
                Debug.Log($"El HunterNPC destruyó al Boid: {target.name}. Cambiando de modo para recolectar el Boid destruido.");
                hunter.DestroyedTarget = target;
                hunter.CurrentTarget = null;
                hunter.FSM.ChangeState(hunter.GatherState);
            }
        }

        public void Exit()
        {
            Debug.Log($"El HunterNPC, cambiando el modo ataque.");
        }
    }
}