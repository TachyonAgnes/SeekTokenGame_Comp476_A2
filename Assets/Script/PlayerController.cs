using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float moveSpeed = 5f;
    public float turnSpeed = 5f;
    public float speedSmoothTime = 0.1f;
    private Animator playerAnim;
    private float currentSpeed;
    private float speedSmoothVelocity;
    public bool isKnockedOut = false;
    private GameAgent gameAgent;
    public GameObject dirIndicator;
    public GameObject dirIndicator2;
    private Coroutine unfreezeCoroutine;
    public int bombInTotal = 2;
    private ScoreBoard scoreBoard = null;

    // Start is called before the first frame update
    void Start() {
        playerAnim = GetComponent<Animator>();
        gameAgent = FindObjectOfType<GameAgent>();
        scoreBoard = FindObjectOfType<ScoreBoard>();
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.G) && bombInTotal > 0) {
            playerAnim.SetTrigger("throwing");
            bombInTotal--;
            scoreBoard.blizzardBomb.text = "Blizzard Bomb  |  " + bombInTotal;
        }
        bool isAnimationPlaying = playerAnim.GetCurrentAnimatorStateInfo(0).IsName("throwing");
        if (isAnimationPlaying != true) {
            Vector3 movement = Vector3.zero;
            if (!isKnockedOut) {  
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            movement = new Vector3(horizontal, 0, vertical).normalized;
       
            // Calculate target speed
            float targetSpeed = movement.magnitude * moveSpeed;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
            
            // Apply movement and rotation
            transform.Translate(movement * currentSpeed * Time.deltaTime, Space.World);
            }
            if (movement.magnitude != 0 && !isKnockedOut) {
                Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                playerAnim.SetBool("running", true);
            }
            else {
                playerAnim.SetBool("running", false);
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Chaser") {
            // set color
            dirIndicator.GetComponent<SpriteRenderer>().color = Color.gray;
            dirIndicator2.GetComponent<SpriteRenderer>().color = Color.gray;

            //set tag and layer
            gameObject.tag = "KnockedOut";
            gameObject.layer = LayerMask.NameToLayer("KnockedOut");

            // change bool
            isKnockedOut = true;
        }
    }
}