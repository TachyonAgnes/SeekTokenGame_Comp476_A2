using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class IsTokenExistNode : ConditionNode {
    private AIAgent agent;

    public override NodeStatus Execute() {
        if (CheckTokenExist()) {
            return NodeStatus.SUCCESS;
        }
        else {
            return NodeStatus.FAILURE;
        }
    }

    public IsTokenExistNode(AIAgent agent) {
        this.agent = agent;
    }

    private bool CheckTokenExist() {
        if (agent.spawned) {
            return true;
        }
        else {
            return false;
        }
    }
}
