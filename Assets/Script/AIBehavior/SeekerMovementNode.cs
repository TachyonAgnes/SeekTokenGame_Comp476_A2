using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.AI;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using System.IO;

public class SeekerMovementNode : ActionNode {
    private AIAgent agent;
    private float timer = 0f;
    private const float clearPathInterval = 0.2f;

    public SeekerMovementNode(AIAgent agent) {
        this.agent = agent;
    }

    [Range(-1, 1)]
    [Tooltip("Lower is a better hiding spot")]
    private float HideSensitivity = 0;

    [Range(5, 20)]
    private float MinChaserDistance = 25f;

    [Range(14, 100)]
    private float MaxForeseeDistance = 30f;

    public override NodeStatus Execute() {
        //DebugUtil.DrawCircle(agent.target.transform.position, agent.target.transform.up, Color.blue, MinChaserDistance);
        //DebugUtil.DrawCircle(agent.transform.position, agent.transform.up, Color.yellow, MaxForeseeDistance);
        if (agent.isCollided) {
            if (agent.target != null) {
                timer += Time.fixedDeltaTime;
                if (timer > clearPathInterval) {
                    Hide(agent.target.transform);
                    timer = 0f;
                }
            }
        }
        return NodeStatus.SUCCESS;
    }

    private void Hide(Transform target) {
        GameObject[] hidingSpots = agent.tacticalLocations;
        List<GameObject> sortedTacticalLocations = agent.tacticalLocations
            .Where(location => Vector3.Distance(agent.transform.position, location.transform.position) <= MaxForeseeDistance)
            .OrderBy(location => Vector3.Distance(agent.transform.position, location.transform.position))
            .ToList();
        Vector3 bestHidingSpot = Vector3.zero;
        float bestDotProduct = HideSensitivity;

        foreach (GameObject tacticalLocation in sortedTacticalLocations) {
            var hidingSpot = tacticalLocation.transform.position;
            // check the distance between hidingSpot is smaller than minChaserDistance
            if (Vector3.Distance(hidingSpot, target.position) < MinChaserDistance) {
                if (agent.debug) { Debug.DrawRay(hidingSpot, Vector3.up * 10f, Color.red, 5f); }
                continue;
            }

            // check dot product to make sure 
            // vec3 tgt -> hspot, vec3 hspot -> agent
            // dotProduct closer to -1 means great, closer to 1 means bad;
            Vector3 direction = (hidingSpot - target.position).normalized;
            float dotProduct = Vector3.Dot(direction, (agent.transform.position - hidingSpot).normalized);

            //check if target can see the spot
            //find smallest one
            if (Physics.Raycast(target.position, hidingSpot - target.position, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Obstacle", "TacticalLocation"))) {   
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("TacticalLocation")) {
                    if (agent.debug) { Debug.DrawRay(hidingSpot, Vector3.up * 10f, Color.red, 5f); }
                    continue;
                }
            }
            else {
                continue;
            }
            if (agent.debug) { Debug.DrawRay(hidingSpot, Vector3.up * 10f, Color.green, 3f); }
            if (dotProduct < bestDotProduct) {
                bestDotProduct = dotProduct;
                bestHidingSpot = hidingSpot;
            }
        }

        // update the best hiding spot obj as agent target
        if (bestHidingSpot != Vector3.zero) {
            agent.targetLastPos = bestHidingSpot;
        }
    }
}