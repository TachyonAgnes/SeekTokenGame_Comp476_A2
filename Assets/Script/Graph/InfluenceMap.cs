using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
//using UnityEditor.Experimental.GraphView;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking.Types;

public class InfluenceMap : MonoBehaviour {
    private float chaserInfluenceRadius = 10.0f;
    private float seekerInfluenceRadius = 5.0f;
    // chaserInfluenceLevcl best records: 80.0f
    private float chaserInfluenceLevcl = 80.0f;
    private float seekerInfluenceLevcl = 20.0f;

    [HideInInspector]
    public static Dictionary<GridGraphNode, float> chaserInfluenceMap = new Dictionary<GridGraphNode, float>();
    public static Dictionary<GridGraphNode, float> seekerInfluenceMap = new Dictionary<GridGraphNode, float>();
    public static Dictionary<GridGraphNode, float> influenceInTotalMap = new Dictionary<GridGraphNode, float>();

    private GridGraph gridGraph;
    private AgentSpawner agentSpawner;
    private List<List<GridGraphNode>> nodeGrid;
    private float timer = 0f;
    private const float clearInterval = 0.05f;
    private int gridRows;
    private int gridCols;
    private LayerMask obstacleLayer;


    private void Start() {
        agentSpawner = FindObjectOfType<AgentSpawner>();
        gridGraph = FindObjectOfType<GridGraph>();
        obstacleLayer = LayerMask.GetMask("Obstacle");
        nodeGrid = gridGraph.nodeGrid;
        gridRows = gridGraph.nodeGrid.Count;
        gridCols = gridGraph.nodeGrid[0].Count;
        UpdateInfluenceMap();
    }

    private void FixedUpdate() {
        timer += Time.fixedDeltaTime;
        if (timer > clearInterval) {
            UpdateInfluenceMap();
            timer = 0f;
        }
    }

    //private void OnDrawGizmos() {
    //    for (int r = 0; r < gridRows; ++r) {
    //        for (int c = 0; c < gridCols; ++c) {
    //            if (nodeGrid[r][c] != null) {
    //                Gizmos.color = Color.white;
    //                Handles.Label(new Vector3(nodeGrid[r][c].transform.position.x, 1f, nodeGrid[r][c].transform.position.z), influenceInTotalMap[nodeGrid[r][c]].ToString());
    //            }
    //        }
    //    }
    //}
    public static void SmoothDictionary(Dictionary<GridGraphNode, float> dict, int iterations) {
        Dictionary<GridGraphNode, float> smoothedDict = new Dictionary<GridGraphNode, float>(dict);

        for (int i = 0; i < iterations; i++) {
            foreach (GridGraphNode node in dict.Keys) {
                float sum = 0f;
                int count = 0;
                foreach (GridGraphNode neighbor in node.adjacencyList) {
                    if (dict.ContainsKey(neighbor)) {
                        sum += dict[neighbor];
                        count++;
                    }
                }
                smoothedDict[node] = sum / count;
            }
            dict = new Dictionary<GridGraphNode, float>(smoothedDict);
        }
    }



    private void UpdateInfluenceMap() {
        float maxInfluence = float.MinValue;
        float minInfluence = float.MaxValue;

        for (int r = 0; r < gridRows; ++r) {
            for (int c = 0; c < gridCols; ++c) {
                float chaserTotalInfluence = 0.0f;
                float seekerTotalInfluence = 0.0f;

                if (nodeGrid[r][c] != null) {
                    // calculate chaser influence
                    foreach (GameObject chaserAgent in agentSpawner.chasers) {
                        if (chaserAgent == null) { continue; }
                        if (chaserAgent.CompareTag("Frozen")) { continue; }
                        Vector3 chaserPosition = chaserAgent.transform.position;
                        float distance = Vector3.Distance(nodeGrid[r][c].transform.position, chaserPosition);
                        Vector3 direction = (nodeGrid[r][c].transform.position - chaserPosition).normalized;
                        if (distance <= chaserInfluenceRadius) {
                            RaycastHit hitInfo;
                            if (!Physics.Raycast(chaserPosition, direction, out hitInfo, distance, obstacleLayer)) {
                                chaserTotalInfluence += chaserInfluenceLevcl / (1.0f + distance);
                            }
                        }
                        //DebugUtil.DrawCircle(chaserAgent.transform.position, transform.up, Color.blue, chaserInfluenceRadius);
                    }

                    // calculate seeker influence
                    foreach (GameObject seekerAgent in agentSpawner.seekers) {
                        if(seekerAgent == null) { continue; }
                        Vector3 seekerPosition = seekerAgent.transform.position;
                        float distance = Vector3.Distance(nodeGrid[r][c].transform.position, seekerPosition);
                        Vector3 direction = (nodeGrid[r][c].transform.position - seekerPosition).normalized;
                        if (distance <= seekerInfluenceRadius) {
                            RaycastHit hitInfo;
                            if (!Physics.Raycast(seekerPosition, direction, out hitInfo, distance, obstacleLayer)) {
                                    seekerTotalInfluence += seekerInfluenceLevcl / (1.0f + distance);
                            }
                        }
                         //DebugUtil.DrawCircle(seekerAgent.transform.position, transform.up, Color.yellow, seekerInfluenceRadius);
                    }

                    chaserInfluenceMap[nodeGrid[r][c]] = chaserTotalInfluence / agentSpawner.chasers.Count;
                    seekerInfluenceMap[nodeGrid[r][c]] = seekerTotalInfluence / agentSpawner.seekers.Count;
                    influenceInTotalMap[nodeGrid[r][c]] = chaserInfluenceMap[nodeGrid[r][c]] - seekerInfluenceMap[nodeGrid[r][c]];

                    // update max and min values
                    if (influenceInTotalMap[nodeGrid[r][c]] > maxInfluence) {
                        maxInfluence = influenceInTotalMap[nodeGrid[r][c]];
                    }
                    if (influenceInTotalMap[nodeGrid[r][c]] < minInfluence) {
                        minInfluence = influenceInTotalMap[nodeGrid[r][c]];
                    }
                    Color color = Color.Lerp(Color.black, Color.green, influenceInTotalMap[nodeGrid[r][c]]);
                    nodeGrid[r][c]._nodeGizmoColor = new Color(color.r, color.g, color.b, 1f);
                }
            }
        }
        for (int r = 0; r < gridRows; ++r) {
            for (int c = 0; c < gridCols; ++c) {
                if (nodeGrid[r][c] != null) {
                    influenceInTotalMap[nodeGrid[r][c]] = (float)Math.Round((influenceInTotalMap[nodeGrid[r][c]] - minInfluence) / (maxInfluence - minInfluence), 2);
                }
            }
        }
    }
}
