using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core.KCore
{
    public class KCoreDecomposition
    {
        public static HashSet<string> FindKCore(Graph graph, int k)
        {
            var kCoreNodes = new HashSet<string>(graph.Nodes.Keys);
            var degrees = new Dictionary<string, int>();

            foreach (var node in graph.Nodes.Values)
            {
                var neighbors = new HashSet<string>();
                neighbors.UnionWith(node.OutgoingNodes.Select(n => n.Id));
                neighbors.UnionWith(node.IncomingNodes.Select(n => n.Id));
                degrees[node.Id] = neighbors.Count;
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                var nodesToRemove = new List<string>();

                foreach (var nodeId in kCoreNodes)
                {
                    int currentDegree = 0;
                    var node = graph.GetNode(nodeId);
                    
                    if (node == null) continue;

                    var neighbors = new HashSet<string>();
                    neighbors.UnionWith(node.OutgoingNodes.Select(n => n.Id));
                    neighbors.UnionWith(node.IncomingNodes.Select(n => n.Id));

                    foreach (var neighbor in neighbors)
                    {
                        if (kCoreNodes.Contains(neighbor))
                        {
                            currentDegree++;
                        }
                    }

                    if (currentDegree < k)
                    {
                        nodesToRemove.Add(nodeId);
                        changed = true;
                    }
                }

                foreach (var nodeId in nodesToRemove)
                {
                    kCoreNodes.Remove(nodeId);
                }
            }

            return kCoreNodes;
        }
    }
}