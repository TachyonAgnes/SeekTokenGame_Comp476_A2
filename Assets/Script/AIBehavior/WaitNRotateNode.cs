using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitNRotateNode : ActionNode {
    private AIAgent agent;
    private float searchTime;
    private float maxSearchTime;

    public WaitNRotateNode(AIAgent agent, float maxSearchTime) {
        this.agent = agent;
        this.maxSearchTime = maxSearchTime;
        searchTime = 0f;
    }

    public override NodeStatus Execute() {
        if (searchTime < maxSearchTime) {
            agent.isSearching = true;
            searchTime += Time.deltaTime;
            return NodeStatus.RUNNING;
        }
        else {
            //reset search timer
            searchTime = 0f;
            agent.targetLastPos = Vector3.positiveInfinity;
            agent.tokenAcquiredPos = Vector3.positiveInfinity;
            agent.isSearching = false;
            return NodeStatus.SUCCESS;
        }
            
    }

}
