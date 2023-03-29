using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentSpawner : MonoBehaviour {
    [SerializeField] private GameObject chaserPrefab;
    [SerializeField] private GameObject evaderPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GridGraph gridGraph;
    [SerializeField] private int chaserSpawnTotal = 2;
    [SerializeField] private int seekerSpawnTotal = 7;


    [HideInInspector]
    public List<GameObject> chasers;
    public List<GameObject> seekers;

    private GameObject[] safeAreas;

    private List<GridGraphNode> nodes;

    private void Awake() {
        nodes = gridGraph.nodes;
        safeAreas = GameObject.FindGameObjectsWithTag("SaveArea");
        SpawnAgent();
    }

    // Randomly pick nodes position on the grid graph to instantiate agents;
    public void SpawnAgent() {
        // Get all nodes within the safe area
        var nodesWithinSafeArea = new List<GridGraphNode>();
        foreach (var safeArea in safeAreas) {
            var safeAreaCol = safeArea.GetComponent<Collider>();
            foreach (var node in nodes) {
                if (safeAreaCol.bounds.Contains(node.transform.position)) {
                    nodesWithinSafeArea.Add(node);
                }
            }
        }

        // If there are not enough nodes within the safe area, spawn all agents randomly
        if (nodesWithinSafeArea.Count < seekerSpawnTotal) {
            Debug.LogWarning("Not enough nodes within the safe area, spawn all agents randomly.");
            nodesWithinSafeArea = nodes;
        }

        // Randomly select nodes to spawn agents
        nodesWithinSafeArea = nodesWithinSafeArea.OrderBy(node => UnityEngine.Random.value).ToList();
        var nodesOutsideSafeArea = nodes.Except(nodesWithinSafeArea).OrderBy(node => UnityEngine.Random.value).ToList();


        // Spawn Chaser
        for (int i = 0; i < nodesOutsideSafeArea.Count && i < chaserSpawnTotal; i++) {
            chasers.Add(Instantiate(chaserPrefab, nodesOutsideSafeArea[i].transform.position, Quaternion.identity));
        }
        // Spawn Player
        seekers.Add(Instantiate(playerPrefab, nodesWithinSafeArea[0].transform.position, Quaternion.identity));
        // Spawn Seeker
        for (int i = 1; i < nodesWithinSafeArea.Count && i < seekerSpawnTotal; i++) {
            seekers.Add(Instantiate(evaderPrefab, nodesWithinSafeArea[i].transform.position, Quaternion.identity));
        }

        foreach (var safeArea in safeAreas) {
            Destroy(safeArea, 1.5f);
        }

        }
}