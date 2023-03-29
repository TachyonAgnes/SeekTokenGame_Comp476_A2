using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserBT : MonoBehaviour
{
    private SelectorNode root;
    private AIAgent agent;

    // gridGraph
    public List<GridGraphNode> nodes;
    private int nodeCount;


    // Patrol points
    private List<GameObject> patrolPoints;

    private void Start() {
        GridGraph gridGraph = FindObjectOfType<GridGraph>();
        nodes = gridGraph.nodes;
        nodeCount = nodes.Count;

        agent = GetComponent<AIAgent>();

        // Generate patrol points
        patrolPoints = new List<GameObject>();
        for (int i = 0; i < 5; i++) {
            patrolPoints.Add(nodes[UnityEngine.Random.Range(0, nodeCount)].gameObject);
        }

        // Build behavior tree
        root = new SelectorNode();
        // 1. InSightThenTrace
        SequenceNode trackSightedSeq = new SequenceNode();
        trackSightedSeq.AddChild(new IsTargetInSightNode(agent));
        trackSightedSeq.AddChild(new TraceTargetNode(agent));
        // 2. EnemyDetectedSearch
        SequenceNode enemyDetectedSearchSeq = new SequenceNode();
        enemyDetectedSearchSeq.AddChild(new IsTargetRememberedNode(agent));
        enemyDetectedSearchSeq.AddChild(new MovToLasKnownPosNode(agent));
        enemyDetectedSearchSeq.AddChild(new WaitNRotateNode(agent, 5f));
        // 3. patrol
        SequenceNode patrolSeq = new SequenceNode();
        patrolSeq.AddChild(new PatrolNode(agent, patrolPoints));

        root.AddChild(trackSightedSeq);
        root.AddChild(enemyDetectedSearchSeq);
        root.AddChild(patrolSeq);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (agent.gameObject.CompareTag("Chaser")) {
            root.debug = agent.debug;
            root.Execute();
        }
    }
}
