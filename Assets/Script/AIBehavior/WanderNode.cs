using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderNode : ActionNode {
    private AIAgent agent;

    public WanderNode(AIAgent agent) {
        this.agent = agent;
    }

    public override NodeStatus Execute() {
        MovementNode wanderNode = new MovementNode(agent.wander, agent);
        MovementNode lookWryGoNode = new MovementNode(agent.lookWryGo, agent);
        MovementNode wallAvoidanceNode = new MovementNode(agent.wallAvoidance, agent);
        MovementNode obstacleAvoidanceNode = new MovementNode(agent.obstacleAvoidance, agent);
        wanderNode.Execute();
        lookWryGoNode.Execute();
        wallAvoidanceNode.Execute();
        obstacleAvoidanceNode.Execute();
        return NodeStatus.SUCCESS;
    }

}