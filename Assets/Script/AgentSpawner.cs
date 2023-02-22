using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentSpawner : MonoBehaviour {
    [SerializeField] private GameObject chaserPrefab;
    [SerializeField] private GameObject evaderPrefab;
    [SerializeField] private GridGraph gridGraph;
    [SerializeField] private int spawnTotal = 11;

    private List<GridGraphNode> nodes;

    private void Awake() {
        nodes = gridGraph.nodes;
        SpawnAgent();
    }

    // Randomly pick nodes position on the grid graph to instantiate agents;
    public void SpawnAgent() {
        var randomNodes = nodes.OrderBy(node => UnityEngine.Random.value).Take(spawnTotal).ToList();
        Instantiate(chaserPrefab, randomNodes[0].transform.position, Quaternion.identity);
        for (int i = 1; i < randomNodes.Count; i++) {
            Instantiate(evaderPrefab, randomNodes[i].transform.position, Quaternion.identity);
        }
    }
}