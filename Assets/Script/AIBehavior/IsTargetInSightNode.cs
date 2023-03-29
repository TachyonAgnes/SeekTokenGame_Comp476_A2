using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class IsTargetInSightNode : ConditionNode {
    private AIAgent aiAgent;

    public override NodeStatus Execute() {
        if (CheckForEnemiesArea()) {
            // Important: reset tokenAcquiredPos to positive infinity so that the agent will not go back to the token
            aiAgent.tokenAcquiredPos = Vector3.positiveInfinity;
            return NodeStatus.SUCCESS;
        } else {
            return NodeStatus.FAILURE;
        }
    }
      
    public IsTargetInSightNode(AIAgent agent) {
        this.aiAgent = agent;
    }

    // Check for enemies within the agent's sight cone
    private bool CheckForEnemiesArea() {
        Collider[] colliders = Physics.OverlapSphere(aiAgent.transform.position, aiAgent.farRadius);
        Collider closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        aiAgent.nearAllies.Clear();
        foreach (Collider col in colliders) {
            if(col.gameObject == aiAgent.gameObject) {
                continue;
            }
            bool isChaser = col.CompareTag("Chaser");
            bool isEvader = col.CompareTag("Evader");

            if ((aiAgent.gameObject.CompareTag("Evader") && isChaser) || (aiAgent.gameObject.CompareTag("Chaser") && isEvader)) {
                if (CheckInCone(col.gameObject)) {
                    float distance = Vector3.Distance(aiAgent.transform.position, col.transform.position);
                    if (distance < closestDistance) {
                        closestEnemy = col;
                        closestDistance = distance;
                    }
                }
            }
            
            if(aiAgent.gameObject.CompareTag("Chaser") && isChaser) {
                // a list that contains all the allies in the area
                if (!CheckObstacleBetween(col.gameObject)) {
                    aiAgent.nearAllies.Add(col.gameObject);
                }
            }
        }
        
        if (closestEnemy != null) {
            aiAgent.isSearching = false;
            aiAgent.target = closestEnemy.gameObject;
            aiAgent.targetLastPos = closestEnemy.gameObject.transform.position;
            if (aiAgent.debug) {
                Debug.DrawRay(closestEnemy.gameObject.transform.position, Vector3.up, Color.red, 5f);
            }
            return true;
        }

        return false;
    }

    private bool CheckInCone(GameObject target) {
        float radius = aiAgent.closeRadius;
        float speed = aiAgent.velocity.magnitude;

        // calculate cone angle based on the agent's speed
        float coneAngle = Mathf.Lerp(aiAgent.maxAngle, aiAgent.minAngle, speed / aiAgent.maxSpeed);
        coneAngle = Mathf.Clamp(coneAngle, aiAgent.minAngle, aiAgent.maxAngle);

        Vector3 directionToTarget = target.transform.position - aiAgent.transform.position;
        float angleToTarget = Vector3.Angle(aiAgent.transform.forward, directionToTarget);

        Debug.DrawRay(aiAgent.transform.position, Quaternion.AngleAxis(coneAngle / 2, aiAgent.transform.up) * aiAgent.transform.forward * radius, Color.yellow);
        Debug.DrawRay(aiAgent.transform.position, Quaternion.AngleAxis(-coneAngle / 2, aiAgent.transform.up) * aiAgent.transform.forward * radius, Color.yellow);


        // check if ray is lying between the angle in other words
        if (CheckObstacleBetween(target)) {
            return false;
        }
        else {
            // Check if the angle to the target is within the cone angle
            if (aiAgent.isSearching || angleToTarget <= coneAngle / 2) {
                return true;
            }
            else {
                return false;
            }
        }
    }

    // return true if obstacle between
    private bool CheckObstacleBetween(GameObject target) {
        Vector3 start = aiAgent.transform.position;
        Vector3 end = target.transform.position - start;
        float maxDistance = end.magnitude + 2f;
        bool result = Physics.Raycast(start, end, out RaycastHit hit, maxDistance) && hit.collider.gameObject != target;
        return result;
    }

}
