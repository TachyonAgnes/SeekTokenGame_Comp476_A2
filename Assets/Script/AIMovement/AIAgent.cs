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
using static UnityEngine.EventSystems.EventTrigger;
using Debug = UnityEngine.Debug;

public class AIAgent : MonoBehaviour {
    // Movement Variables
    [Header("Movement Variables")]
    public float maxForce = 10f;
    public float maxSpeed = 5f;
    public float actualSpeed;
    public float slowSpeedThreshold = 1.8f;
    public Vector3 velocity;
    public bool isFacingTarget;
    public bool isFacingAwayFromTarget;
    public bool isBeingChased = false;
    public bool isFrozen = false;
    public bool isUnfreezed = false;

    // Radius Variables
    [Header("Safe Radius")]
    public float nearRadius = 5f;
    public float closeRadius = 15f;
    public float farRadius = 25f;

    // Cone Variables
    [Header("Field Of View")]
    public float maxAngle = 150f;
    public float minAngle = 100f;

    // Room Variables
    private GameObject visitedAxis = null;
    private Vector3 bestAxisPosition;
    private float visitedAxisResetTime = 0f;
    public float visitedAxisResetDelay = 20f;
    public float visitedAxisDistanceThreshold = 5.1f;

    //Others
    public GameObject target;
    public GameObject aStarTarget;
    public GameObject marker;
    public bool debug = false;
    public  bool lockY = true;
    public float dotProduct;
    public SpriteRenderer dirIndicator;
    private AIMovement flock, flee, seek, pursue, obstacleAvoidance, lookWryGo, wander, aStarPathFinding, turnTowardTarget, turnAwayFromTarget, flocking;
    private Coroutine unfreezeCoroutine;

    // initialized in awake
    public LayerMask obstacleMask;
    public PathFinding pathFinding;
    public GridGraph gridGraph;
    private Animator anim;
    private Dictionary<Type, AIMovement> movementsDic = new Dictionary<Type, AIMovement>();
    private Vector3 previousPosition;
    private GameObject[] evaders;
    private GameAgent gameAgent;

    #region Initialize

    // this method initialize the AIMovement by getting all the almovement components attached to the AI agent,
    // and storing them in a dictionary for a quick access;
    private void AIMovementInit() {
        AIMovement[] components = GetComponents<AIMovement>();
        foreach (AIMovement component in components) {
            movementsDic[component.GetType()] = component;
        }
        (flock, flee, seek, pursue, obstacleAvoidance, lookWryGo, wander, aStarPathFinding, turnTowardTarget, turnAwayFromTarget, flocking) =
            (movementsDic.GetValueOrDefault(typeof(Flock)) as Flock,
             movementsDic.GetValueOrDefault(typeof(Flee)) as Flee,
             movementsDic.GetValueOrDefault(typeof(Seek)) as Seek,
             movementsDic.GetValueOrDefault(typeof(Pursue)) as Pursue,
             movementsDic.GetValueOrDefault(typeof(ObstacleAvoidance)) as ObstacleAvoidance,
             movementsDic.GetValueOrDefault(typeof(LookWhereYouAreGoing)) as LookWhereYouAreGoing,
             movementsDic.GetValueOrDefault(typeof(Wander)) as Wander,
             movementsDic.GetValueOrDefault(typeof(AstarMovement)) as AstarMovement,
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
        if (gameObject.tag != null && gameObject.tag == "Chaser") { TargetReset(); }
        gameAgent = FindObjectOfType<GameAgent>();
        dirIndicator = transform.Find("Head").GetComponent<SpriteRenderer>();
        marker = transform.GetChild(4).gameObject;

        AIMovementInit();
        TargetReset(CompareTag("Chaser") ? "Evader": "Chaser");
        AstarTargetReset();
    }
    #endregion

    #region Apply Steering
    private void FixedUpdate() {
        actualSpeed = ActualSpeed();

        
        GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation);

        // Update the agent's velocity based on the steeringForce, and clamp it to the maximum speed
        velocity += steeringForceSum * Time.fixedDeltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.fixedDeltaTime;
        transform.rotation *= rotation;

        // Set animation speed based on velocity
        anim.SetFloat("Velocity", velocity.magnitude / maxSpeed);
    }

