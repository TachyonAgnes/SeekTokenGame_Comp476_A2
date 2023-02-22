using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

//by attribute "ExecuteInEditMode", this code will run in edit mode as well as play mode. 
[ExecuteInEditMode]
public class GridGraphNode : MonoBehaviour
{
    //this Serialize Field is for debug purpose only
    [SerializeField] public List<GridGraphNode> adjacencyList = new List<GridGraphNode>();

    public Color _nodeGizmoColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f);

    private GridGraph graph;
    private GridGraph Graph {
        get {
            if(graph == null) {
                graph = GetComponent<GridGraph>();
            }
            return graph;
        }
    }

    private void OnDestroy()
    {
        if (Graph != null) {
            Graph.Remove(this);
        }
    }
}
