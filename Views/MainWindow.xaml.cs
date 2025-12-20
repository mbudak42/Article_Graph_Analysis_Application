using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Article_Graph_Analysis_Application.Core;
using Article_Graph_Analysis_Application.Core.HIndex;
using Article_Graph_Analysis_Application.Core.Centrality;
using Article_Graph_Analysis_Application.Core.KCore;
using Article_Graph_Analysis_Application.Models;
using Article_Graph_Analysis_Application.Services;
using Article_Graph_Analysis_Application.Services.Json;
using Article_Graph_Analysis_Application.Visualization.Msagl;

using Microsoft.Msagl.WpfGraphControl;

namespace Article_Graph_Analysis_Application.Views
{
    public partial class MainWindow : Window
    {
        private Graph _fullGraph = null!;
        private Graph _viewGraph = null!;
        private HIndexCalculator _hIndexCalculator = null!;
        private GraphExpander _graphExpander = null!;
        private MsaglGraphController _graphController = null!;
        private GraphViewer _viewer = null!;
        private StatisticsService _statsService = null!;
        private KCoreResult? _currentKCoreResult = null;

        private TextBlock _txtTotalNodes = null!;
        private TextBlock _txtTotalEdges = null!;
        private TextBlock _txtMostCited = null!;
        private TextBlock _txtMostReferencing = null!;
        private TextBox _txtHIndexInput = null!;
        private TextBlock _txtHIndex = null!;
        private TextBlock _txtHMedian = null!;
        private TextBox _txtHCore = null!;
        private TextBox _txtBetweenness = null!;
        private TextBox _txtKValue = null!;
        private TextBlock _txtKCoreNodes = null!;
        private TextBlock _txtKCoreEdges = null!;

        public MainWindow()
        {
            InitializeComponent();
            BuildUI();
            Loaded += OnLoaded;
        }

        private void BuildUI()
        {
            var stack = new StackPanel();
            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = stack };
            RightPanel.Children.Add(scroll);

            stack.Children.Add(CreateHeader("GENEL İSTATİSTİKLER"));
            _txtTotalNodes = CreateLabeledText(stack, "Toplam Makale:", "0");
            _txtTotalEdges = CreateLabeledText(stack, "Toplam Referans:", "0");
            _txtMostCited = CreateLabeledText(stack, "En Çok Atıf Alan:", "-", true);
            _txtMostReferencing = CreateLabeledText(stack, "En Çok Referans Veren:", "-", true);
            
            stack.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            stack.Children.Add(CreateHeader("H-INDEX HESAPLAMA"));
            stack.Children.Add(CreateLabel("Makale ID:"));
            _txtHIndexInput = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(_txtHIndexInput);
            
            var btnHIndex = new Button { Content = "H-Index Hesapla", Margin = new Thickness(0, 0, 0, 10) };
            btnHIndex.Click += BtnCalculateHIndex_Click;
            stack.Children.Add(btnHIndex);

            _txtHIndex = CreateLabeledText(stack, "H-Index:", "-");
            _txtHMedian = CreateLabeledText(stack, "H-Median:", "-");
            
