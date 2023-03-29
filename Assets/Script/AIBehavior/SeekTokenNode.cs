using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekTokenNode : ActionNode {
    private AIAgent agent;

    public SeekTokenNode(AIAgent agent) {
        this.agent = agent;
    }

    public override NodeStatus Execute() {
        if (agent.spawned && agent.aStarTarget!= null) {
            MovementNode astarMovementNode = new(agent.aStarPathFinding, agent);
            MovementNode seekAstarTargetNode = new(agent.seekAstarTarget, agent);
            MovementNode lookWryGoNode = new(agent.lookWryGo, agent);
            MovementNode wallAvoidanceNode = new(agent.wallAvoidance, agent);
            var distance = Vector3.Distance(agent.transform.position, agent.aStarTarget.transform.position);
            if (distance < 20f && !CheckObstacleBetween(agent.aStarTarget)) {
                seekAstarTargetNode.Execute();
            }
            else {
                astarMovementNode.Execute();
            }

            if(distance >= 5) {
                lookWryGoNode.Execute();
                wallAvoidanceNode.Execute();
            }
            return NodeStatus.SUCCESS;
        }
        else {
            return NodeStatus.FAILURE;
        }
        
    }
    private bool CheckObstacleBetween(GameObject target) {
        Vector3 start = agent.transform.position;
        Vector3 end = target.transform.position - start;
        float maxDistance = end.magnitude + 2f;
        bool result = Physics.Raycast(start, end, out RaycastHit hit, maxDistance) && hit.collider.gameObject != target;
        return result;
    }
}
