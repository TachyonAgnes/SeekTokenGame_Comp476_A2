using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : AIMovement {
    public float wanderRadius = 150f;
    public float wanderDistance = 10f;
    public float deltaDegree = 90f;
    private float wanderTargetUpdateTime = 1.5f;
    private float timeSinceLastWander = 0f;
    private Vector3 wanderTarget;
    private Vector3 ToWander(AIAgent agent) {
        // Update the wander target periodically
        timeSinceLastWander += Time.fixedDeltaTime;
        if (timeSinceLastWander >= wanderTargetUpdateTime) {
            float wanderAngle = (UnityEngine.Random.value - UnityEngine.Random.value) * deltaDegree;
            Vector3 circlePos = transform.position + transform.forward * wanderDistance;
            wanderTarget = circlePos + Quaternion.AngleAxis(wanderAngle, transform.up) * (transform.forward * wanderRadius);
            timeSinceLastWander = 0f;
        }
        // Steer towards the wander target
        return ToSeek(agent, wanderTarget);
    }

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);

        output.linear = ToWander(agent);
        if (debug) Debug.DrawRay(transform.position, output.linear, Color.cyan);
        return output;
    }
    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        Vector3 steering = GetKinematic(agent).linear - output.linear;
        output.linear = steering;
        if (debug) Debug.DrawRay(transform.position + agent.velocity, output.linear, Color.green);
        return output;
    }
}
