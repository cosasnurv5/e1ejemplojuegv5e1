using UnityEngine;

namespace Steering
{
    public static class SteeringBehaviors
    {
        public static Vector3 Seek(SteeringAgent agent, Vector3 targetPosition)
        {
            Vector3 desiredVelocity = (targetPosition - agent.transform.position).normalized * agent.maxSpeed;
            return desiredVelocity - agent.Velocity;
        }

        public static Vector3 Flee(SteeringAgent agent, Vector3 targetPosition)
        {
            Vector3 desiredVelocity = (agent.transform.position - targetPosition).normalized * agent.maxSpeed;
            return desiredVelocity - agent.Velocity;
        }

        public static Vector3 Arrive(SteeringAgent agent, Vector3 targetPosition, float slowingRadius)
        {
            Vector3 desiredVelocity = targetPosition - agent.transform.position;
            float distance = desiredVelocity.magnitude;

            if (distance < 0.001f) return -agent.Velocity;

            if (distance < slowingRadius)
            {
                desiredVelocity = desiredVelocity.normalized * agent.maxSpeed * (distance / slowingRadius);
            }
            else
            {
                desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            }

            return desiredVelocity - agent.Velocity;
        }

        public static Vector3 Evade(SteeringAgent agent, SteeringAgent pursuerAgent)
        {
            Vector3 distance = pursuerAgent.transform.position - agent.transform.position;
            float updatesEsti = distance.magnitude / Mathf.Max(agent.maxSpeed, 0.1f);
            Vector3 estiPosition = pursuerAgent.transform.position + pursuerAgent.Velocity * updatesEsti;

            return Flee(agent, estiPosition);
        }

       
        public static Vector3 Pursuit(SteeringAgent agent, SteeringAgent targetAgent)
        {
            Vector3 distance = targetAgent.transform.position - agent.transform.position;
            float updatesEsti = distance.magnitude / Mathf.Max(agent.maxSpeed, 0.1f);
            Vector3 estiPosition = targetAgent.transform.position + targetAgent.Velocity * updatesEsti;

            return Seek(agent, estiPosition);
        }
    }
}