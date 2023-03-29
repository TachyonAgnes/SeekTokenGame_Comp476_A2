using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBomb : MonoBehaviour
{
    public GameObject bombPrefab; 
    public GameObject bombSpawnPoint;
    public GameObject agent;
    private float throwForce = 2.0f;

    public void Throw() {
        GameObject bombInstance = Instantiate(bombPrefab, bombSpawnPoint.transform.position, bombSpawnPoint.transform.rotation);
        Rigidbody bombRigidbody = bombInstance.GetComponent<Rigidbody>();
        bombRigidbody.AddForce(agent.transform.forward * throwForce, ForceMode.Impulse);
    }


}
