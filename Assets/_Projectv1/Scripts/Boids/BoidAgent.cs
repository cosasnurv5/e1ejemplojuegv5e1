using System.Collections.Generic;
using UnityEngine;
using Steering;
using FSM;
using Boids.States;

namespace Boids
{
    [RequireComponent(typeof(BoidSensor))]
    public class BoidAgent : SteeringAgent
    {
        [Header("Rangos de Percepción.")]
        public float neighborRadius = 5f;
        public float separationRadius = 2f;
        public float visionRadius = 8f;

        [Header("Pesos de Comportamiento.")]
        public float separationWeight = 2.5f;
        public float alignmentWeight = 1.0f;
        public float cohesionWeight = 1.0f;
        public float evadeWeight = 4.0f;
        public float arriveWeight = 1.5f;

        [Header("Límites.")]
        public float mapLimitX = 18f;
        public float mapLimitZ = 18f;
        public float boundaryWeight = 3.5f;

        [Header("Sistema de Golpes y Daño.")]
        public int HitsReceived { get; private set; } = 0;
        public float damagePerSecond = 25f;
        public float interactDistance = 1.2f;

        public bool IsDestroyed { get; set; }
        public Transform CurrentInterestTarget { get; set; }

        public BoidSensor Sensor { get; private set; }
        public FiniteStateMachine FSM { get; private set; }

        public BoidFlockngState FlockngState { get; private set; }
        public BoidEvadState EvadState { get; private set; }
        public BoidSkObjState SkObjState { get; private set; }
        public BoidIntrState IntrState { get; private set; }
        public BoidDestroydState DestroydState { get; private set; }
        public BoidRespwnState RespwnState { get; private set; }

        protected void Awake()
        {
            Sensor = GetComponent<BoidSensor>();
            FSM = new FiniteStateMachine();

            FlockngState = new BoidFlockngState(this);
            EvadState = new BoidEvadState(this);
            SkObjState = new BoidSkObjState(this);
            IntrState = new BoidIntrState(this);
            DestroydState = new BoidDestroydState(this);
            RespwnState = new BoidRespwnState(this);
        }

        private void Start()
        {
            FSM.ChangeState(FlockngState);
        }

        protected override void Update()
        {
            if (IsDestroyed && FSM.CurrentState != DestroydState && FSM.CurrentState != RespwnState)
            {
                FSM.ChangeState(DestroydState);
            }

            Vector3 boundaryForce = CalculateBoundaryForce() * boundaryWeight;
            AddForce(boundaryForce);

            FSM.Update();
            base.Update();
        }

        public void ReceiveHit(int hits)
        {
            if (IsDestroyed) return;

            HitsReceived += hits;

            if (HitsReceived >= 2)
            {
                HitsReceived = 2;
                IsDestroyed = true;
                FSM.ChangeState(DestroydState);
            }
        }

        public void ApplyFlockingForces()
        {
            List<BoidAgent> neighbors = Sensor.GetNeighbors(neighborRadius);
            if (neighbors.Count > 0)
            {
                AddForce(CalculateSeparation(neighbors) * separationWeight);
                AddForce(CalculateAlignment(neighbors) * alignmentWeight);
                AddForce(CalculateCohesion(neighbors) * cohesionWeight);
            }

            if (Velocity.sqrMagnitude < 0.5f)
            {
                AddForce(transform.forward * maxSpeed);
            }
        }

        public Vector3 CalculateBoundaryForce()
        {
            Vector3 force = Vector3.zero;
            Vector3 pos = transform.position;

            if (pos.x > mapLimitX) force.x = -1f;
            else if (pos.x < -mapLimitX) force.x = 1f;

            if (pos.z > mapLimitZ) force.z = -1f;
            else if (pos.z < -mapLimitZ) force.z = 1f;

            if (force != Vector3.zero)
            {
                Vector3 desired = force.normalized * maxSpeed;
                return desired - Velocity;
            }

            return Vector3.zero;
        }

        public Vector3 CalculateSeparation(List<BoidAgent> neighbors)
        {
            Vector3 separation = Vector3.zero;
            int count = 0;
            float sqrSeparationRadius = separationRadius * separationRadius;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor == this || neighbor.IsDestroyed) continue;

                Vector3 diff = transform.position - neighbor.transform.position;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist > 0 && sqrDist < sqrSeparationRadius)
                {
                    separation += diff.normalized / Mathf.Max(sqrDist, 0.05f);
                    count++;
                }
            }

            if (count > 0) separation /= count;
            return separation;
        }

        public Vector3 CalculateAlignment(List<BoidAgent> neighbors)
        {
            Vector3 averageVelocity = Vector3.zero;
            int validNeighbors = 0;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor.IsDestroyed) continue;
                averageVelocity += neighbor.Velocity;
                validNeighbors++;
            }

            if (validNeighbors == 0) return Vector3.zero;

            averageVelocity /= validNeighbors;
            return averageVelocity - Velocity;
        }

        public Vector3 CalculateCohesion(List<BoidAgent> neighbors)
        {
            Vector3 centerOfMass = Vector3.zero;
            int validNeighbors = 0;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor.IsDestroyed) continue;
                centerOfMass += neighbor.transform.position;
                validNeighbors++;
            }

            if (validNeighbors == 0) return Vector3.zero;

            centerOfMass /= validNeighbors;
            return SteeringBehaviors.Seek(this, centerOfMass);
        }

        public void StopAgent()
        {
            Velocity = Vector3.zero;
        }

        public void ResetAgent()
        {
            HitsReceived = 0;
            IsDestroyed = false;
            gameObject.SetActive(true);
            FSM.ChangeState(FlockngState);
        }
    }
}