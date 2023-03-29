using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EscapeNode : ActionNode {
    private AIAgent agent;

    public EscapeNode(AIAgent agent) {
        this.agent = agent;
    }

    public override NodeStatus Execute() {
        MovementNode astarMovementNode = new(agent.aStarLastPos, agent);
        MovementNode seekLasPosNode = new(agent.seekLastPos, agent);
        MovementNode lookWryGoNode = new(agent.lookWryGo, agent);
        //MovementNode obstacleAvoidance = new(agent.obstacleAvoidance, agent);
        MovementNode wallAvoidanceNode = new(agent.wallAvoidance, agent);
        if (!agent.targetLastPos.Equals(Vector3.positiveInfinity)) {
            if(!CheckObstacleBetween(agent.targetLastPos)) {
                seekLasPosNode.Execute();
            }
            else {
                astarMovementNode.Execute();
            }
        }
        lookWryGoNode.Execute();
        //obstacleAvoidance.Execute();
        wallAvoidanceNode.Execute();
        return NodeStatus.SUCCESS;
    }

    private bool CheckObstacleBetween(Vector3 target) {
        Vector3 start = agent.transform.position;
        Vector3 end = target - start;
        float maxDistance = end.magnitude + 2f;
        bool result = Physics.Raycast(start, end, out RaycastHit hit, maxDistance) && hit.collider.gameObject.CompareTag("Obstacle");
        return result;
    }
}
