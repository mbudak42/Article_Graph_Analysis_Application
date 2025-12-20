using System;
using System.Collections.Generic;
using System.Linq;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core.KCore
{
    public class KCoreDecomposition
    {
        private readonly Graph _graph;

        public KCoreDecomposition(Graph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        public KCoreResult Decompose(int k)
        {
            if (k < 0)
                throw new ArgumentException("k must be non-negative", nameof(k));

            var workingNodes = new HashSet<string>(_graph.GetAllNodes().Select(n => n.Id));
            var degrees = CalculateUndirectedDegrees(workingNodes);

            bool changed = true;
            while (changed)
            {
                changed = false;

                var nodesToRemove = new List<string>();
                foreach (var nodeId in workingNodes)
                {
                    if (degrees[nodeId] < k)
                    {
                        nodesToRemove.Add(nodeId);
                        changed = true;
                    }
                }

                foreach (var nodeId in nodesToRemove)
                {
                    workingNodes.Remove(nodeId);

                    foreach (var neighbor in GetUndirectedNeighbors(nodeId))
                    {
                        if (workingNodes.Contains(neighbor))
                        {
                            degrees[neighbor]--;
                        }
                    }
                }
            }

            var kCoreEdges = GetEdgesBetweenNodes(workingNodes);

            return new KCoreResult(k, workingNodes.ToList(), kCoreEdges);
        }

        private Dictionary<string, int> CalculateUndirectedDegrees(HashSet<string> nodeIds)
        {
            var degrees = nodeIds.ToDictionary(id => id, _ => 0);

            foreach (var edge in _graph.GetAllEdges().Where(e => e.Type == EdgeType.Citation))
            {
                if (nodeIds.Contains(edge.Source.Id) && nodeIds.Contains(edge.Target.Id))
                {
                    degrees[edge.Source.Id]++;
                    degrees[edge.Target.Id]++;
                }
            }

            return degrees;
        }

        private HashSet<string> GetUndirectedNeighbors(string nodeId)
        {
            var neighbors = new HashSet<string>();

            foreach (var edge in _graph.GetAllEdges().Where(e => e.Type == EdgeType.Citation))
            {
                if (edge.Source.Id == nodeId)
                    neighbors.Add(edge.Target.Id);

                if (edge.Target.Id == nodeId)
                    neighbors.Add(edge.Source.Id);
            }

            return neighbors;
        }

        private List<(string Source, string Target)> GetEdgesBetweenNodes(HashSet<string> nodeIds)
        {
            var edges = new List<(string, string)>();

            foreach (var edge in _graph.GetAllEdges().Where(e => e.Type == EdgeType.Citation))
            {
                if (nodeIds.Contains(edge.Source.Id) && nodeIds.Contains(edge.Target.Id))
                {
                    edges.Add((edge.Source.Id, edge.Target.Id));
                }
            }

            return edges;
        }
    }

    public class KCoreResult
    {
        public int K { get; }
        public IReadOnlyList<string> NodeIds { get; }
        public IReadOnlyList<(string Source, string Target)> Edges { get; }

        public KCoreResult(int k, List<string> nodeIds, List<(string Source, string Target)> edges)
        {
            K = k;
            NodeIds = nodeIds;
            Edges = edges;
        }
    }
}