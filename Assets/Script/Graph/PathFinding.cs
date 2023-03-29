using System.Collections;
using System.Collections.Generic;
using UnityEditor;
//using UnityEditor.Experimental.GraphView;
//using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public delegate float Heuristic(Transform start, Transform end);

    [SerializeField] 
    public GridGraph graph;
    public GridGraphNode startNode;
    public GridGraphNode goalNode;
   
    public GameObject openPointPrefab;
    public GameObject closedPointPrefab;
    public GameObject pathPointPrefab;

    public float ChebyshevDistanceHeuristic(Transform start, Transform end) {
        float dx = Mathf.Abs(start.position.x - end.position.x);
        float dy = Mathf.Abs(start.position.y - end.position.y);

        return Mathf.Max(dx, dy);
    }
    private float MovementCost(GridGraphNode next, GridGraphNode current, GridGraph graph) {
        Vector3 movementOffset= (next.transform.position - current.transform.position) / graph.genGridCellSize;
        int[,] diagonal = new int[,] {{ -1, 1 }, { 1, 1 },{ -1,-1 }, { 1,-1 }};
        for (int i = 0; i < diagonal.GetLength(0); ++i) {
            if (movementOffset.x == diagonal[i,0] && movementOffset.z == diagonal[i, 1]) {
                return 1.4f;
            }
        }
        return 1f;
    }


    public List<GridGraphNode> FindPath(GridGraphNode start, GridGraphNode goal, bool isDebug, bool isChaser, Heuristic heuristic = null, bool isAdmissible = true) {
        if(graph == null) { return new List<GridGraphNode>(); }
        //if no heuristic, heuristic = 0;
        if (heuristic == null) { heuristic = (Transform s, Transform e) => 0; }

        List<GridGraphNode> path = null;
        bool solutionFound = false;

        List<GridGraphNode> openList = new List<GridGraphNode>() { start };
        HashSet<GridGraphNode> closedSet = new HashSet<GridGraphNode>();
        //dictionary to keep track of our path (came_from)
        Dictionary<GridGraphNode, GridGraphNode> pathDict = new Dictionary<GridGraphNode, GridGraphNode>();
        pathDict.Add(start, null);

        //dictionary to keep track of g(n) values (movement costs)
        Dictionary<GridGraphNode, float> gnDict = new Dictionary<GridGraphNode, float>();
        gnDict.Add(start, default);

        //dictionary to keep track of f(n) values (movement cost + heuristic)
        Dictionary<GridGraphNode, float> fnDict = new Dictionary<GridGraphNode, float>();
        fnDict.Add(start, heuristic(start.transform, goal.transform) + gnDict[start]);


        while (openList.Count > 0) {
            //current = open_list.Pop()
            GridGraphNode current = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);

            closedSet.Add(current);

            //if it is already goal
            if (current == goal && isAdmissible) {
                solutionFound = true;
                break;
            }
            //if goal is on the closedSet, we check if it is shortest path, by iterate through the openList,
            //if we found a entry that has smaller g value than gGoal, then it means that there is a shorter path, we have to search
            else if (closedSet.Contains(goal)) {
                float gGoal = gnDict[goal];
                bool pathIsTheShortest = true;

                foreach (GridGraphNode entry in openList) {
                    if(gGoal > gnDict[entry]) {
                        pathIsTheShortest = false;
                        break;
                    }
                }
                if (pathIsTheShortest) break;
            }
            // foreach next_node in graph.GetNeighbors(current_node):
            List<GridGraphNode> neighbors = graph.GetNeighbors(current);
            foreach (GridGraphNode next in neighbors) {
                float gNeighbor = gnDict[current] + MovementCost(next, current, graph); ;

                if (!gnDict.ContainsKey(next) || gNeighbor < gnDict[next]) {
                    gnDict[next] = gNeighbor;
                    if (isChaser) {
                        fnDict[next] = (gNeighbor + heuristic(next.transform, goal.transform)) * (1 + InfluenceMap.influenceInTotalMap[next]);
                        
                    }
                    else {
                        // if is seeker, we add the chaser influence map
                        fnDict[next] = (gNeighbor + heuristic(goal.transform, next.transform)) * (1 + InfluenceMap.chaserInfluenceMap[next]);
                        //fnDict[next] = (gNeighbor + heuristic(next.transform, goal.transform))* (1+ InfluenceMap.chaserInfluenceMap[next]);
                       // print("GameObject"+ next.gameObject+ " before:" + (gNeighbor + heuristic(next.transform, goal.transform)) + "after:" + fnDict[next]);
                    }
                    
                    FakePQListInsert(openList,fnDict, next);
                    pathDict[next] = current;
                }

                // check if you need to update tables, calculate fn, and update open_list using FakePQListInsert() function
                // and do so if necessary
                // ...
            }
        }
        // if the closed list contains the goal node then we have found a solution
        if (!solutionFound && closedSet.Contains(goal))
            solutionFound = true;

        if (solutionFound) {
            // create the path by traversing the previous nodes in the pathDict
            // starting at the goal and finishing at the start
            path = new List<GridGraphNode>();
            GridGraphNode current = goal;
            while (current != start) {
                path.Insert(0, current);
                current = pathDict[current];
            }
            path.Insert(0, start);
            // ...

            // reverse the path since we started adding nodes from the goal 
            //path.Reverse();
        }

        if (isDebug) {
            ClearPoints();

            List<Transform> openListPoints = new List<Transform>();
            foreach (GridGraphNode node in openList) {
                openListPoints.Add(node.transform);
            }
            SpawnPoints(openListPoints, openPointPrefab, Color.magenta);

            List<Transform> closedListPoints = new List<Transform>();
            foreach (GridGraphNode node in closedSet) {
                if (solutionFound && !path.Contains(node))
                    closedListPoints.Add(node.transform);
            }
            SpawnPoints(closedListPoints, closedPointPrefab, Color.red);

            if (solutionFound) {
                List<Transform> pathPoints = new List<Transform>();
                foreach (GridGraphNode node in path) {
                    pathPoints.Add(node.transform);
                }
                SpawnPoints(pathPoints, pathPointPrefab, Color.green);
            }
        }

        return path;
    }
    private void SpawnPoints(List<Transform> points, GameObject prefab, Color color) {
        for (int i = 0; i < points.Count; ++i) {
#if UNITY_EDITOR
            // Scene view visuals
            points[i].GetComponent<GridGraphNode>()._nodeGizmoColor = color;
#endif

            // Game view visuals
            GameObject obj = Instantiate(prefab, points[i].position, Quaternion.identity, points[i]);
            obj.name = "DEBUG_POINT";
            obj.transform.localPosition += Vector3.up * 0.5f;
        }
    }
    private void ClearPoints() {
        foreach (GridGraphNode node in graph.nodes) {
            node._nodeGizmoColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f);
            for (int c = 0; c < node.transform.childCount; ++c) {
                if (node.transform.GetChild(c).name == "DEBUG_POINT") {
                    Destroy(node.transform.GetChild(c).gameObject);
                }
            }
        }
    }
    /// <summary>
    /// mimics a priority queue here by inserting at the right position using a loop
    /// not a very good solution but ok for this lab example
    /// </summary>
    /// <param name="pqList"></param>
    /// <param name="fnDict"></param>
    /// <param name="node"></param>
    private void FakePQListInsert(List<GridGraphNode> pqList, Dictionary<GridGraphNode, float> fnDict, GridGraphNode node) {
        if (pqList.Count == 0)
            pqList.Add(node);
        else {
            for (int i = pqList.Count - 1; i >= 0; --i) {
                if (fnDict[pqList[i]] > fnDict[node]) {
                    pqList.Insert(i + 1, node);
                    break;
                }
                else if (i == 0)
                    pqList.Insert(0, node);
            }
        }
    }
}
