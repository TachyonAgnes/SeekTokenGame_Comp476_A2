using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Token : MonoBehaviour
{
    public TokenSpawner spawner;
    private GameAgent agent;
    private AIAgent[] aiAgents;
    public GameObject player;

    // define a TokenAcquired type delegate, is to notify if there is a token acquired
    public delegate void TokenAcquired(Vector3 position, GameObject nearlestChaser);
    public static event TokenAcquired OnTokenAcquired;

    private void Start() {
        agent = FindObjectOfType<GameAgent>();
        player = GameObject.Find("player");
        aiAgents = GameAgent.FindObjectsOfType<AIAgent>();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Evader")){
            if(agent != null) {
                if (collision.gameObject.name == "player(Clone)" || collision.gameObject.name == "player") {
                    agent.AddScore(collision.gameObject.name);
                }
                else {
                    agent.AddScore(collision.gameObject.tag);
                }
            }
            foreach (AIAgent aiAgent in aiAgents) { 
                aiAgent.spawned = false;
                aiAgent.aStarTarget = null;
            }
            //token acquired, notify chaser;
            OnTokenAcquired?.Invoke(transform.position, FindNearlestChaser(gameObject));
            Destroy(gameObject);
        }
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

    private void OnDestroy() {
        if (!this.gameObject.scene.isLoaded) return;
        spawner.canSpawn = true;
    }
}
