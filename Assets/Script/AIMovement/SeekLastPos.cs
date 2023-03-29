using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// actually an arrive method
public class SeekLastPos : AIMovement {
    public float slowingRadius = 5f;
    public float stoppingRadius = 1f;


    private Vector3 ToArrive(AIAgent agent, Vector3 targetPos, float slowingRadius, float stoppingRadius) {
        Vector3 direction = targetPos - agent.transform.position;
        float distance = direction.magnitude;

        // If the agent is close enough to the target, stop moving
        if (distance < stoppingRadius) {
            return Vector3.zero;
        }

        // Calculate the target speed, depending on the distance to the target
        float targetSpeed = agent.maxSpeed;
        if (distance < slowingRadius) {
            targetSpeed *= (distance / slowingRadius);
        }

        // Calculate the desired velocity and steering force
        Vector3 desiredVelocity = direction.normalized * targetSpeed;
        Vector3 steeringForce = desiredVelocity - agent.velocity;

        return steeringForce;
    }

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        Vector3 targetPos = agent.targetLastPos;

        output.linear = ToArrive(agent, targetPos, slowingRadius, stoppingRadius);

        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        Vector3 steering = GetKinematic(agent).linear - output.linear;
        steering.Set(steering.x, 0, steering.z);    
        output.linear = steering;

        return output;
    }
}
