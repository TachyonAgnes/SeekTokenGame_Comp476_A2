using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class IsTargetInAreaNode : ConditionNode {
    
    private AIAgent agent;
    private float dectectRadius = 30f;
    public override NodeStatus Execute() {
        if (CheckForEnemiesArea()) {
            return NodeStatus.SUCCESS;
        }else{
            return NodeStatus.FAILURE;
        }
    }

    public IsTargetInAreaNode(AIAgent agent) {
        this.agent = agent;
    }

    // check if there is an enemy in the area
    private bool CheckForEnemiesArea() {
        LayerMask mask = LayerMask.GetMask("Chaser", "Frozen");
        Collider[] colliders = Physics.OverlapSphere(agent.transform.position, dectectRadius, mask);

        agent.colliders = colliders;
        agent.isCollided = colliders.Length > 0;

        Collider closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in colliders) {
            if (!CheckObstacleBetween(col.gameObject)) {
                float distance = Vector3.Distance(agent.transform.position, col.transform.position);
                if (distance < closestDistance) {
                    closestEnemy = col;
                    closestDistance = distance;
                }
            }
        }

        if (closestEnemy != null) {
            agent.target = closestEnemy.gameObject;
            if (agent.debug) {
                Debug.DrawRay(closestEnemy.gameObject.transform.position, Vector3.up, Color.red, 5f);
            }
            return true;
        }

        return false;
    }

    // Check if there is an obstacle between the agent and the target
    private bool CheckObstacleBetween(GameObject target) {
        Vector3 start = agent.transform.position;
        Vector3 end = target.transform.position - start;
        float maxDistance = end.magnitude + 2f;
        bool result = Physics.Raycast(start, end, out RaycastHit hit, maxDistance) && hit.collider.gameObject != target;
        agent.isObstacleBetween = result;
        return result;
    }
}
