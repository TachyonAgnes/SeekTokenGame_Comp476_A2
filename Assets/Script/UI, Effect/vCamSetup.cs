using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vCamSetup : MonoBehaviour
{
    public GameObject player;
    public CinemachineVirtualCamera virtualCamera;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("player(Clone)");
        //player = GameObject.Find("allies(Clone)");
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        virtualCamera.Follow = player.transform;
    }
}
