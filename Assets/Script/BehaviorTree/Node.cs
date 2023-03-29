using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Node;

//define basic node class, including nodestatus,initialize, excute func
public abstract class Node {
    public enum NodeStatus { SUCCESS, FAILURE, RUNNING }

    protected NodeStatus nodeStatus;

    public Node() {
        nodeStatus = NodeStatus.RUNNING;
    }

    public abstract NodeStatus Execute();
}

// childrenList to store multiple child node, and a add child func
public class CompositeNode : Node {
    protected List<Node> children;

    public CompositeNode() {
        children = new List<Node>();
    }

    public void AddChild(Node child) {
        children.Add(child);
    }

    public override NodeStatus Execute() {
        throw new NotImplementedException();
    }
}

public class DecoratorNode : Node {
    protected Node child;

    public void SetChild(Node node) {
        child = node;
    }

    public override NodeStatus Execute() {
        throw new NotImplementedException();
    }
}

public abstract class ActionNode : Node {
    public override abstract NodeStatus Execute();
}

public abstract class ConditionNode : Node {
    public override abstract NodeStatus Execute();
}
// SelectorNode£º
// excute their nodes in order, if one return success then success else failure
public class SelectorNode : CompositeNode {
    public bool debug = false;
    private int currentChildIndex = -1;
    private readonly Dictionary<int, string> sequenceNames = new() {
        { 0, "1st sequence" },
        { 1, "2nd sequence" },
        { 2, "3rd sequence" }
    };


    public override NodeStatus Execute() {
        foreach (Node child in children) {
            currentChildIndex++;
            NodeStatus status = child.Execute();
            if (status != NodeStatus.FAILURE) {
                if (debug) { Debug.Log(sequenceNames[currentChildIndex]); }
                currentChildIndex = -1;
                return status;
            }
        }
        currentChildIndex = -1;
        return NodeStatus.FAILURE;
    }
}
// SequenceNode£º
// excute their nodes in order, if one return failure then failure else success
public class SequenceNode : CompositeNode {
    public override NodeStatus Execute() {
        foreach (Node child in children) {
            NodeStatus status = child.Execute();
            if (status != NodeStatus.SUCCESS) {
                return status;
            }
        }
        return NodeStatus.SUCCESS;
    }
}
// InverterNode£ºonly one child
// if child success, return failure
// if child failure, return success
// if running then running
public class InverterNode : DecoratorNode {
    public override NodeStatus Execute() {
        if (child != null) {
            NodeStatus status = child.Execute();
            if (status == NodeStatus.SUCCESS) {
                return NodeStatus.FAILURE;
            }
            else if (status == NodeStatus.FAILURE) {
                return NodeStatus.SUCCESS;
            }
            else {
                return NodeStatus.RUNNING;
            }
        }
        else {
            return NodeStatus.FAILURE;
        }
    }
}