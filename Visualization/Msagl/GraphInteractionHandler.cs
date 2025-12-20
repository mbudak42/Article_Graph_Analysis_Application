using System;
using Article_Graph_Analysis_Application.Core;
using Article_Graph_Analysis_Application.Core.HIndex;
using Microsoft.Msagl.WpfGraphControl;

using DomainGraph = Article_Graph_Analysis_Application.Core.Graph;
using MsaglNode = Microsoft.Msagl.Drawing.Node;

namespace Article_Graph_Analysis_Application.Visualization.Msagl
{
    public class GraphInteractionHandler
    {
        private readonly DomainGraph _fullGraph;
        private readonly DomainGraph _viewGraph;
        private readonly HIndexCalculator _hIndexCalculator;
        private readonly GraphExpander _graphExpander;
        private readonly MsaglGraphController _graphController;
        private readonly GraphViewer _viewer;
        private readonly Action<HIndexResult> _onGraphUpdated;

        public GraphInteractionHandler(
            DomainGraph fullGraph,
            DomainGraph viewGraph,
            HIndexCalculator hIndexCalculator,
            GraphExpander graphExpander,
            MsaglGraphController graphController,
            GraphViewer viewer,
            Action<HIndexResult> onGraphUpdated)
        {
            _fullGraph = fullGraph;
            _viewGraph = viewGraph;
            _hIndexCalculator = hIndexCalculator;
            _graphExpander = graphExpander;
            _graphController = graphController;
            _viewer = viewer;
            _onGraphUpdated = onGraphUpdated;

            _viewer.MouseDown += OnMouseDown;
        }

        private void OnMouseDown(object? sender, EventArgs e)
        {
            var me = e as dynamic;

            if (me?.Entity is MsaglNode msNode)
            {
                string clickedId = msNode.Id;

                var hResult = _hIndexCalculator.Calculate(clickedId);

                _graphExpander.ExpandByHCore(_viewGraph, clickedId, hResult.HCorePaperIds);

                _viewer.Graph = _graphController.BuildMsaglGraph(_viewGraph);

                _onGraphUpdated?.Invoke(hResult);
            }
        }
    }
}