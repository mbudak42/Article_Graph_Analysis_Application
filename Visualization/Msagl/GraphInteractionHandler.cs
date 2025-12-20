using Microsoft.Msagl.Drawing;
using System.Windows;

namespace Article_Graph_Analysis_Application.Visualization.Msagl
{
    public class GraphInteractionHandler
    {
        public event Action<string>? NodeClicked;

        public void AttachToViewer(Microsoft.Msagl.WpfGraphControl.GraphViewer viewer)
        {
            viewer.MouseDown += (sender, args) =>
            {
                var objectUnderCursor = viewer.ObjectUnderMouseCursor;
                if (objectUnderCursor?.DrawingObject is Node node)
                {
                    NodeClicked?.Invoke(node.Id);
                }
            };
        }
    }
}