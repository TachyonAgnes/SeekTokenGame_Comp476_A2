using System;
using UnityEngine;

public abstract class AIMovement : MonoBehaviour {
    public bool debug;

    public Vector3 ToSeek(AIAgent agent, Vector3 targetPos) {
        Vector3 desired = targetPos - transform.position;
        desired = desired.normalized * agent.maxSpeed;
        return desired;
    }
    public virtual SteeringOutput GetKinematic(AIAgent agent) {
        return new SteeringOutput { angular = agent.transform.rotation };
    }

    public virtual SteeringOutput GetSteering(AIAgent agent) {
        return new SteeringOutput { angular = Quaternion.identity };
    }

}
