using Microsoft.Msagl.Drawing;

namespace Article_Graph_Analysis_Application.Visualization.Msagl
{
    public class NodeStyleService
    {
        public static void ApplyNodeStyle(Node node, Color color, int citationCount)
        {
            node.Attr.FillColor = color;
            node.Attr.Shape = Shape.Circle;
            
            int size = Math.Max(20, Math.Min(80, 20 + citationCount * 2));
            node.Attr.LabelMargin = size / 4;
            
            node.Label.FontSize = Math.Max(8, Math.Min(16, 8 + citationCount / 5));
        }

        public static void ApplyEdgeStyle(Edge edge, Color color, double width = 1.0)
        {
            edge.Attr.Color = color;
            edge.Attr.LineWidth = width;
        }
    }
}