    // Every moveing behaviour was done by this method.
    // Calculate the total steering and rotation.
    private void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation) {
        steeringForceSum = Vector3.zero;
        rotation = Quaternion.identity;
        List<AIMovement> applyMovement = new List<AIMovement>();

        // Determine which movements to apply based on the agent's current state and tag
        if (tag != null && CompareTag("Chaser")) {
            if (target == null) { TargetReset(); }
            evaders = GameObject.FindGameObjectsWithTag("Evader");
            applyMovement = MoveTowardsTarget();
        }
        else if (gameAgent.isSomeOneFrozen) {
            applyMovement = MoveTowardsTarget();
        }
        else {
            TargetReset("Chaser");
            applyMovement = MoveAwayFromTarget();
        }

        // Apply each movement
        if (isFrozen) {
            StopMoving();
        }
        else {
            string movementDebugLog = "";
            foreach (AIMovement movement in applyMovement) {
                if (movement != null) {
                    movementDebugLog += movement.GetType().Name + " ";
                    steeringForceSum += movement.GetSteering(this).linear;
                    rotation *= movement.GetSteering(this).angular;
                }
            }
            if (debug) {
                print(movementDebugLog);
            }
        }
    }
    #endregion


    //recall:
    // R2) Human Character Behavior:
    // 1. If the character is stationary or moving very slowly, then
    //      1.1. If it is a very small distance from its target, 
    //          it will step there directly, even if this involves moving backward or sidestepping,
    //      1.2. Else if the target is farther away, 
    //          1.2.1 the character will first turn on the spot to face its target
    //          1.2.2 and then move forward to reach it.
    // 2. Else if: the character is moving with some speed, then,
    //      2.1. If: the target is within a speed - dependent arc 
    //          it will continue to move forward but add a rotational component to incrementally turn toward the target,
    //      2.2. Else if: the target is outside its arc, 
    //          2.2.1. then it will stop moving and change direction on the spot
    //          2.2.2. setting off once more

    // return List<AIMovement> to be applied in GetSteeringSum()
    // when moving towards the target
    private List<AIMovement> MoveTowardsTarget() {
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        List<AIMovement> applyMovement = new List<AIMovement>();

        // Lambda functions for readability
        Func<bool> isObstacleBetween = () => CheckObstacleBetween();
        Func<bool> isTurnOnSpot = () => !isFacingTarget && !isObstacleBetween();
        Func<bool> shouldUseAStar = () => distanceToTarget > closeRadius || isObstacleBetween();
        bool isAstar;
        AstarTargetReset();  // Reset A* pathfinding target

        if (isUnfreezed) { applyMovement.Add(flocking); }
        if (actualSpeed <= slowSpeedThreshold) { // 1. If the agent is moving slowly
            if (distanceToTarget < nearRadius && !target.CompareTag("Token")) { // 1.1. If the target is within the nearRadius (and not a token)
                applyMovement.Add(seek);
            }
            else if (isTurnOnSpot()) { // 1.2.1  If the agent is not facing the target (and there is no obstacle between), stop and turn towards the target
                StopMoving();
                applyMovement.Add(turnTowardTarget);
            }
            else { // 1.2.2 do the move
                isAstar = shouldUseAStar();
                applyMovement.Add(isAstar ? aStarPathFinding : pursue);
                applyMovement.Add(turnTowardTarget);
            }
        }
        else { // 2. If the agent is moving fast
            if (CheckInCone()) { // 2.1 If the target is within the cone of view
                applyMovement.Add(pursue);
            }
            else if (isTurnOnSpot()) { // 2.2.1 If the agent is not facing the target (and there is no obstacle between), stop and turn towards the target
                StopMoving();
            }
            else { // 2.2.2 setting off once more
                isAstar = shouldUseAStar();
                applyMovement.Add(isAstar ? aStarPathFinding : pursue);
            }
            applyMovement.Add(turnTowardTarget); // all of the case listed about need turn towards the target
        }
        applyMovement.Add(obstacleAvoidance);
        return applyMovement;
    }

    // return List<AIMovement> to be applied in GetSteeringSum()
    // when moving away from the target
    private List<AIMovement> MoveAwayFromTarget() {
    float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
    List<AIMovement> applyMovement = new List<AIMovement>();
    AstarTargetReset(); // Reset A* pathfinding target

    if (distanceToTarget > farRadius) { // added: if target is out of farRadius
            applyMovement.Add(wander);
    }
    else { //if target is in the farRadius
        AIAgent targetAgent = target.GetComponent<AIAgent>();
            if (targetAgent.target == null || targetAgent.target != gameObject) {
                applyMovement.Add(flee);
                isBeingChased = false;
            }
            else {
                applyMovement.Add(aStarPathFinding);
                isBeingChased = true;
            }
    }
        if (isUnfreezed) {
            applyMovement.Add(flocking); // flocking
        }
        applyMovement.Add(lookWryGo); // better move performance tbh
        applyMovement.Add(obstacleAvoidance);
        return applyMovement;
}

    // chaser exclusive: pick the best next target
    public void TargetReset(String tagName = "Evader") {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tagName); // Find all game objects with the specified tag
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject tgt in targets) {
            AIAgent aiAgent = tgt.GetComponent<AIAgent>();
            if (aiAgent == null || aiAgent.isBeingChased) { // Check if target has AIAgent component and is not already being chased
                if (tgt.CompareTag("Frozen")) { 
                    closestTarget = tgt;
                    break;
                }
            }
            float distance = Vector3.Distance(transform.position, tgt.transform.position);

            // Set current target as closest if distance is smaller than closestDistance
            // find the closest one in other word
            if (distance < closestDistance) {
                closestDistance = distance;
                closestTarget = tgt;
            }
        }
        target = closestTarget;
    }

    private void AstarTargetReset() {
        if (gameObject.tag != null && gameObject.tag == "Chaser") { // If agent is chaser, set target for A* pathfinding to be the current target
            aStarTarget = target;
        }
        else if(gameAgent.isSomeOneFrozen) { 
            TargetReset("Frozen"); // If there is at least one frozen target, set target for A* pathfinding to be the closest frozen target
            if (target != null) { 
                aStarTarget = target;
            }
            else {
                gameAgent.isSomeOneFrozen = false;
            }
        }
        else {
            // Otherwise, set target for A* pathfinding to be the best room axis
            aStarTarget = FindBestRoomAxis();
        }
    }

    #region MoveAway: Find Best Axis To Leave

    // This method finds the best room axis for the chaser to move towards
    public GameObject FindBestRoomAxis() {
        GameObject[] roomAxes = GameObject.FindGameObjectsWithTag("RoomAxis");
        GameObject bestAxis = null;
        float minAngle = float.MaxValue; 
        if (visitedAxis != null && Time.time >= visitedAxisResetTime) {
            ResetVisitedAxis(); // Reset visited axis if it has been too long since last visit
        }

        // Loop through all room axes
        foreach (GameObject axis in roomAxes) {
            if (axis == visitedAxis) continue; // If the axis has been visited recently, skip it
            float distance = Vector3.Distance(axis.transform.position, transform.position);
            if (distance > 45f) continue; // If the distance is too far, skip this axis

            Vector3 directionToAxis = axis.transform.position - transform.position;
            Vector3 enemyToAgent = transform.position - target.transform.position;
            
            float angle = Vector3.Angle(directionToAxis, enemyToAgent);

            // This means that we choose not to move towards the enemys as much as possible
            if (angle < minAngle) { // find min angle, 
                minAngle = angle;
                bestAxis = axis;
                bestAxisPosition = axis.transform.position;
            }
        }
        if (bestAxis != null) {
            Invoke(nameof(CheckVisitedAxis), 0.1f);
        }
        return bestAxis;
    }
    // This method checks if the agent has visited the axis, in other word we don't want to stick at one axis
    private void CheckVisitedAxis() {
        float distance = Vector3.Distance(transform.position, bestAxisPosition);
        if (distance <= visitedAxisDistanceThreshold) {
            visitedAxis = new GameObject();
            visitedAxis.transform.position = bestAxisPosition;
            visitedAxisResetTime = Time.fixedTime + visitedAxisResetDelay;
        }
        else {
            Invoke(nameof(CheckVisitedAxis), 0.1f);
        }
    }

    private void ResetVisitedAxis() {
        visitedAxis = null;
    }
    #endregion

    #region Check The Environment

    // Check if the target is within the cone of vision
    private bool CheckInCone() {
        float radius = closeRadius;
        float speed = velocity.magnitude;

        // calculate cone angle based on the agent's speed
        float coneAngle = Mathf.Lerp(maxAngle, minAngle, speed / maxSpeed);
        coneAngle = Mathf.Clamp(coneAngle, minAngle, maxAngle);

        Vector3 directionToTarget = target.transform.position - transform.position;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        // check if ray is lying between the angle in other words
        if (CheckObstacleBetween()) {
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-coneAngle / 2, gameObject.transform.up) * transform.forward * closeRadius, Color.blue);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(coneAngle / 2, gameObject.transform.up) * transform.forward * closeRadius, Color.blue);
            return false;
        }
        else {
            // Check if the angle to the target is within the cone angle
            if (angleToTarget <= coneAngle / 2) {
                return true;
            }
            else {
                return false;
            }
        }
    }

    // Check for obstacles between the agent and the target
    private bool CheckObstacleBetween() {
        Vector3 start = transform.position;
        Vector3 end = target.transform.position - start;
        int layer = obstacleMask;
        LayerMask layerMask = 1 << layer;
        // Check if there is a collision with any object on the obstacle mask layer 
        return Physics.Raycast(start, end, out RaycastHit hit, layerMask) && hit.collider.gameObject != target;
    }
    #endregion

    #region Delegate Event Type:TokenSpawned
    // Subscribe to the TokenSpawner event to update the status when a token is spawned.
    private void OnEnable() {
        TokenSpawner.OnTokenSpawned += UpdateTokenStatus;
    }

    // Unsubscribe to the TokenSpawner event when the script is disabled.
    private void OnDisable() {
        TokenSpawner.OnTokenSpawned -= UpdateTokenStatus;
    }

    // called when the TokenSpawner event is triggered to update the token status.
    private void UpdateTokenStatus(bool spawned, GameObject tokenSpawned, GameObject closestChaser) {
        // If this AI agent is the closest chaser to the token, update its target and set the marker active. (we want this chaser looks different)
        if (spawned) {
            if (gameObject == closestChaser) {
                if (target != null && target.CompareTag("Evader") && target.name != "player") {
                    target.GetComponent<AIAgent>().isBeingChased = false;
                }
                target = tokenSpawned;
                marker.SetActive(true);
            }
        }
    }
    #endregion


    private void OnCollisionEnter(Collision collision) {
        // If the AI agent collides with a chaser and it is not a chaser itself, freeze it.
        if (collision.gameObject.CompareTag("Chaser") && !CompareTag("Chaser")) {
            dirIndicator.color = Color.gray;
            isFrozen = true;
            gameObject.tag = "Frozen";
            gameObject.layer = LayerMask.NameToLayer("Frozen");
            gameAgent.isSomeOneFrozen = true;
            collision.gameObject.GetComponent<AIAgent>().TargetReset();
            unfreezeCoroutine = StartCoroutine(UnfreezeAfterDelay());
        }
        // If the AI agent is frozen and collides with an evader, unfreeze it.
        if (CompareTag("Frozen") && collision.gameObject.tag == "Evader") {
            if (collision.gameObject.name != "player") { 
                isUnfreezed = true;
            }
            dirIndicator.color = Color.green;
            isFrozen = false;
            gameObject.tag = "Evader";
            gameObject.layer = LayerMask.NameToLayer("Evader");
            target = collision.gameObject;
            gameAgent.isSomeOneFrozen = false;
            StopCoroutine(unfreezeCoroutine);
            unfreezeCoroutine = null;
        }
    }

    // This coroutine waits for a delay and unfreezes the AI agent if it is still frozen.
    private IEnumerator UnfreezeAfterDelay() {
        yield return new WaitForSeconds(15f);
        
        if (isFrozen) {
            dirIndicator.color = Color.red;
            gameObject.tag = "Chaser";
            gameObject.layer = LayerMask.NameToLayer("Chaser");
            gameAgent.isSomeOneFrozen = false;
            isFrozen = false;
            TargetReset();
        }
    }





}
