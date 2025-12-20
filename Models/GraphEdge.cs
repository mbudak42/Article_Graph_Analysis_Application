namespace Article_Graph_Analysis_Application.Models
{
    public class GraphEdge
    {
        public GraphNode Source { get; set; }
        public GraphNode Target { get; set; }
        public bool IsDirected { get; set; }

        public GraphEdge(GraphNode source, GraphNode target, bool isDirected = true)
        {
            Source = source;
            Target = target;
            IsDirected = isDirected;
        }
    }
}