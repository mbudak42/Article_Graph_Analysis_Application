namespace Article_Graph_Analysis_Application.Models
{
    public class Paper
    {
        private string _id = string.Empty;
        private List<string> _referencedWorks = new();
        
        public string Id 
        { 
            get => _id;
            set 
            {
                _id = value.Replace("https://openalex.org/", "");
            }
        }
        
        public List<string> ReferencedWorks 
        { 
            get => _referencedWorks;
            set 
            {
                _referencedWorks = value.Select(v => v.Replace("https://openalex.org/", "")).ToList();
            }
        }
        
        public List<string> Authors { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public int InCitationCount { get; set; }
    }
}