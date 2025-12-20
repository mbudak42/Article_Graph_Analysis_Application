using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
    public class GraphExpander
    {
        private Graph mainGraph;
        private Graph displayGraph;

        public GraphExpander(Graph mainGraph, Graph displayGraph)
        {
            this.mainGraph = mainGraph;
            this.displayGraph = displayGraph;
        }

        public void ExpandWithHCore(List<GraphNode> hCoreNodes)
        {
            foreach (var node in hCoreNodes)
            {
                if (!displayGraph.ContainsNode(node.Id))
                {
                    displayGraph.AddNode(node);
                }
            }

            var allDisplayNodeIds = new HashSet<string>(displayGraph.Nodes.Keys);

            foreach (var nodeId in allDisplayNodeIds)
            {
                var mainNode = mainGraph.GetNode(nodeId);
                if (mainNode == null) continue;

                foreach (var outNode in mainNode.OutgoingNodes)
                {
                    if (allDisplayNodeIds.Contains(outNode.Id))
                    {
                        var displaySource = displayGraph.GetNode(nodeId);
                        var displayTarget = displayGraph.GetNode(outNode.Id);

                        if (displaySource == null || displayTarget == null) continue;

                        bool edgeExists = displayGraph.Edges.Any(e => 
                            e.Source.Id == displaySource.Id && e.Target.Id == displayTarget.Id);

                        if (!edgeExists)
                        {
                            displayGraph.AddEdge(displaySource, displayTarget);
                        }
                    }
                }
            }
        }
    }
}