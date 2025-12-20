using System;
using System.Collections.Generic;
using System.Linq;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
    /// <summary>
    /// Yönlü graf yapısını temsil eder.
    /// Düğümler: GraphNode (Paper sarmalayıcı)
    /// Kenarlar: GraphEdge
    /// </summary>
    public class Graph
    {
        private readonly Dictionary<string, GraphNode> _nodes;
        private readonly List<GraphEdge> _edges;

        public Graph()
        {
            _nodes = new Dictionary<string, GraphNode>();
            _edges = new List<GraphEdge>();
        }

        // =========================
        // NODE İŞLEMLERİ
        // =========================

        public void AddNode(GraphNode node)
        {
            if (!_nodes.ContainsKey(node.Id))
            {
                _nodes.Add(node.Id, node);
            }
        }

        public bool ContainsNode(string id)
        {
            return _nodes.ContainsKey(id);
        }

        public GraphNode? GetNode(string id)
        {
            _nodes.TryGetValue(id, out var node);
            return node;
        }

        public IReadOnlyCollection<GraphNode> GetAllNodes()
        {
            return _nodes.Values;
        }

        // =========================
        // EDGE İŞLEMLERİ
        // =========================

        public void AddEdge(
            string sourceId,
            string targetId,
            EdgeType type = EdgeType.Citation,
            bool isDirected = true)
        {
            var source = GetNode(sourceId);
            var target = GetNode(targetId);

            if (source == null || target == null)
                return;

            bool exists = _edges.Any(e =>
                e.Source.Id == sourceId &&
                e.Target.Id == targetId &&
                e.Type == type);

            if (!exists)
            {
                _edges.Add(new GraphEdge(source, target, type, isDirected));
            }
        }

        public IReadOnlyCollection<GraphEdge> GetAllEdges()
        {
            return _edges;
        }

        // =========================
        // DERECE HESAPLARI
        // =========================

        public int GetOutDegree(string nodeId)
        {
            return _edges.Count(e => e.Source.Id == nodeId);
        }

        public int GetInDegree(string nodeId)
        {
            return _edges.Count(e => e.Target.Id == nodeId);
        }

        public IEnumerable<GraphNode> GetOutgoingNeighbors(string nodeId)
        {
            return _edges
                .Where(e => e.Source.Id == nodeId)
                .Select(e => e.Target);
        }

        public IEnumerable<GraphNode> GetIncomingNeighbors(string nodeId)
        {
            return _edges
                .Where(e => e.Target.Id == nodeId)
                .Select(e => e.Source);
        }

        // =========================
        // İSTATİSTİK DESTEK
        // =========================

        public int TotalNodeCount => _nodes.Count;
        public int TotalEdgeCount => _edges.Count;

        public GraphNode? GetMostCitedNode()
        {
            return _nodes.Values
                .OrderByDescending(n => GetInDegree(n.Id))
                .FirstOrDefault();
        }

        public GraphNode? GetMostReferencingNode()
        {
            return _nodes.Values
                .OrderByDescending(n => GetOutDegree(n.Id))
                .FirstOrDefault();
        }
    }
}
