using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Token : MonoBehaviour
{

    public TokenSpawner spawner;
    private GameAgent agent;
    public GameObject player;

    private void Start() {
      agent = FindObjectOfType<GameAgent>();
        player = GameObject.Find("player");
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Evader")){
            
            if(agent != null && (collision.gameObject == player)) {
                agent.AddScore(collision.gameObject.tag, spawner.tokenTotal);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Chaser")) {
            if (agent != null) {
                agent.AddScore(collision.gameObject.tag, spawner.tokenTotal);
            }
            collision.gameObject.GetComponent<AIAgent>().marker.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if (!this.gameObject.scene.isLoaded) return;
        spawner.canSpawn = true;
    }
}
