using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flee : AIMovement {
    AIAgent myAgent;

    // Flee from a target position
    private Vector3 ToFlee(AIAgent agent) {
        List<GameObject> chasers = FindChasers();
        if (chasers.Count > 0) {
            Vector3 desired = Vector3.zero;
            foreach (GameObject chaser in chasers) {
                Vector3 direction = transform.position - agent.target.transform.position;
                desired = direction.normalized * agent.maxSpeed;
            }
            desired.y = 0;
            return desired;
        }
        return Vector3.zero;
    }

    public List<GameObject> FindChasers() {
        float fleeDistance = myAgent.nearRadius;
        Collider[] colliders = Physics.OverlapSphere(transform.position, fleeDistance, LayerMask.GetMask("Chaser"));
        List<GameObject> chasers = new List<GameObject>();
        foreach (Collider collider in colliders) {
            chasers.Add(collider.gameObject);
        }
        return chasers;
    }

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        myAgent = agent;

        output.linear = ToFlee(agent);

        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        output.linear = GetKinematic(agent).linear - output.linear;

        return output;
    }

}
