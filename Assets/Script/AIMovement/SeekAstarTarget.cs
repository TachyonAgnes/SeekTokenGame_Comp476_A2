using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekAstarTarget : AIMovement {
    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        if(agent.aStarTarget != null) {
            Vector3 targetPos = agent.aStarTarget.transform.position;
            output.linear = ToSeek(agent, targetPos);
        }

        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        Vector3 steering = GetKinematic(agent).linear - output.linear;
        output.linear = steering;

        return output;
    }
}
