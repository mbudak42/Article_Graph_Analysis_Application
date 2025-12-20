using Article_Graph_Analysis_Application.Models;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System.Windows;
using System.Windows.Threading;
using CoreGraph = Article_Graph_Analysis_Application.Core.Graph;
using MsaglGraph = Microsoft.Msagl.Drawing.Graph;

namespace Article_Graph_Analysis_Application.Visualization.Msagl
{
    public class MsaglGraphController
    {
        private GraphViewer viewer;
        private CoreGraph? displayGraph;
        private HashSet<string> clickedNodes;
        private HashSet<string> currentHCoreNodes;

        public MsaglGraphController(GraphViewer viewer)
        {
            this.viewer = viewer;
            this.clickedNodes = new HashSet<string>();
            this.currentHCoreNodes = new HashSet<string>();
        }

        public void DrawGraph(CoreGraph graph)
        {
            displayGraph = graph;
            var msaglGraph = new MsaglGraph();

            foreach (var node in graph.Nodes.Values)
            {
                var msaglNode = msaglGraph.AddNode(node.Id);
                msaglNode.LabelText = node.Id;

                int citationCount = node.GetInDegree();
                NodeStyleService.ApplyNodeStyle(msaglNode, GraphColorPalette.DefaultNodeColor, citationCount);
            }

            foreach (var edge in graph.Edges)
            {
                var msaglEdge = msaglGraph.AddEdge(edge.Source.Id, edge.Target.Id);
                NodeStyleService.ApplyEdgeStyle(msaglEdge, Color.Black, 1.0);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                viewer.Graph = msaglGraph;
            }, DispatcherPriority.Render);
        }

        public void HighlightHCore(List<GraphNode> hCoreNodes, string clickedNodeId)
        {
            if (viewer.Graph == null || displayGraph == null) return;

            clickedNodes.Add(clickedNodeId);

            var newHCoreIds = hCoreNodes.Select(n => n.Id).Where(id => !currentHCoreNodes.Contains(id)).ToHashSet();
            currentHCoreNodes.UnionWith(hCoreNodes.Select(n => n.Id));

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Node node in viewer.Graph.Nodes)
                {
                    var graphNode = displayGraph.GetNode(node.Id);
                    if (graphNode == null) continue;

                    int citationCount = graphNode.GetInDegree();

                    if (node.Id == clickedNodeId)
                    {
                        NodeStyleService.ApplyNodeStyle(node, GraphColorPalette.ClickedNodeColor, citationCount);
                    }
                    else if (newHCoreIds.Contains(node.Id))
                    {
                        NodeStyleService.ApplyNodeStyle(node, GraphColorPalette.NewHCoreNodeColor, citationCount);
                    }
                    else if (currentHCoreNodes.Contains(node.Id))
                    {
                        NodeStyleService.ApplyNodeStyle(node, GraphColorPalette.HCoreNodeColor, citationCount);
                    }
                    else if (clickedNodes.Contains(node.Id))
                    {
                        NodeStyleService.ApplyNodeStyle(node, GraphColorPalette.ClickedNodeColor, citationCount);
                    }
                }

                viewer.Invalidate();
            }, DispatcherPriority.Render);
        }

        public void HighlightKCore(HashSet<string> kCoreNodeIds)
        {
            if (viewer.Graph == null || displayGraph == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Node node in viewer.Graph.Nodes)
                {
                    var graphNode = displayGraph.GetNode(node.Id);
                    if (graphNode == null) continue;

                    int citationCount = graphNode.GetInDegree();

                    if (kCoreNodeIds.Contains(node.Id))
                    {
                        NodeStyleService.ApplyNodeStyle(node, GraphColorPalette.KCoreNodeColor, citationCount);
                    }
                    else
                    {
                        NodeStyleService.ApplyNodeStyle(node, GraphColorPalette.DefaultNodeColor, citationCount);
                    }
                }

                foreach (Edge edge in viewer.Graph.Edges)
                {
                    if (kCoreNodeIds.Contains(edge.Source) && kCoreNodeIds.Contains(edge.Target))
                    {
                        NodeStyleService.ApplyEdgeStyle(edge, GraphColorPalette.KCoreNodeColor, 2.0);
                    }
                    else
                    {
                        NodeStyleService.ApplyEdgeStyle(edge, Color.LightGray, 1.0);
                    }
                }

                viewer.Invalidate();
            }, DispatcherPriority.Render);
        }

        public void ResetHighlights()
        {
            clickedNodes.Clear();
            currentHCoreNodes.Clear();
        }
    }
}