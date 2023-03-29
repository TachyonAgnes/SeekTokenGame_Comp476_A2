using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekerBT : MonoBehaviour
{
    private SelectorNode root;
    private AIAgent agent;


    private void Start() {
        agent = GetComponent<AIAgent>();
        // Build behavior tree
        root = new SelectorNode();
        SequenceNode enimeInAreaSeq = new SequenceNode();
        enimeInAreaSeq.AddChild(new IsTargetInAreaNode(agent));
        enimeInAreaSeq.AddChild(new SeekerMovementNode(agent));
        enimeInAreaSeq.AddChild(new IsBlizzardNeededNode(agent));
        enimeInAreaSeq.AddChild(new EscapeNode(agent));
        SequenceNode seekTokenSeq = new SequenceNode();
        seekTokenSeq.AddChild(new IsTokenExistNode(agent));
        seekTokenSeq.AddChild(new SeekTokenNode(agent));
        SequenceNode wanderSeq = new SequenceNode();
        wanderSeq.AddChild(new WanderNode(agent));

        root.AddChild(enimeInAreaSeq);
        root.AddChild(seekTokenSeq);
        root.AddChild(wanderSeq);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (agent.gameObject.CompareTag("Evader")) {
            root.debug = agent.debug;
            root.Execute();
        }
    }
}
