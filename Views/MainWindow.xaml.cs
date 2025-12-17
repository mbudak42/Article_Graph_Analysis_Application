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
using Microsoft.Msagl.WpfGraphControl;

namespace Article_Graph_Analysis_Application.Views
{
    public enum ViewLevel { Overview, Medium, Detailed }

    public partial class MainWindow : Window
    {
        private List<Article>? _allArticles;
        private HashSet<string> _visibleNodeIds = new HashSet<string>();
        private ViewLevel _currentLevel = ViewLevel.Overview;
        
        private const int MAX_OVERVIEW_NODES = 50;
        private const int MAX_MEDIUM_NODES = 150;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            KeyDown += MainWindow_KeyDown;
            
            // MouseDown olayını doğrudan kontrolün kendisine bağlıyoruz
            GraphViewer.MouseDown += GraphViewer_MouseDown;
        }

        private void GraphViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // CS1061 HATASININ KESİN ÇÖZÜMÜ: 
            // Kontrolü (IViewer) arayüzüne cast ederek ObjectUnderMouseCursor'a ulaşıyoruz.
            // Bu yöntem sürüm farklarını ortadan kaldırır.
            var viewer = GraphViewer as Microsoft.Msagl.Drawing.IViewer;
            
            if (viewer == null || viewer.ObjectUnderMouseCursor == null) return;

            var clickedObject = viewer.ObjectUnderMouseCursor;
            string? articleId = null;

            // Düğüm tipini tespit ediyoruz
            if (clickedObject is IViewerNode vNode)
            {
                articleId = vNode.Node.Id;
            }
            else if (clickedObject is Microsoft.Msagl.Drawing.Node dNode)
            {
                articleId = dNode.Id;
            }

            if (!string.IsNullOrEmpty(articleId))
            {
                ExpandNode(articleId);
            }
        }

        private async void ExpandNode(string articleId)
        {
            if (_allArticles == null) return;

            var article = _allArticles.FirstOrDefault(a => a.Id == articleId);
            if (article == null) return;

            bool addedAny = false;
            foreach (var refId in article.ReferencedWorks)
            {
                if (!_visibleNodeIds.Contains(refId) && _allArticles.Any(a => a.Id == refId))
                {
                    _visibleNodeIds.Add(refId);
                    addedAny = true;
                }
            }

            if (addedAny)
            {
                StatusText.Text = $"Genişletildi: {articleId.Split('/').Last()} bağlantıları eklendi.";
                await RenderGraphAsync();
            }
        }

        private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1) _currentLevel = ViewLevel.Overview;
            else if (e.Key == Key.F2) _currentLevel = ViewLevel.Medium;
            else if (e.Key == Key.F3) _currentLevel = ViewLevel.Detailed;
            else if (e.Key == Key.F4) { ShowTopArticles(); return; }
            else if (e.Key == Key.F5) { _allArticles = null; await CreateDataAndRenderAsync(); return; }
            else return;

            ApplyLevelFilter();
            await RenderGraphAsync();
        }

        private void ShowTopArticles()
        {
            if (_allArticles == null) return;
            var top5 = _allArticles.OrderByDescending(a => a.CitationCount).Take(5).ToList();
            string msg = "--- EN ÇOK ATIF ALAN 5 MAKALE ---\n\n" + 
                         string.Join("\n", top5.Select(a => $"{a.Id} (Atıf: {a.CitationCount})"));
            MessageBox.Show(msg, "Analiz Raporu");
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CreateDataAndRenderAsync();
        }

        private async Task CreateDataAndRenderAsync()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "DataJson.json");
                if (_allArticles == null)
                {
                    JsonParser parser = new JsonParser();
                    _allArticles = await Task.Run(() => parser.ParseArticles(jsonPath));
                    CalculateCitationCounts();
                }
                ApplyLevelFilter();
                await RenderGraphAsync();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void ApplyLevelFilter()
        {
            if (_allArticles == null) return;
            _visibleNodeIds.Clear();
            var sorted = _allArticles.OrderByDescending(a => a.CitationCount).ToList();
            var selected = _currentLevel switch {
                ViewLevel.Overview => sorted.Take(MAX_OVERVIEW_NODES).ToList(),
                ViewLevel.Medium => sorted.Take(MAX_MEDIUM_NODES).ToList(),
                _ => _allArticles.Where(a => a.CitationCount > 0 || a.ReferencedWorks.Any()).ToList()
            };
            foreach (var a in selected) if (a.Id != null) _visibleNodeIds.Add(a.Id);
        }

        private async Task RenderGraphAsync()
        {
            if (_allArticles == null) return;
            Graph graph = new Graph("articleGraph");
            graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings();

            foreach (var id in _visibleNodeIds) {
                var art = _allArticles.FirstOrDefault(a => a.Id == id);
                if (art == null) continue;
                Node n = graph.AddNode(id);
                n.LabelText = id.Split('/').Last();
                n.Attr.FillColor = GetNodeColor(art.CitationCount);
                n.Attr.Shape = art.CitationCount >= 10 ? Shape.Diamond : Shape.Box;
            }

            foreach (var art in _allArticles) {
                if (art.Id == null || !_visibleNodeIds.Contains(art.Id)) continue;
                foreach (var rId in art.ReferencedWorks) {
                    if (_visibleNodeIds.Contains(rId)) graph.AddEdge(art.Id, rId).Attr.ArrowheadAtTarget = ArrowStyle.None;
                }
            }

            await Task.Run(() => graph.CreateGeometryGraph());
            GraphViewer.Graph = graph;
            StatusText.Text = $"Graf [{_currentLevel}] - {_visibleNodeIds.Count} Makale gösteriliyor.";
        }

        private void CalculateCitationCounts() {
            if (_allArticles == null) return;
            var dict = _allArticles.Where(a => a.Id != null).ToDictionary(a => a.Id!, a => a);
            foreach (var a in _allArticles) 
                foreach (var r in a.ReferencedWorks) 
                    if (dict.ContainsKey(r)) dict[r].CitationCount++;
        }

        private Color GetNodeColor(int c) {
            if (c >= 20) return Color.Red;
            if (c >= 10) return Color.Orange;
            if (c >= 5) return Color.Yellow;
            return Color.LightBlue;
        }
    }
}