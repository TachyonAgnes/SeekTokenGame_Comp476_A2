using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class TokenSpawner : MonoBehaviour {
    [SerializeField] 
    public GameObject tokenPrefab;
    public List<GridGraphNode> nodes;
    public GridGraph gridGraph;

    public bool canSpawn = true;

    private int nodeCount;
    private int randomNodeIndex;

    public int tokenTotal = 0;

    public float spawnInterval = 5.0f;
    public GameObject tokenSpawned;

    // define a TokenSpawned type delegate, is to notify if there is a token generated
    public delegate void TokenSpawned(bool spawned, GameObject tokenSpawned, GameObject closestChaser);
    public static event TokenSpawned OnTokenSpawned;

    void Start() {
        nodes = gridGraph.nodes;
        nodeCount = nodes.Count;
    }
    private void Update() {
        if (canSpawn) {
            StartCoroutine(DelaySpawnTimer());
        }
    }

    public void SpawnToken() {
        if (!canSpawn) { return; }
        randomNodeIndex = UnityEngine.Random.Range(0, nodeCount);
        GameObject newToken = Instantiate(tokenPrefab, nodes[randomNodeIndex].transform.position, Quaternion.identity);
        newToken.GetComponent<Token>().spawner = this;
        tokenSpawned = newToken;
        canSpawn = false;
        tokenTotal++;

        //token generated, notify chaser;
        OnTokenSpawned?.Invoke(true, newToken, FindNearlestChaser(newToken));
    }

    public GameObject FindNearlestChaser(GameObject token) {
        GameObject[] chasers = GameObject.FindGameObjectsWithTag("Chaser");
        if (chasers != null && chasers.Length > 1) {
            float closestDistance = Mathf.Infinity;
            GameObject closestChaser = null;
            foreach (GameObject chaser in chasers) {
                float distance = Vector3.Distance(token.transform.position, chaser.transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestChaser = chaser;
                }
            }
            return closestChaser;
        }
        return null;
    }

    private IEnumerator DelaySpawnTimer() {
        yield return new WaitForSeconds(spawnInterval);
        SpawnToken();
    }




}