            stack.Children.Add(CreateLabel("H-Core Makaleler:"));
            _txtHCore = new TextBox { 
                IsReadOnly = true, 
                TextWrapping = TextWrapping.Wrap, 
                Height = 100, 
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(_txtHCore);

            stack.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            stack.Children.Add(CreateHeader("BETWEENNESS CENTRALITY"));
            var btnBetweenness = new Button { Content = "Tüm Düğümler İçin Hesapla", Margin = new Thickness(0, 0, 0, 10) };
            btnBetweenness.Click += BtnCalculateBetweenness_Click;
            stack.Children.Add(btnBetweenness);

            stack.Children.Add(CreateLabel("Sonuçlar (Top 10):"));
            _txtBetweenness = new TextBox { 
                IsReadOnly = true, 
                TextWrapping = TextWrapping.Wrap, 
                Height = 150, 
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(_txtBetweenness);

            stack.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            stack.Children.Add(CreateHeader("K-CORE DECOMPOSITION"));
            stack.Children.Add(CreateLabel("K Değeri:"));
            _txtKValue = new TextBox { Text = "2", Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(_txtKValue);

            var btnKCore = new Button { Content = "K-Core Uygula", Margin = new Thickness(0, 0, 0, 10) };
            btnKCore.Click += BtnApplyKCore_Click;
            stack.Children.Add(btnKCore);

            _txtKCoreNodes = CreateLabeledText(stack, "K-Core Düğüm Sayısı:", "-");
            _txtKCoreEdges = CreateLabeledText(stack, "K-Core Kenar Sayısı:", "-");
        }

        private TextBlock CreateHeader(string text)
        {
            return new TextBlock { 
                Text = text, 
                FontWeight = FontWeights.Bold, 
                FontSize = 14, 
                Margin = new Thickness(0, 0, 0, 10) 
            };
        }

        private TextBlock CreateLabel(string text)
        {
            return new TextBlock { 
                Text = text, 
                FontWeight = FontWeights.SemiBold, 
                Margin = new Thickness(0, 5, 0, 2) 
            };
        }

        private TextBlock CreateLabeledText(StackPanel parent, string label, string value, bool wrap = false)
        {
            parent.Children.Add(CreateLabel(label));
            var txt = new TextBlock { 
                Text = value, 
                Margin = new Thickness(10, 0, 0, 5) 
            };
            if (wrap) txt.TextWrapping = TextWrapping.Wrap;
            parent.Children.Add(txt);
            return txt;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var loader = new JsonPaperLoader();
                string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "articles.json");
                var papers = loader.LoadFromFile(jsonPath);

                var builder = new GraphBuilder();
                _fullGraph = builder.BuildGraph(papers);

                _statsService = new StatisticsService(_fullGraph);
                _statsService.UpdateCitationData();

                _viewGraph = new Graph();
                var firstPaper = papers.First();
                _viewGraph.AddNode(new GraphNode(firstPaper));

                _viewer = new GraphViewer();
                _viewer.BindToPanel(GraphHost);

                var styleService = new NodeStyleService();
                _graphController = new MsaglGraphController(styleService);

                _viewer.Graph = _graphController.BuildMsaglGraph(_viewGraph);

                _hIndexCalculator = new HIndexCalculator(_fullGraph);
                _graphExpander = new GraphExpander(_fullGraph);

                _ = new GraphInteractionHandler(
                    _fullGraph,
                    _viewGraph,
                    _hIndexCalculator,
                    _graphExpander,
                    _graphController,
                    _viewer,
                    OnGraphUpdated);

                UpdateStatistics();

                TestAllFeatures();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}\n\n{ex.StackTrace}", "Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestAllFeatures()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TEST BAŞLADI ===");
                
                System.Diagnostics.Debug.WriteLine($"✓ FullGraph Nodes: {_fullGraph.TotalNodeCount}");
                System.Diagnostics.Debug.WriteLine($"✓ FullGraph Edges: {_fullGraph.TotalEdgeCount}");
                
                var firstNode = _viewGraph.GetAllNodes().FirstOrDefault();
                System.Diagnostics.Debug.WriteLine($"✓ First Node: {firstNode?.Id}");
                
                if (firstNode != null)
                {
                    var hResult = _hIndexCalculator.Calculate(firstNode.Id);
                    System.Diagnostics.Debug.WriteLine($"✓ H-Index: {hResult.HIndex}, H-Median: {hResult.HMedian}");
                }
                
                var bc = new BetweennessCentrality(_viewGraph);
                var bcResults = bc.CalculateForAllNodes();
                System.Diagnostics.Debug.WriteLine($"✓ Betweenness calculated for {bcResults.Count} nodes");
                
                var kCore = new KCoreDecomposition(_viewGraph);
                var kResult = kCore.Decompose(1);
                System.Diagnostics.Debug.WriteLine($"✓ K-Core (k=1): {kResult.NodeIds.Count} nodes");
                
                System.Diagnostics.Debug.WriteLine("\n✓✓✓ TÜM TESTLER BAŞARILI! ✓✓✓");
                
                MessageBox.Show("Tüm testler başarılı! Output penceresini kontrol et.", "Test Sonucu", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ TEST HATASI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                
                MessageBox.Show($"Test hatası:\n{ex.Message}", "Test Hatası", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnGraphUpdated(HIndexResult hResult)
        {
            _txtHIndex.Text = hResult.HIndex.ToString();
            _txtHMedian.Text = hResult.HMedian.ToString("F2");

            var hCoreText = string.Join("\n", hResult.HCoreDetails.Select(x => 
                $"{x.PaperId} (Atıf: {x.InCitationCount})"));
            _txtHCore.Text = hCoreText;

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            _txtTotalNodes.Text = _viewGraph.TotalNodeCount.ToString();
            
            int citationEdgeCount = _viewGraph.GetAllEdges().Count(e => e.Type == EdgeType.Citation);
            _txtTotalEdges.Text = citationEdgeCount.ToString();

            var mostCited = _statsService.GetMostCitedNode();
            if (mostCited != null)
            {
                _txtMostCited.Text = $"{mostCited.Id}\n(Atıf: {mostCited.Paper.InCitationCount})";
            }

            var mostReferencing = _statsService.GetMostReferencingNode();
            if (mostReferencing != null)
            {
                int refCount = _fullGraph.GetOutDegree(mostReferencing.Id);
                _txtMostReferencing.Text = $"{mostReferencing.Id}\n(Referans: {refCount})";
            }
        }

        private void BtnCalculateHIndex_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string paperId = _txtHIndexInput.Text.Trim();
                if (string.IsNullOrWhiteSpace(paperId))
                {
                    MessageBox.Show("Lütfen bir makale ID'si girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var hResult = _hIndexCalculator.Calculate(paperId);

                _graphExpander.ExpandByHCore(_viewGraph, paperId, hResult.HCorePaperIds);
                _viewer.Graph = _graphController.BuildMsaglGraph(_viewGraph);

                OnGraphUpdated(hResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hesaplama Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCalculateBetweenness_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _txtBetweenness.Text = "Hesaplanıyor...";
                this.Cursor = System.Windows.Input.Cursors.Wait;

                var bc = new BetweennessCentrality(_viewGraph);
                var results = bc.CalculateForAllNodes();

                var top10 = results
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .Select(x => $"{x.Key}: {x.Value:F4}")
                    .ToList();

                _txtBetweenness.Text = string.Join("\n", top10);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hesaplama Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                _txtBetweenness.Text = "Hata!";
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void BtnApplyKCore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(_txtKValue.Text, out int k) || k < 0)
                {
                    MessageBox.Show("Lütfen geçerli bir k değeri girin (0 veya üstü).", "Uyarı", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var kCore = new KCoreDecomposition(_viewGraph);
                _currentKCoreResult = kCore.Decompose(k);

                _txtKCoreNodes.Text = _currentKCoreResult.NodeIds.Count.ToString();
                _txtKCoreEdges.Text = _currentKCoreResult.Edges.Count.ToString();

                _viewer.Graph = _graphController.BuildMsaglGraphWithKCore(_viewGraph, _currentKCoreResult);

                MessageBox.Show($"K-Core uygulandı!\nDüğüm: {_currentKCoreResult.NodeIds.Count}\nKenar: {_currentKCoreResult.Edges.Count}", 
                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "K-Core Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}