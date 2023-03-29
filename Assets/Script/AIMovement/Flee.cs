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
                if (!CheckObstacleBetween(chaser.gameObject)) {
                    Vector3 direction = agent.transform.position - chaser.transform.position;
                    desired += direction.normalized;
                }
            }
            desired.Normalize();
            desired.y = 0;
            return desired * agent.maxSpeed;
        }
        return Vector3.zero;
    }

    public List<GameObject> FindChasers() {
        float fleeDistance = myAgent.farRadius;
        LayerMask mask = LayerMask.GetMask("Chaser", "Frozen");
        Collider[] colliders = Physics.OverlapSphere(myAgent.transform.position, fleeDistance, mask);
        List<GameObject> chasers = new List<GameObject>();
        foreach (Collider col in colliders) {
            chasers.Add(col.gameObject);
        }
        return chasers;
    }

    // Check if there is an obstacle between the agent and the target
    private bool CheckObstacleBetween(GameObject target) {
        Vector3 start = myAgent.transform.position;
        Vector3 end = target.transform.position - start;
        float maxDistance = end.magnitude + 2f;
        bool result = Physics.Raycast(start, end, out RaycastHit hit, maxDistance) && hit.collider.gameObject != target;
        return result;
    }

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        output.linear = ToFlee(agent);
        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);
        myAgent = agent;
        output.linear = GetKinematic(agent).linear - output.linear;
        return output;
    }

}
