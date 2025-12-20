namespace Article_Graph_Analysis_Application.Models
{
    public class Paper
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<string> Authors { get; set; }
        public int Year { get; set; }
        public List<string> ReferencedWorks { get; set; }
        public int InCitationCount { get; set; }

        public Paper()
        {
            Authors = new List<string>();
            ReferencedWorks = new List<string>();
            InCitationCount = 0;
        }
    }
}