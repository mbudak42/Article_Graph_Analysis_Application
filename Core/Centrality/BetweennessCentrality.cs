using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core.Centrality
{
    public class BetweennessCentrality
    {
        public static Dictionary<string, double> Calculate(Graph graph)
        {
            var centrality = new Dictionary<string, double>();
            foreach (var nodeId in graph.Nodes.Keys)
            {
                centrality[nodeId] = 0.0;
            }

            var nodesList = graph.Nodes.Values.ToList();

            for (int i = 0; i < nodesList.Count; i++)
            {
                for (int j = i + 1; j < nodesList.Count; j++)
                {
                    var source = nodesList[i];
                    var target = nodesList[j];

                    var paths = FindAllShortestPaths(graph, source, target);

                    if (paths.Count > 0)
                    {
                        foreach (var path in paths)
                        {
                            for (int k = 1; k < path.Count - 1; k++)
                            {
                                centrality[path[k].Id] += 1.0 / paths.Count;
                            }
                        }
                    }
                }
            }

            return centrality;
        }

        private static List<List<GraphNode>> FindAllShortestPaths(Graph graph, GraphNode source, GraphNode target)
        {
            var allPaths = new List<List<GraphNode>>();
            var queue = new Queue<List<GraphNode>>();
            queue.Enqueue(new List<GraphNode> { source });

            int shortestLength = int.MaxValue;

            while (queue.Count > 0)
            {
                var currentPath = queue.Dequeue();
                var lastNode = currentPath[currentPath.Count - 1];

                if (currentPath.Count > shortestLength)
                {
                    continue;
                }

                if (lastNode.Id == target.Id)
                {
                    if (currentPath.Count < shortestLength)
                    {
                        shortestLength = currentPath.Count;
                        allPaths.Clear();
                    }

                    if (currentPath.Count == shortestLength)
                    {
                        allPaths.Add(new List<GraphNode>(currentPath));
                    }
                    continue;
                }

                foreach (var neighbor in GetNeighbors(lastNode))
                {
                    if (!currentPath.Contains(neighbor))
                    {
                        var newPath = new List<GraphNode>(currentPath) { neighbor };
                        queue.Enqueue(newPath);
                    }
                }
            }

            return allPaths;
        }

        private static List<GraphNode> GetNeighbors(GraphNode node)
        {
            var neighbors = new HashSet<GraphNode>();
            neighbors.UnionWith(node.OutgoingNodes);
            neighbors.UnionWith(node.IncomingNodes);
            return neighbors.ToList();
        }
    }
}