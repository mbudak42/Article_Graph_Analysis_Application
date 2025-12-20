using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
    public class GraphExpander
    {
        private readonly Graph mainGraph;
        private readonly Graph displayGraph;

        public GraphExpander(Graph mainGraph, Graph displayGraph)
        {
            this.mainGraph = mainGraph;
            this.displayGraph = displayGraph;
        }

        public void ExpandWithHCore(List<GraphNode> hCoreNodes)
        {
            var nodesToAdd = new List<GraphNode>();
            
            foreach (var hCoreNode in hCoreNodes.ToList())
            {
                if (!displayGraph.ContainsNode(hCoreNode.Id))
                {
                    nodesToAdd.Add(hCoreNode);
                }
            }

            foreach (var node in nodesToAdd)
            {
                displayGraph.AddNode(node);
            }

            foreach (var node in nodesToAdd)
            {
                foreach (var outgoing in node.OutgoingNodes)
                {
                    if (displayGraph.ContainsNode(outgoing.Id))
                    {
                        displayGraph.AddEdge(node, outgoing);
                    }
                }

                foreach (var incoming in node.IncomingNodes)
                {
                    if (displayGraph.ContainsNode(incoming.Id))
                    {
                        displayGraph.AddEdge(incoming, node);
                    }
                }
            }
        }
    }
}