using System;
using UnityEngine;

namespace RealityFlow.NodeGraph.Testing
{
    public class TestCase01 : MonoBehaviour
    {
        public NodeDefinition OnInteract;
        public NodeDefinition Add;
        public NodeDefinition Integer;
        public NodeDefinition Print;

        public Graph ConstructGraph()
        {
            Graph graph = new();
            Node onInteract = new(OnInteract);
            Node start = graph.AddRoot(onInteract);
            Node print = new(Print);
            graph.AddExecutionEdge(start, 0, print);
            Node twelve = new(Integer);
            graph.AddEdge(twelve, 0, print, 0);

            return graph;
        }

        public void Start()
        {
            Graph graph = ConstructGraph();
            graph.EvaluateRoot(0);

            string json = JsonUtility.ToJson(graph, true);
            Debug.Log(json);
        }
    }
}