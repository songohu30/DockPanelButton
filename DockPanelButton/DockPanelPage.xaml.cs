using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;

namespace DockPanelButton
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DockPanelPage : Page, IDockablePaneProvider
    {
        public DockPanelPage()
        {
            InitializeComponent();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this as FrameworkElement;
            data.InitialState = new DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Floating;
            data.InitialState.SetFloatingRectangle(new Autodesk.Revit.DB.Rectangle(100, 100, 300, 500));
        }
    }
}
