using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MovToLasKnownPosNode : ActionNode {
    private AIAgent agent;
    public MovToLasKnownPosNode(AIAgent agent) {
        this.agent = agent;
    }

    public override NodeStatus Execute() {
        if (Vector3.Distance(agent.transform.position, agent.targetLastPos) > 1.1f) {
            MovToLasKnownPos();
            return NodeStatus.RUNNING;
        }
        else {
            return NodeStatus.SUCCESS;
        }
    }

    // Move to the last known position of the target
    // both token tokenAcquiredPos and targetLastPos will be reseted In waitNRotateNode
    // if chaser find seeker in isTargetInSightNode, reset tokenAcquiredPos to positiveInfinity
    private void MovToLasKnownPos() {
        MovementNode astarLastPosNode = new(agent.aStarLastPos, agent);
        MovementNode seekLastPosNode = new(agent.seekLastPos, agent);
        // If the seek has acquired token, move to the last known position
        if (!agent.tokenAcquiredPos.Equals(Vector3.positiveInfinity)) {
            agent.targetLastPos = agent.tokenAcquiredPos;
            
            if (Vector3.Distance(agent.transform.position, agent.targetLastPos) < 5.1f
                && !CheckObstacleBetween(agent.targetLastPos)) {
                seekLastPosNode.Execute();
            }
            else {
                astarLastPosNode.Execute();
            }
        }
        else {
            FindNearestLasKnownPos();

            if (!agent.targetLastPos.Equals(Vector3.positiveInfinity)) {
                seekLastPosNode.Execute();
            }
        }
        MovementNode lookWryGoNode = new(agent.lookWryGo, agent);
        MovementNode wallAvoidanceNode = new(agent.wallAvoidance, agent);
        MovementNode obstacleAvoidanceNode = new(agent.obstacleAvoidance, agent);

        lookWryGoNode.Execute();
        wallAvoidanceNode.Execute();
        obstacleAvoidanceNode.Execute();
    }

    // Find the nearest last known position in the gameAgent.lastPosCollector
    // Note that every AIAgent maintains a Vec3 tarLastPos and a gameObject nearAllies, and gameAgent(singleton) maintains a lastPosCollector
    // isTargetInSightNode class update every near ally's gameObject to the agent.nearAllies
    // and in the update function of AIAgent, it will update the tarLastPos to the lastPosCollector
    // so that this function can find the nearest last known position in the lastPosCollector
    private void FindNearestLasKnownPos() {
        if (agent.nearAllies.Count > 0) {
            Vector3 nearestTargetPos = agent.targetLastPos;
            float distanceToTarget = Vector3.Distance(agent.transform.position, agent.targetLastPos);

            float nearestTargetDistance = distanceToTarget;
            String colst = agent.gameObject.name + " " + distanceToTarget + " ";
            foreach (GameObject obj in agent.nearAllies) {
                if (obj.CompareTag("Chaser") && obj != agent.gameObject) {
                    var tarLastPos = agent.gameAgent.lastPosCollector.ContainsKey(obj.GetComponent<AIAgent>())
                        ? agent.gameAgent.lastPosCollector[obj.GetComponent<AIAgent>()]
                        : Vector3.zero;
                    if (!tarLastPos.Equals(Vector3.positiveInfinity)) {
                        distanceToTarget = Vector3.Distance(agent.transform.position, tarLastPos);
                        if (distanceToTarget < nearestTargetDistance) {
                            nearestTargetDistance = distanceToTarget;
                            nearestTargetPos = tarLastPos;
                            colst += nearestTargetDistance + " ";
                        }
                    }
                }
            }
            agent.targetLastPos = nearestTargetPos;
            // an example: enemy(Clone) 1.670743 1.658309 ,picked: 1.658309
            // so it is working
            //if (agent.debug) {
            //    Debug.Log(colst + ",picked: " + nearestTargetDistance);
            //}
        }
        else {
            agent.targetLastPos = Vector3.positiveInfinity;
        }

    }
    // Check if there is an obstacle between the agent and the last known position
    private bool CheckObstacleBetween(Vector3 lastPos) {
        Vector3 start = agent.transform.position;
        Vector3 end = lastPos - start;
        float maxDistance = end.magnitude + 2f;
        bool result = Physics.Raycast(start, end, out RaycastHit hit, maxDistance) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle");
        return result;
    }

}
