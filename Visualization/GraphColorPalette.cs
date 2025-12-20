using Microsoft.Msagl.Drawing;

namespace Article_Graph_Analysis_Application.Visualization
{
    public static class GraphColorPalette
    {
        public static Color DefaultNode => Color.LightGray;
        public static Color SelectedNode => Color.LightGoldenrodYellow;
        public static Color NewlyAddedNode => Color.LightBlue;
        public static Color KCoreNode => Color.LightCoral;
        
        public static Color CitationEdge => Color.Black;
        public static Color SequentialEdge => Color.Green;
        public static Color KCoreEdge => Color.Red;
        public static Color DefaultEdge => Color.Gray;
    }
}