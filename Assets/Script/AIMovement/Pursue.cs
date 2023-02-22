using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pursue : AIMovement {
    public float maxSeeAheadPursue = 1f;
    public override SteeringOutput GetKinematic(AIAgent agent) {
        var output = base.GetKinematic(agent);
        output.linear = Vector3.zero;
        GameObject target = agent.target;
        if (target != null) {
            Vector3 targetPos;
            Vector3 toTarget = agent.target.transform.position - transform.position;
            float distance = toTarget.magnitude;
            float speed = output.linear.magnitude;
            float prediction = Mathf.Min(maxSeeAheadPursue, distance / speed);

            // if AI, see Ahead through velocity.
            if (target.GetComponent<AIAgent>()) {
                AIAgent targetAI = target.GetComponent<AIAgent>();
                targetPos = target.transform.position + targetAI.velocity * prediction;
            }
            // if human, see ahead through target's forward vector
            else {
                targetPos = target.transform.position + target.transform.forward * prediction;
            }
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
