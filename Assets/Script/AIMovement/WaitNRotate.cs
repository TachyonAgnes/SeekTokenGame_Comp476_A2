using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitNRotate : AIMovement {

    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        output.linear = GetKinematic(agent).linear - output.linear;

        return output;
    }

}
