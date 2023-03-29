using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Node;

public class IsTargetRememberedNode : ConditionNode {
    private AIAgent agent;

    public override NodeStatus Execute() {
        if (CheckLastTarPosRecorded()) {
            return NodeStatus.SUCCESS;
        }
        return NodeStatus.FAILURE;
    }

    public IsTargetRememberedNode(AIAgent agent) {
        this.agent = agent;
    }

    private bool CheckLastTarPosRecorded() {
        if (!agent.targetLastPos.Equals(Vector3.positiveInfinity)) {
            return true;
        }
        else {
            return false;
        }
    }
 }
