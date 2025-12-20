namespace Article_Graph_Analysis_Application.Models
{
    public class GraphNode
    {
        public string Id { get; set; }
        public Paper Paper { get; set; }
        public List<GraphNode> OutgoingNodes { get; set; }
        public List<GraphNode> IncomingNodes { get; set; }

        public GraphNode(Paper paper)
        {
            Id = paper.Id;
            Paper = paper;
            OutgoingNodes = new List<GraphNode>();
            IncomingNodes = new List<GraphNode>();
        }

        public int GetInDegree()
        {
            return IncomingNodes.Count;
        }

        public int GetOutDegree()
        {
            return OutgoingNodes.Count;
        }
    }
}