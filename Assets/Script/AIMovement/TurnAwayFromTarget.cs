using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAwayFromTarget : AIMovement {
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

        // Calculate direction away from target
        Vector3 directionAwayFromTarget = agent.transform.position - agent.target.transform.position;
        directionAwayFromTarget.y = 0f; // Lock rotation around y-axis

        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionAwayFromTarget);

        // Determine if facing away from target or not
        agent.isFacingAwayFromTarget = Quaternion.Angle(GetKinematic(agent).angular, targetRotation) < 1.0f;

        // Calculate rotation speed
        float rotateSpeed = Mathf.Min(Quaternion.Angle(GetKinematic(agent).angular, targetRotation) / timeToTarget, maxRotationSpeed);

        // Calculate steering angular velocity
        Quaternion desiredRotation = agent.isFacingAwayFromTarget ? GetKinematic(agent).angular : Quaternion.RotateTowards(GetKinematic(agent).angular, targetRotation, rotateSpeed);
        output.angular = desiredRotation * Quaternion.Inverse(GetKinematic(agent).angular);

        return output;
    }
}