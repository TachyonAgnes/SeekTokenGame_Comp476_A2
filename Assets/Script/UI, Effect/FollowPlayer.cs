using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 position;
    // Start is called before the first frame update
    void Start() {
        player = GameObject.Find("player(Clone)");
    }

    // Update is called once per frame
    void Update() {
        position = player.transform.position;
        transform.position = new Vector3(position.x, 4f, position.z);
    }
}
