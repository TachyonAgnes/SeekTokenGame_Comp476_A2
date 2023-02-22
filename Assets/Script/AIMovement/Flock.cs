using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : AIMovement
{    
    public float neighborRadius = 5f;
    public float separationRadius = 3.5f;
    private float maxSpeed;
    private Vector3 velocity;
    private float maxForce;
    private AIAgent thisAgent;



    private Vector3 ToFlock(AIAgent agent) {
        Collider[] colliders = GetNeighborContext();
        List<AIAgent> neighbors = new List<AIAgent>();
        foreach (Collider col in colliders) {
            AIAgent neighbor = col.GetComponent<AIAgent>();
            if (neighbor != null && neighbor != this) {
                neighbors.Add(neighbor);
            }
        }
        //Debug Colliders
        Vector3 alignment = Align(neighbors);
        Vector3 cohesion = Cohere(neighbors);
        Vector3 separation = Separate(neighbors);
        Vector3 flockingForce = alignment + cohesion + separation;
        Vector3 horizontalFlockingForce = Vector3.ProjectOnPlane(flockingForce, Vector3.up); 
        return horizontalFlockingForce;
    }

    private Vector3 Align(List<AIAgent> neighbors) {
        if (neighbors.Count == 0) {
            return Vector3.zero;
        }
        Vector3 averageVelocity = Vector3.zero;
        foreach (AIAgent neighbor in neighbors) {
            averageVelocity += neighbor.velocity;
        }
        averageVelocity /= neighbors.Count;
        Vector3 desired = averageVelocity.normalized * maxSpeed;
        Vector3 steering = desired - velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        return steering;
    }
    private Vector3 Cohere(List<AIAgent> neighbors) {
        if (neighbors.Count == 0) {
            return Vector3.zero;
        }
        Vector3 averagePos = Vector3.zero;
        foreach (AIAgent neighbor in neighbors) {
            averagePos += neighbor.transform.position;
        }
        averagePos /= neighbors.Count;
        return ToSeek(thisAgent,averagePos);
    }
    private Vector3 Separate(List<AIAgent> neighbors) {
        Vector3 steering = Vector3.zero;
        foreach (AIAgent neighbor in neighbors) {
            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
            if (distance > 0 && distance < separationRadius) {
                Vector3 awayFromNeighbor = transform.position - neighbor.transform.position;
                awayFromNeighbor = awayFromNeighbor.normalized / distance;
                steering += awayFromNeighbor;
            }
        }
        steering = steering.normalized * maxSpeed;
        steering -= velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        return steering;
    }

    public Collider[] GetNeighborContext() {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, neighborRadius, LayerMask.GetMask("Evader"));
        return neighbors;
    }

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        thisAgent = agent;
        maxSpeed = agent.maxSpeed;
        velocity = agent.velocity;
        maxForce = agent.maxForce;

        output.linear = ToFlock(agent);

        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        output.linear = GetKinematic(agent).linear - output.linear;

        return output;
    }
}
