﻿using UnityEngine;

public class LookWhereYouAreGoing : AIMovement {
    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);

        if (agent.velocity == Vector3.zero)
            return output;

        if (agent.velocity != Vector3.zero)
            output.angular = Quaternion.LookRotation(agent.velocity);

        return output;
    }

    public override SteeringOutput GetSteering(AIAgent agent) {
        var output = base.GetSteering(agent);

        if (agent.lockY) {
            // get the rotation around the y-axis
            Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = GetKinematic(agent).angular * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            output.angular = Quaternion.AngleAxis(angleY, Vector3.up);
        }
        else
            output.angular = Quaternion.FromToRotation(agent.transform.forward, GetKinematic(agent).angular * Vector3.forward);

        return output;
    }
}
        