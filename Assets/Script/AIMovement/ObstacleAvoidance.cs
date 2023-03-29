using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : AIMovement {
    private float avoidanceRadius = 3f;
    private float avoidanceForce = 1f;

    private int numberOfRays = 5;
    private float angle = 45f;
    private float avoidanceShape = -0.3f;


    private Vector3 ToObstacleAvoidance(AIAgent agent) {
        Vector3 steering = Vector3.zero;
        LayerMask mask;
        if (agent.CompareTag("Chaser")) {
            avoidanceShape = -0.1f;
            mask = LayerMask.GetMask("Obstacle", "Blizzard");
        }
        else {
            avoidanceRadius = 0.3f;
            mask = LayerMask.GetMask("Obstacle", "Chaser");
        }
        

        for (int i = 0; i < numberOfRays; i++) {
            var rotation = agent.transform.rotation;
            var rotaionMod = Quaternion.AngleAxis((i / (float)numberOfRays) * angle * 2 - angle, this.transform.up);
            var direction = rotation * rotaionMod * Vector3.forward;

            var distanceFromCenter = i / (float)(numberOfRays - 1) + avoidanceShape;
            var length = avoidanceRadius;
            if (Mathf.Abs(distanceFromCenter) > 0.001f) {
                length /= Mathf.Abs(distanceFromCenter);
            }
            var ray = new Ray(agent.transform.position, direction * length);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, avoidanceRadius, mask)) {
                if (agent.debug) {
                    Debug.DrawRay(agent.transform.position, (1.0f / numberOfRays) * direction * length);
                }
                steering -= (1.0f / numberOfRays) * avoidanceForce * direction;
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
