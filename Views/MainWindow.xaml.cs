using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Article_Graph_Analysis_Application.Helpers;
using Article_Graph_Analysis_Application.Models;
using Microsoft.Msagl.Drawing;

namespace Article_Graph_Analysis_Application.Views
{
    public enum ViewLevel { Overview, Medium, Detailed }

    public partial class MainWindow : Window
    {
        private List<Article>? _allArticles;
        private HashSet<string> _visibleNodeIds = new HashSet<string>();
        private ViewLevel _currentLevel = ViewLevel.Overview;
        private const int MAX_OVERVIEW_NODES = 100;
        private const int MAX_COMMUNITY_NODES = 300;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            KeyDown += MainWindow_KeyDown;
        }

        private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1) _currentLevel = ViewLevel.Overview;
            else if (e.Key == Key.F2) _currentLevel = ViewLevel.Medium;
            else if (e.Key == Key.F3) _currentLevel = ViewLevel.Detailed;
            else if (e.Key == Key.F5) _currentLevel = ViewLevel.Overview;
            else return;

            await CreateGraphAsync();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CreateGraphAsync();
        }

        private async Task CreateGraphAsync()
        {
            try
            {
                this.Title = "Graf Analizi Yapılıyor...";
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string jsonPath = Path.Combine(basePath, "Assets", "DataJson.json");

                if (_allArticles == null)
                {
                    JsonParser parser = new JsonParser();
                    _allArticles = await Task.Run(() => parser.ParseArticles(jsonPath));
                    CalculateCitationCounts();
                }

                Graph graph = new Graph("citationGraph");
                
                // Tasarımı düzelten ayar:
                graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings();

                List<Article> filteredArticles = GetFilteredArticles();
                _visibleNodeIds.Clear();

                foreach (var article in filteredArticles)
                {
                    if (string.IsNullOrEmpty(article.Id)) continue;
                    
                    Node node = graph.AddNode(article.Id);
                    node.LabelText = article.Id.Split('/').Last();
                    node.Attr.FillColor = GetNodeColor(article.CitationCount);
                    node.Attr.Shape = article.CitationCount >= 10 ? Shape.Diamond : Shape.Box;
                    _visibleNodeIds.Add(article.Id);
                }

                foreach (var article in filteredArticles)
                {
                    if (string.IsNullOrEmpty(article.Id)) continue;
                    foreach (var refId in article.ReferencedWorks)
                    {
                        if (_visibleNodeIds.Contains(refId))
                        {
                            Edge edge = graph.AddEdge(article.Id, refId);
                            edge.Attr.Color = Color.Black;
                            edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                        }
                    }
                }

                await Task.Run(() => graph.CreateGeometryGraph());
                GraphViewer.Graph = graph;
                
                this.Title = $"Graf [{_currentLevel}] - {filteredArticles.Count} Makale (F1-F2-F3: Seviyeler)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CalculateCitationCounts()
        {
            if (_allArticles == null) return;
            var dict = _allArticles.Where(a => a.Id != null).ToDictionary(a => a.Id!, a => a);
            foreach (var article in _allArticles)
            {
                foreach (var refId in article.ReferencedWorks)
                {
                    if (dict.ContainsKey(refId)) dict[refId].CitationCount++;
                }
            }
        }

        private List<Article> GetFilteredArticles()
        {
            if (_allArticles == null) return new List<Article>();
            var sorted = _allArticles.OrderByDescending(a => a.CitationCount).ToList();

            return _currentLevel switch
            {
                ViewLevel.Overview => sorted.Take(MAX_OVERVIEW_NODES).ToList(),
                ViewLevel.Medium => sorted.Take(MAX_COMMUNITY_NODES).ToList(),
                _ => _allArticles.Where(a => a.CitationCount > 0 || a.ReferencedWorks.Any()).ToList()
            };
        }

        private Color GetNodeColor(int count)
        {
            if (count >= 20) return Color.Red;
            if (count >= 10) return Color.Orange;
            if (count >= 5) return Color.Yellow;
            return Color.LightBlue;
        }
    }
}