using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float moveSpeed = 2.5f;
    public float turnSpeed = 5f;
    public float speedSmoothTime = 0.1f;
    private Animator playerAnim;
    private float currentSpeed;
    private float speedSmoothVelocity;
    public bool isFrozen = false;
    private GameAgent gameAgent;
    public SpriteRenderer dirIndicator;
    private Coroutine unfreezeCoroutine;

    // Start is called before the first frame update
    void Start() {
        playerAnim = GetComponent<Animator>();
        gameAgent = FindObjectOfType<GameAgent>();
        dirIndicator = transform.Find("Head").GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        Vector3 movement = Vector3.zero;
        if (!isFrozen) {  
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        movement = new Vector3(horizontal, 0, vertical).normalized;
       
        // Calculate target speed
        float targetSpeed = movement.magnitude * moveSpeed;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
            
        // Apply movement and rotation
        transform.Translate(movement * currentSpeed * Time.deltaTime, Space.World);
        }
        if (movement.magnitude != 0 && !isFrozen) {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            playerAnim.SetBool("running", true);
        }
        else {
            playerAnim.SetBool("running", false);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Chaser") {
            dirIndicator.color = Color.gray;
            isFrozen = true;
            tag = "Frozen";
            gameObject.layer = LayerMask.NameToLayer("Frozen");
            gameAgent.isSomeOneFrozen = true;
            collision.gameObject.GetComponent<AIAgent>().TargetReset();
            unfreezeCoroutine = StartCoroutine(UnfreezeAfterDelay());
        }
        if (CompareTag("Frozen") && collision.gameObject.tag == "Evader") {
            dirIndicator.color = Color.green;
            isFrozen = false;
            tag = "Evader";
            gameObject.layer = LayerMask.NameToLayer("Player");
            gameAgent.isSomeOneFrozen = false;
            StopCoroutine(unfreezeCoroutine);
            unfreezeCoroutine = null;
        }
    }

    private IEnumerator UnfreezeAfterDelay() {
        yield return new WaitForSeconds(15f);

        if (isFrozen) {
            dirIndicator.color = Color.red;
            tag = "Chaser";
            gameObject.layer = LayerMask.NameToLayer("Chaser");
            gameAgent.isSomeOneFrozen = false;
            isFrozen = false;
        }
    }
}