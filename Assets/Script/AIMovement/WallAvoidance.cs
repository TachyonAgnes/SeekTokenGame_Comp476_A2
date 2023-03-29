using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAvoidance : AIMovement {
    private float avoidanceRadius = 2.5f;
    private float avoidanceForce = 20.0f;

    private Collider[] obstacles;
    private Vector3 ToWallAvoidance(AIAgent agent) {
        LayerMask mask;
        if (agent.CompareTag("Evader")) {
            avoidanceRadius = 4f;
             avoidanceForce = 15.0f;
             mask = LayerMask.GetMask("Obstacle", "Chaser");
        }
        else {
            mask = LayerMask.GetMask("Obstacle", "Chaser");
        }
        Vector3 steering = Vector3.zero;
        

        obstacles = Physics.OverlapSphere(agent.transform.position, avoidanceRadius, mask);

        foreach (Collider obstacle in obstacles) {
            Vector3 closestPoint = obstacle.ClosestPoint(agent.transform.position);
            Vector3 direction = agent.transform.position - closestPoint;
            float distance = direction.magnitude;
            if (distance < avoidanceRadius) {
                float strength = avoidanceForce * (avoidanceRadius - distance) / avoidanceRadius;
                steering += direction.normalized * strength;
            }
        }
        steering.y = 0f;
        return steering;
    }

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        output.linear += ToWallAvoidance(agent);
        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);
        output.linear += ToWallAvoidance(agent);
        return output;
    }
}