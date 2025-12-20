using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Visualization.Msagl
{
    /// <summary>
    /// GraphNode için görsel stil bilgisini üretir.
    /// MSAGL veya başka bir çizim kütüphanesinden BAĞIMSIZDIR.
    /// </summary>
    public class NodeStyleService
    {
        public NodeVisualStyle GetNodeStyle(GraphNode node)
        {
            if (node.IsSelected)
            {
                return NodeVisualStyle.Selected;
            }

            if (node.IsNewlyAdded)
            {
                return NodeVisualStyle.NewlyAdded;
            }

            return NodeVisualStyle.Default;
        }

        public EdgeVisualStyle GetEdgeStyle(EdgeType type)
        {
            return type switch
            {
                EdgeType.Citation => EdgeVisualStyle.Citation,
                EdgeType.Sequential => EdgeVisualStyle.Sequential,
                _ => EdgeVisualStyle.Default
            };
        }
    }

    /// <summary>
    /// UI bağımsız node stil türleri
    /// </summary>
    public enum NodeVisualStyle
    {
        Default,
        Selected,
        NewlyAdded
    }

    /// <summary>
    /// UI bağımsız edge stil türleri
    /// </summary>
    public enum EdgeVisualStyle
    {
        Default,
        Citation,
        Sequential
    }
}
