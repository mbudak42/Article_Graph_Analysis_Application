using System;
using System.Linq;
using Article_Graph_Analysis_Application.Core;
using Article_Graph_Analysis_Application.Core.KCore;
using Article_Graph_Analysis_Application.Models;
using Article_Graph_Analysis_Application.Visualization;
using MsaglGraph = Microsoft.Msagl.Drawing.Graph;
using MsaglNode = Microsoft.Msagl.Drawing.Node;
using MsaglEdge = Microsoft.Msagl.Drawing.Edge;
using Microsoft.Msagl.Drawing;

namespace Article_Graph_Analysis_Application.Visualization.Msagl
{
    public class MsaglGraphController
    {
        private readonly NodeStyleService _styleService;

        public MsaglGraphController(NodeStyleService styleService)
        {
            _styleService = styleService ?? throw new ArgumentNullException(nameof(styleService));
        }

        public MsaglGraph BuildMsaglGraph(Core.Graph domainGraph)
        {
            if (domainGraph == null) throw new ArgumentNullException(nameof(domainGraph));

            var msGraph = new MsaglGraph("ArticleGraph");

            foreach (var node in domainGraph.GetAllNodes())
            {
                var msNode = msGraph.AddNode(node.Id);
                ApplyNodeStyle(msNode, node);
            }

            foreach (var edge in domainGraph.GetAllEdges())
            {
                var msEdge = msGraph.AddEdge(edge.Source.Id, edge.Target.Id);
                ApplyEdgeStyle(msEdge, edge.Type);
            }

            return msGraph;
        }

        public MsaglGraph BuildMsaglGraphWithKCore(Core.Graph domainGraph, KCoreResult kCoreResult)
        {
            if (domainGraph == null) throw new ArgumentNullException(nameof(domainGraph));
            if (kCoreResult == null) throw new ArgumentNullException(nameof(kCoreResult));

            var msGraph = new MsaglGraph("ArticleGraph");
            var kCoreNodeSet = kCoreResult.NodeIds.ToHashSet();
            var kCoreEdgeSet = kCoreResult.Edges.Select(e => (e.Source, e.Target)).ToHashSet();

            foreach (var node in domainGraph.GetAllNodes())
            {
                var msNode = msGraph.AddNode(node.Id);
                
                if (kCoreNodeSet.Contains(node.Id))
                {
                    msNode.Attr.FillColor = GraphColorPalette.KCoreNode;
                    msNode.Attr.LineWidth = 3;
                }
                else
                {
                    ApplyNodeStyle(msNode, node);
                }
            }

            foreach (var edge in domainGraph.GetAllEdges())
            {
                var msEdge = msGraph.AddEdge(edge.Source.Id, edge.Target.Id);
                
                if (edge.Type == EdgeType.Citation && 
                    kCoreEdgeSet.Contains((edge.Source.Id, edge.Target.Id)))
                {
                    msEdge.Attr.Color = GraphColorPalette.KCoreEdge;
                    msEdge.Attr.LineWidth = 3;
                }
                else
                {
                    ApplyEdgeStyle(msEdge, edge.Type);
                }
            }

            return msGraph;
        }

        private void ApplyNodeStyle(MsaglNode msNode, GraphNode domainNode)
        {
            var style = _styleService.GetNodeStyle(domainNode);

            msNode.Attr.Shape = Shape.Box;
            msNode.Attr.LineWidth = 1;

            switch (style)
            {
                case NodeVisualStyle.Selected:
                    msNode.Attr.FillColor = GraphColorPalette.SelectedNode;
                    msNode.Attr.LineWidth = 3;
                    break;

                case NodeVisualStyle.NewlyAdded:
                    msNode.Attr.FillColor = GraphColorPalette.NewlyAddedNode;
                    msNode.Attr.LineWidth = 2;
                    break;

                default:
                    msNode.Attr.FillColor = GraphColorPalette.DefaultNode;
                    break;
            }

            msNode.LabelText = domainNode.Id;
        }

        private void ApplyEdgeStyle(MsaglEdge msEdge, EdgeType type)
        {
            var style = _styleService.GetEdgeStyle(type);

            msEdge.Attr.LineWidth = 1.5;
            msEdge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;

            switch (style)
            {
                case EdgeVisualStyle.Citation:
                    msEdge.Attr.Color = GraphColorPalette.CitationEdge;
                    break;

                case EdgeVisualStyle.Sequential:
                    msEdge.Attr.Color = GraphColorPalette.SequentialEdge;
                    break;

                default:
                    msEdge.Attr.Color = GraphColorPalette.DefaultEdge;
                    break;
            }
        }
    }
}