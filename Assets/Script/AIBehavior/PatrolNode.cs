using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PatrolNode : ActionNode {
    private AIAgent agent;
    private List<GameObject> patrolPoints;
    private List<GameObject> tempPatrolPoints;
    private GameObject closestPoint = null;

    public PatrolNode(AIAgent agent, List<GameObject> patrolPoints) {
        this.agent = agent;
        this.patrolPoints = patrolPoints;
        tempPatrolPoints = new List<GameObject>();
        tempPatrolPoints.AddRange(patrolPoints);
    }

    public override NodeStatus Execute() {
        agent.aStarTarget = FindClosestPatrolNode(agent.transform.position, patrolPoints);
        MovementNode astarMovement = new MovementNode(agent.aStarPathFinding, agent);
        MovementNode lookWryGoNode = new MovementNode(agent.lookWryGo, agent);
        MovementNode wallAvoidance = new MovementNode(agent.wallAvoidance, agent);
        MovementNode obstacleAvoidanceNode = new MovementNode(agent.obstacleAvoidance, agent);
        astarMovement.Execute();
        lookWryGoNode.Execute();
        wallAvoidance.Execute();
        obstacleAvoidanceNode.Execute();
        return NodeStatus.SUCCESS;
    }

    private GameObject FindClosestPatrolNode(Vector3 currentPosition, List<GameObject> patrolPoints) {
        float closestDistance = Mathf.Infinity;
        if (agent.aStarTarget == null) {
            foreach (GameObject point in patrolPoints) {
                float distance = Vector3.Distance(currentPosition, point.transform.position);
                if (distance < closestDistance) {
                    closestPoint = point;
                    closestDistance = distance;
                }
            }
        }

        if (Vector3.Distance(agent.transform.position, closestPoint.transform.position) < 5f) {
            // Remove the node from the path
            patrolPoints.Remove(closestPoint);
            // If the AI has reached the final node, stop moving
            if (patrolPoints.Count == 0) {
                patrolPoints.AddRange(tempPatrolPoints);
            }
            foreach (GameObject point in patrolPoints) {
                float distance = Vector3.Distance(currentPosition, point.transform.position);
                if (distance < closestDistance) {
                    closestPoint = point;
                    closestDistance = distance;
                }
            }
        }

        return closestPoint;
    }


}
