using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : AIMovement {
    public float avoidanceRadius = 2.5f;
    public float avoidanceForce = 14.0f;

    private Collider[] obstacles;
    private Vector3 ToObstacleAvoidance(AIAgent agent) {
        Vector3 steering = Vector3.zero;
        LayerMask mask = LayerMask.GetMask("Obstacle", agent.tag == "Chaser" ? "Chaser" : "Evader");

        obstacles = Physics.OverlapSphere(transform.position, avoidanceRadius, mask);

        foreach (Collider obstacle in obstacles) {
            Vector3 closestPoint = obstacle.ClosestPoint(transform.position);
            Vector3 direction = transform.position - closestPoint;
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
        output.linear += ToObstacleAvoidance(agent);
        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);
        output.linear += ToObstacleAvoidance(agent);
        return output;
    }
}