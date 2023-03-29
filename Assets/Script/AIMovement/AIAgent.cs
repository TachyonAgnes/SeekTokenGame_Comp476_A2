using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;
using static TokenSpawner;
using static UnityEngine.EventSystems.EventTrigger;
using Debug = UnityEngine.Debug;

public class AIAgent : MonoBehaviour {
    public bool debug = false;
    public bool debug2 = false;

    // Movement Variables
    [Header("Movement Variables")]
    public float maxForce = 10f;
    public float maxSpeed = 5f;
    public float actualSpeed;
    public float slowSpeedThreshold = 1.8f;

    [HideInInspector]
    public Vector3 velocity;
    public bool isFacingTarget;
    public bool isFacingAwayFromTarget;
    public bool isBeingChased = false;
    public bool isKnockedOut = false;
    public bool isUnfreezed = false;
    public HashSet<AIMovement> applyMovement;
    public bool isSearching = false;

    // Radius Variables
    [Header("Safe Radius")]
    public float nearRadius = 5f;
    public float closeRadius = 15f;
    public float farRadius = 40f;

    // Cone Variables
    [Header("Field Of View")]
    public float maxAngle = 190f;
    public float minAngle = 170f;

    // Room Variables
    //private GameObject visitedAxis = null;
    //private Vector3 bestAxisPosition;
    //private float visitedAxisResetTime = 0f;
    //public float visitedAxisResetDelay = 20f;
    //public float visitedAxisDistanceThreshold = 5.1f;

    //Others
    [HideInInspector]
    public GameObject target;
    public Vector3 targetLastPos = Vector3.positiveInfinity;
    public Vector3 tokenAcquiredPos = Vector3.positiveInfinity;
    public List<GameObject> nearAllies;
    public GameObject aStarTarget;
    public  bool lockY = true;
    public float dotProduct;
    public GameObject dirIndicator;
    public GameObject dirIndicator2;
    public AIMovement waitNRotate, flee, seek, seekAstarTarget, seekLastPos, pursue, wallAvoidance, obstacleAvoidance, lookWryGo, wander, aStarPathFinding, aStarLastPos,turnTowardTarget, turnAwayFromTarget, flocking;
    public Coroutine unfreezeCoroutine;
    public bool spawned = false;
    public Collider[] colliders;
    public bool isCollided = false;
    public bool isObstacleBetween = false;
    public bool isChaser;

    // initialized in awake
    [HideInInspector]
    public LayerMask obstacleMask;
    public PathFinding pathFinding;
    public GridGraph gridGraph;
    public Animator anim;
    private Dictionary<Type, AIMovement> movementsDic = new Dictionary<Type, AIMovement>();
    private Vector3 previousPosition;
    public GameAgent gameAgent;
    public TokenSpawner tokenSpawner;
    public GameObject[] tacticalLocations;
    private AgentSpawner agentSpawner;
    public Rigidbody rb;


    #region Initialize

    // this method initialize the AIMovement by getting all the almovement components attached to the AI agent,
    // and storing them in a dictionary for a quick access;
    private void AIMovementInit() {
        AIMovement[] components = GetComponents<AIMovement>();
        foreach (AIMovement component in components) {
            movementsDic[component.GetType()] = component;
        }
        (waitNRotate, flee, seek, seekAstarTarget, seekLastPos, pursue, wallAvoidance, obstacleAvoidance, lookWryGo, wander, aStarPathFinding, aStarLastPos, turnTowardTarget, turnAwayFromTarget, flocking) =
            (
            movementsDic.GetValueOrDefault(typeof(WaitNRotate)) as WaitNRotate,
             movementsDic.GetValueOrDefault(typeof(Flee)) as Flee,
             movementsDic.GetValueOrDefault(typeof(Seek)) as Seek,
             movementsDic.GetValueOrDefault(typeof(SeekAstarTarget)) as SeekAstarTarget,
             movementsDic.GetValueOrDefault(typeof(SeekLastPos)) as SeekLastPos,
             movementsDic.GetValueOrDefault(typeof(Pursue)) as Pursue,
             movementsDic.GetValueOrDefault(typeof(WallAvoidance)) as WallAvoidance,
             movementsDic.GetValueOrDefault(typeof(ObstacleAvoidance)) as ObstacleAvoidance,
             movementsDic.GetValueOrDefault(typeof(LookWhereYouAreGoing)) as LookWhereYouAreGoing,
             movementsDic.GetValueOrDefault(typeof(Wander)) as Wander,
             movementsDic.GetValueOrDefault(typeof(AstarMovement)) as AstarMovement,
             movementsDic.GetValueOrDefault(typeof(AstarLastPos)) as AstarLastPos,
             movementsDic.GetValueOrDefault(typeof(TurnTowardTarget)) as TurnTowardTarget,
             movementsDic.GetValueOrDefault(typeof(TurnAwayFromTarget)) as TurnAwayFromTarget,
             movementsDic.GetValueOrDefault(typeof(Flock)) as Flock
             );
    }

    // actual speed = speed of agent based on delta positions;
    public float ActualSpeed() {
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, previousPosition);
        float time = Time.fixedDeltaTime;
        float speed = distance / time;

        previousPosition = currentPosition;

