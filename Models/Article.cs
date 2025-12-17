using System.Collections.Generic;

namespace Article_Graph_Analysis_Application.Models
{
    public class Article
    {
        public string? Id { get; set; }           
        public string? Title { get; set; }        
        public List<string> Authors { get; set; } 
        public int Year { get; set; }            
        public List<string> ReferencedWorks { get; set; } 
        public int CitationCount { get; set; }   

        public Article()
        {
            Authors = new List<string>();
            ReferencedWorks = new List<string>();
        }
    }
}