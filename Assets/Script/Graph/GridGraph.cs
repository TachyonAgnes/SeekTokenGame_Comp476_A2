using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridGraph : MonoBehaviour {
    [SerializeField, HideInInspector] public List<GridGraphNode> nodes = new List<GridGraphNode>();
    [SerializeField] public GameObject nodePrefab;
    [SerializeField] public List<List<GridGraphNode>> nodeGrid = new List<List<GridGraphNode>>();
    public int Count => nodes.Count;

#region grid_generation_properties
    [HideInInspector, Min(0)] public int genGridCols = 1;
    [HideInInspector, Min(0)] public int genGridRows = 1;
    [HideInInspector, Min(0)] public float genGridCellSize = 1;

#if UNITY_EDITOR
    [Header("Gizmos")]
    /// <summary>WARNING: This property is used by Gizmos only and is removed from the build. DO NOT reference it outside of Editor-Only code.</summary>
    public float _nodeGizmoRadius = 0.5f;
    /// <summary>WARNING: This property is used by Gizmos only and is removed from the build. DO NOT reference it outside of Editor-Only code.</summary>
    public Color _edgeGizmoColor = Color.white;

    private void OnDrawGizmos() {
        if (nodes == null) return;

        // nodes
        foreach (GridGraphNode node in nodes) {
            if (node == null) continue;

            //Draw node with node color
            Gizmos.color = node._nodeGizmoColor;
            Gizmos.DrawSphere(node.transform.position, _nodeGizmoRadius);

            //Draw line with edge color
            Gizmos.color = _edgeGizmoColor;
            //grab adjacencyList from node, and draw lines
            List<GridGraphNode> neighbors = GetNeighbors(node);
            foreach (GridGraphNode neighbor in neighbors) {
                Gizmos.DrawLine(node.transform.position, neighbor.transform.position);
            }
        }
    }
#endif
    #endregion

    private void Awake() {
        for (int r = 0; r < genGridRows; r++) {
            nodeGrid.Add(new List<GridGraphNode>());
            for (int c = 0; c < genGridCols; c++) {
                nodeGrid[r].Add(null);
            }
        }
        GenerateGrid();
    }

    //Remove nodes ref and destroy child instance;
    public void Clear() {
        nodes.Clear();
        gameObject.DestroyChildren();
    }

    // Remove one node from graph
    // To remove a node, remove this node from neighborNode's adjacency List, then remove the node it self;
    public void Remove(GridGraphNode node) {
        if (node == null || !nodes.Contains(node)) return;

        foreach (GridGraphNode neighborNode in node.adjacencyList) {
            neighborNode.adjacencyList.Remove(node);
        }
        nodes.Remove(node);
    }
    public GridGraphNode FindObjectAdjacentNode(Vector3 pos) {
        GridGraphNode closestNode = null;

        float closestDistance = Mathf.Infinity;

        foreach (GridGraphNode node in nodes) {

            float currentDistance = Vector3.Distance(pos, node.transform.position);

            if (currentDistance < closestDistance) {

                closestDistance = currentDistance;

                closestNode = node;

            }
        }
        return closestNode;
    }

    public void GenerateGrid(bool checkCollisions = true) {
        Clear();

        // if genGridCols or genGridRows <= 0, their corresponding width or height value will be zero, other wise, £¨genGrid* - 1)  * genGridCellSize
        // ex. say if we have 10 cols, then there are 9 space between them
        float width = (genGridCols > 0 ? genGridCols - 1 : 0) * genGridCellSize;
        float height = (genGridRows > 0 ? genGridRows - 1 : 0) * genGridCellSize;


        Vector3 genPosition = new Vector3(transform.position.x - (width / 2), transform.position.y, transform.position.z - (height / 2));

        //first step: generate nodes
        for (int r = 0; r < genGridRows; ++r) {
            float startingX = genPosition.x;
            for (int c = 0; c < genGridCols; ++c) {
                //Check Collisions, if Collides, genPosition change to next position, then skip to next one.
                if (checkCollisions &&
                   Physics.CheckBox(genPosition, Vector3.one / 2, Quaternion.identity, LayerMask.GetMask("Obstacle"))) {
                    genPosition = new Vector3(genPosition.x + genGridCellSize, genPosition.y, genPosition.z);
                    continue;
                }

                //start generate game object
                GameObject obj;
                if (nodePrefab == null) {
                    obj = new GameObject("Node", typeof(GridGraphNode));
                }
                else { obj = Instantiate(nodePrefab); }

                obj.name = $"Node ({nodes.Count})";
                obj.tag = "Node";
                obj.transform.parent = transform;
                obj.transform.position = genPosition;
                //end of generate Object


                GridGraphNode addedNode = obj.GetComponent<GridGraphNode>();
                nodes.Add(addedNode);
                nodeGrid[r][c] = addedNode;

                //next col
                genPosition = new Vector3(genPosition.x + genGridCellSize, genPosition.y, genPosition.z);
            }
            //next row
            genPosition = new Vector3(startingX, genPosition.y, genPosition.z + genGridCellSize);
        }

        //second step:  create adjacency lists(edges)
        //the relative neighbor position offset;
        int[,] operations = new int[,] {{ -1, 1 }, { 0, 1 }, { 1, 1 },
                                        { -1, 0 },           { 1, 0 },
                                        { -1,-1 }, { 0,-1 }, { 1,-1 }};
        for (int r = 0; r < genGridRows; ++r) {
            for (int c = 0; c < genGridCols; ++c) {
                //the nodeGrid is two dimention array that is storing generated node instance;
                if (nodeGrid[r][c] == null) continue;

                //each loop, check 8 neighbor around that node, and see if operation bring us out of bounds, if so, check next one
                //GetLength(0), find 1st dimension length.
                for (int i = 0; i < operations.GetLength(0); ++i) {
                    int[] neighborId = new int[2] { r + operations[i, 0], c + operations[i, 1] };

                    // check to see if operation brings us out of bounds,if so continues
                    if (neighborId[0] < 0 || neighborId[0] >= nodeGrid.Count || neighborId[1] < 0 || neighborId[1] >= nodeGrid[0].Count) {
                        continue;
                    }

                    // a neighbor position that is valid, and it has a created instance
                    GridGraphNode neighbor = nodeGrid[neighborId[0]][neighborId[1]];

                    // use raycast to check collisions, if so continues, else store it.
                    if (neighbor != null) {
                        if(checkCollisions) {
                            Vector3 direction = neighbor.transform.position - nodeGrid[r][c].transform.position;
                            if(Physics.Raycast(nodeGrid[r][c].transform.position, direction, direction.magnitude, LayerMask.GetMask("Obstacle"))) {
                                continue;
                            }
                        }
                        nodeGrid[r][c].adjacencyList.Add(neighbor);
                    }
                }
            }
        }
    }
   public List<GridGraphNode> GetNeighbors(GridGraphNode node) {
        return node.adjacencyList;
    }
}
