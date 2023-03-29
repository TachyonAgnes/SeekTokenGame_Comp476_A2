using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bombCol : MonoBehaviour
{
    public GameObject blizzard;
    private void OnCollisionEnter(Collision collision) {
        var instantiatePos = new Vector3(transform.position.x, 2.54331f, transform.position.z);
        var blizzardEffect = Instantiate(blizzard, instantiatePos, Quaternion.identity);
        Destroy(this.gameObject);
        Destroy(blizzardEffect,2f);
    }
}
