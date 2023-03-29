using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MovementNode : ActionNode
{
    private AIMovement movement;
    private AIAgent agent;

    public MovementNode(AIMovement movement, AIAgent agent) {
        this.movement = movement;
        this.agent = agent;
    }

    public override NodeStatus Execute() {
        if (agent != null && movement != null) {
            agent.applyMovement.Add(movement);
            if (agent.debug) {
                Debug.Log("Agent " + agent.name + " is executing " + movement.ToString() + " behavior.");
            }
            return NodeStatus.SUCCESS;
        }
        return NodeStatus.FAILURE;
    }
}