        return speed;
    }

    public void StopMoving() {
        velocity = Vector3.zero;
    }

    private void Start() {
        obstacleMask = LayerMask.GetMask("Obstacle");
        anim = GetComponent<Animator>();
        pathFinding = FindObjectOfType<PathFinding>();
        gridGraph = FindObjectOfType<GridGraph>();
        previousPosition = transform.position;
        gameAgent = FindObjectOfType<GameAgent>();
        tokenSpawner = FindObjectOfType<TokenSpawner>();
        agentSpawner = FindObjectOfType<AgentSpawner>();
        rb = GetComponent<Rigidbody>();

        AIMovementInit();
        tacticalLocations = GameObject.FindGameObjectsWithTag("TacticalLocation");
        applyMovement = new HashSet<AIMovement>();
        isChaser = this.CompareTag("Chaser");
    }
    #endregion

    #region Apply Steering

    private void FixedUpdate() {

        // Add target last position to the list that belongs to singleton class
        if (gameAgent.lastPosCollector.ContainsKey(this)) {
            gameAgent.lastPosCollector[this] = targetLastPos;
        }
        else {
            gameAgent.lastPosCollector.Add(this, targetLastPos);
        }

        // Update the actual speed of the agent
        actualSpeed = ActualSpeed();
        GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation);

        // Update the agent's velocity based on the steeringForce, and clamp it to the maximum speed
        velocity += steeringForceSum * Time.fixedDeltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.fixedDeltaTime;
        transform.rotation *= rotation;

        // Set animation speed based on velocity
        anim.SetFloat("Velocity", velocity.magnitude / maxSpeed);
        anim.SetBool("isSearching", isSearching);
    }

    // Every moveing behaviour was done by this method.
    // Calculate the total steering and rotation.
    private void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation) {
        steeringForceSum = Vector3.zero;
        rotation = Quaternion.identity;
        
        // Apply each movement
        if (isKnockedOut || isSearching || anim.GetCurrentAnimatorStateInfo(0).IsName("throwing") == true) {
            StopMoving();
        }
        else {
            string movementDebugLog = "";
            foreach (AIMovement movement in applyMovement) {
                if (movement != null) {
                    movementDebugLog += movement.GetType().Name + " ";
                    var steeringForce = movement.GetSteering(this);
                    var steeringLinear = new Vector3(steeringForce.linear.x, 0, steeringForce.linear.z);
                    steeringForceSum += steeringForce.linear;
                    rotation *= steeringForce.angular;
                }
            }
            // reset applyMovement
            applyMovement.Clear();
            if (debug) {
                print(movementDebugLog);
            }
        }
    }
    #endregion

    #region Subscribe to Delegate Event
    // Subscribe to the TokenSpawner event to update the status when a token is spawned.
    private void OnEnable() {
        TokenSpawner.OnTokenSpawned += UpdateTokenStatus;
        Token.OnTokenAcquired += UpdateTokenStatus;
    }

    // Unsubscribe to the TokenSpawner event when the script is disabled.
    private void OnDisable() {
        TokenSpawner.OnTokenSpawned -= UpdateTokenStatus;
        Token.OnTokenAcquired -= UpdateTokenStatus;
    }
    #endregion
    #region Delegate Event Type:TokenSpawned
    // called when the TokenSpawner event is triggered to update the token status.
    private void UpdateTokenStatus(bool spawned, GameObject tokenSpawned) {
        if (spawned) {
            if (gameObject.CompareTag("Evader")) {
                this.spawned = spawned;
                aStarTarget = tokenSpawned;
            }
        }
    }
    #endregion

    #region Delegate Event Type:TokenAcquired

    // called when the TokenSpawner event is triggered to update the token status.
    private void UpdateTokenStatus(Vector3 position, GameObject nearlestChaser) {
        if(gameObject == nearlestChaser) {
            tokenAcquiredPos = position;
        }
    }
    #endregion



    private void OnCollisionEnter(Collision collision) {
        // If the AI agent collides with a chaser and it is not a chaser itself, knocked out.
        if (collision.gameObject.CompareTag("Chaser") && CompareTag("Evader")) {
            // set color
            dirIndicator.GetComponent<SpriteRenderer>().color = Color.black;
            dirIndicator2.GetComponent<SpriteRenderer>().color = Color.black;

            //set tag and layer
            gameObject.tag = "KnockedOut";
            gameObject.layer = LayerMask.NameToLayer("KnockedOut");

            // change bool
            isKnockedOut = true;

            anim.SetBool("isKnockedOut", true);
            //collision.gameObject.GetComponent<AIAgent>().targetLastPos = Vector3.positiveInfinity;
            Destroy(gameObject, 5f);
            agentSpawner.seekers.Remove(gameObject);
        }
    }

    // This coroutine waits for a delay and unfreezes the AI agent if it is still frozen.
    public IEnumerator UnfreezeAfterDelay() {
        yield return new WaitForSeconds(5f);

        if (CompareTag("Frozen")) {
            dirIndicator.GetComponent<SpriteRenderer>().color = Color.red;
            dirIndicator2.GetComponent<SpriteRenderer>().color = Color.red;

            gameObject.tag = "Chaser";
            gameObject.layer = LayerMask.NameToLayer("Chaser");

            isKnockedOut = false;

            anim.SetBool("isFrozen", false);
        }
    }

}
