using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core.HIndex
{
    public class HIndexResult
    {
        public int HIndex { get; set; }
        public double HMedian { get; set; }
        public List<GraphNode> HCore { get; set; }

        public HIndexResult()
        {
            HCore = new List<GraphNode>();
        }
    }
}