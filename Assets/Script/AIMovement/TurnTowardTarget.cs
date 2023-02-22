using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTowardTarget : AIMovement {
    public float maxRotationSpeed = 10f;
    public float timeToTarget = 0.01f;

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);

        return output;
    }
    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        if (agent.target == null)
            return output;

        // Calculate direction to target
        Vector3 directionToTarget = agent.target.transform.position - agent.transform.position;
        directionToTarget.y = 0f; // Lock rotation around y-axis

        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Determine if facing target or not
        agent.isFacingTarget = Quaternion.Angle(GetKinematic(agent).angular, targetRotation) < 1.0f;

        // Calculate rotation speed
        float rotateSpeed = Mathf.Min(Quaternion.Angle(GetKinematic(agent).angular, targetRotation) / timeToTarget, maxRotationSpeed);

        // Calculate steering angular velocity
        Quaternion desiredRotation = agent.isFacingTarget ? GetKinematic(agent).angular : Quaternion.RotateTowards(GetKinematic(agent).angular, targetRotation, rotateSpeed);
        output.angular = desiredRotation * Quaternion.Inverse(GetKinematic(agent).angular);

        return output;
    }

}