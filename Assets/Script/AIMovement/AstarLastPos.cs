using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarLastPos : AIMovement
{
    private List<GridGraphNode> path = new List<GridGraphNode>();
    public GridGraphNode startNode;
    public GridGraphNode goalNode;
    public float arriveDistance = 5.1f;
    private float timer = 0f;
    private const float clearPathInterval = 0.2f;

    // Path following
    private Vector3 FollowPath(AIAgent agent) {
        Vector3 nextNode = path[0].transform.position;
        // Check if the AI has reached the node
        if (Vector3.Distance(transform.position, nextNode) < arriveDistance) {
            // Remove the node from the path
            path.RemoveAt(0);
            // If the AI has reached the final node, stop moving
            if (path.Count == 0) {
                return Vector3.zero;
            }

            // Get the new next node
            nextNode = path[0].transform.position;
        }

        // Move towards the next node using Seek
        Vector3 steering = ToSeek(agent,nextNode);

        // Update the timer
        timer += Time.fixedDeltaTime;
        if (timer > clearPathInterval) {
            // Clear the path and reset the timer
            path.Clear();
            timer = 0f;
        }

        return steering;
    }

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        if (agent.gridGraph == null) {
            Debug.LogError("GridGraph is null!");
            return output;
        }

        if (!agent.targetLastPos.Equals(Vector3.positiveInfinity)) {

            startNode = agent.gridGraph.FindObjectAdjacentNode(agent.transform.position);
            goalNode = agent.gridGraph.FindObjectAdjacentNode(agent.targetLastPos);
            if (path.Count == 0) {
                path = agent.pathFinding.FindPath(startNode, goalNode, agent.debug, agent.isChaser, agent.pathFinding.ChebyshevDistanceHeuristic);
            }

            output.linear = FollowPath(agent);
        }
        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        Vector3 steering = GetKinematic(agent).linear - output.linear;
        if(!float.IsNaN(steering.x) || !float.IsNaN(steering.z)) {
            steering.Set(steering.x, 0, steering.z);
        }
        output.linear = steering;

        return output;
    }
}