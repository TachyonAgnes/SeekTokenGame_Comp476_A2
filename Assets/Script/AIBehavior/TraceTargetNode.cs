using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TraceTargetNode : ActionNode {
    private readonly AIAgent agent;
    public TraceTargetNode(AIAgent agent) {
        this.agent = agent;
    }

    public override NodeStatus Execute() {
        MoveTowardsTarget();
        return NodeStatus.SUCCESS;
    }

    private void MoveTowardsTarget() {
        float distanceToTarget = Vector3.Distance(agent.transform.position, agent.target.transform.position);
        MovementNode seekNode = new(agent.seek, agent);
        MovementNode pursueNode = new(agent.pursue, agent);
        MovementNode lookWryGoNode = new(agent.lookWryGo, agent);
        MovementNode wallAvoidanceNode = new(agent.wallAvoidance, agent);
        MovementNode obstacleAvoidance = new(agent.obstacleAvoidance, agent);

        if (agent.actualSpeed <= agent.slowSpeedThreshold && distanceToTarget < agent.nearRadius) {
            seekNode.Execute();
            lookWryGoNode.Execute();
        }
        else { // 2. If the agent is moving fast
            pursueNode.Execute();
            lookWryGoNode.Execute();
        }
        obstacleAvoidance.Execute();
        wallAvoidanceNode.Execute();
    }
}
