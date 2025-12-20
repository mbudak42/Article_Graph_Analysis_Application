using System;
using System.Collections.Generic;
using System.Linq;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core.Centrality
{
    public class BetweennessCentrality
    {
        private readonly Graph _graph;

        public BetweennessCentrality(Graph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        public Dictionary<string, double> CalculateForAllNodes()
        {
            var allNodes = _graph.GetAllNodes().Select(n => n.Id).ToList();
            var betweenness = allNodes.ToDictionary(id => id, _ => 0.0);

            foreach (var source in allNodes)
            {
                var (predecessors, distances) = BfsShortestPaths(source);
                AccumulateBetweenness(source, predecessors, distances, betweenness);
            }

            foreach (var key in betweenness.Keys.ToList())
            {
                betweenness[key] /= 2.0;
            }

            return betweenness;
        }

        private (Dictionary<string, List<string>> predecessors, Dictionary<string, int> distances) 
            BfsShortestPaths(string source)
        {
            var distances = new Dictionary<string, int> { [source] = 0 };
            var predecessors = new Dictionary<string, List<string>>();
            var queue = new Queue<string>();
            queue.Enqueue(source);

            foreach (var node in _graph.GetAllNodes())
            {
                predecessors[node.Id] = new List<string>();
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int currentDist = distances[current];

                var neighbors = GetUndirectedNeighbors(current);

                foreach (var neighbor in neighbors)
                {
                    if (!distances.ContainsKey(neighbor))
                    {
                        distances[neighbor] = currentDist + 1;
                        queue.Enqueue(neighbor);
                    }

                    if (distances[neighbor] == currentDist + 1)
                    {
                        predecessors[neighbor].Add(current);
                    }
                }
            }

            return (predecessors, distances);
        }

        private void AccumulateBetweenness(
            string source,
            Dictionary<string, List<string>> predecessors,
            Dictionary<string, int> distances,
            Dictionary<string, double> betweenness)
        {
            var delta = new Dictionary<string, double>();
            foreach (var node in _graph.GetAllNodes())
            {
                delta[node.Id] = 0.0;
            }

            var sortedNodes = distances
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var w in sortedNodes)
            {
                if (w == source) continue;

                foreach (var v in predecessors[w])
                {
                    int sigmaV = CountPathsTo(source, v, predecessors, distances);
                    int sigmaW = CountPathsTo(source, w, predecessors, distances);

                    if (sigmaW > 0)
                    {
                        delta[v] += (sigmaV / (double)sigmaW) * (1.0 + delta[w]);
                    }
                }

                if (w != source)
                {
                    betweenness[w] += delta[w];
                }
            }
        }

        private int CountPathsTo(
            string source, 
            string target, 
            Dictionary<string, List<string>> predecessors,
            Dictionary<string, int> distances)
        {
            if (!distances.ContainsKey(target))
                return 0;

            if (target == source)
                return 1;

            var memo = new Dictionary<string, int>();
            return CountPathsRecursive(source, target, predecessors, memo);
        }

        private int CountPathsRecursive(
            string source,
            string current,
            Dictionary<string, List<string>> predecessors,
            Dictionary<string, int> memo)
        {
            if (current == source)
                return 1;

            if (memo.ContainsKey(current))
                return memo[current];

            int total = 0;
            foreach (var pred in predecessors[current])
            {
                total += CountPathsRecursive(source, pred, predecessors, memo);
            }

            memo[current] = total;
            return total;
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

        public double CalculateForNode(string nodeId)
        {
            var allResults = CalculateForAllNodes();
            return allResults.TryGetValue(nodeId, out var value) ? value : 0.0;
        }
    }
}