namespace Article_Graph_Analysis_Application.Models
{
    public class Paper
    {
        // === JSON ALANLARI ===
        public string Id { get; set; } = string.Empty;
        public List<string> Authors { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }

        // Out-degree
        public List<string> ReferencedWorks { get; set; } = new();

        // In-degree (duplicate engelli)
        public HashSet<string> CitedBy { get; set; } = new();

        // === HESAPLANABİLİR ===
        public int OutCitationCount => ReferencedWorks.Count;
        public int InCitationCount => CitedBy.Count;
    }
}
