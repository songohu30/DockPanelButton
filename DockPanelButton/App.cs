using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DockPanelButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class App : IExternalApplication
    {
        RibbonItem _button_easy; //buttton that switchess on/off dock panel visiblity and changes its visibility state
        RibbonItem _button_medium; //button that does the same as above but it also uses external event to align with current dock panel state
        ExternalEvent _toggleEvent;

        //this will give you access to App instance and its methods
        internal static App _app = null;
        public static App Instance
        {
            get { return _app; }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            _app = this;

            //Path to dll file, pushbutton requires this path and :IExternalCommand class (I placed ShowHideDock : IExternalCommand on the bottom, it doesn't have to be here though:)
            string path = Assembly.GetExecutingAssembly().Location;

            //Create panel in Add-ins tab
            RibbonPanel ribPanel = application.CreateRibbonPanel("My Dock Panel");

            //Pushbutton to hide show dockpanel easy option
            PushButtonData pushButtonPanelControl1 = new PushButtonData("DockControlEasy", "Show Easy", path, "DockPanelButton.ShowHideDockEasy");
            pushButtonPanelControl1.LargeImage = GetEmbeddedImage("Resources.Inactive.png");
            _button_easy = ribPanel.AddItem(pushButtonPanelControl1);

            //Pushbutton to hide show dockpanel medium option
            PushButtonData pushButtonPanelControl2 = new PushButtonData("DockControlMedium", "Show Medium", path, "DockPanelButton.ShowHideDockMedium");
            //this one initial state image is assigned automatically according to dock panel visibility state
            _button_medium = ribPanel.AddItem(pushButtonPanelControl2);

            RegisterDockPanel(application);

            //create external event to change button state and text
            IExternalEventHandler exEventHandler = new ExternalEventShowHideDockMedium();
            _toggleEvent = ExternalEvent.Create(exEventHandler);

            //when user closes dock panel using "x", button state and text must be changed
            application.DockableFrameVisibilityChanged += OnDockableFrameVisibilityChanged;
            //when documents is loaded dockpanel may be visible or not therefore button state and text must be also aligned with current visibility
            application.ViewActivated += OnDockableFrameVisibilityChanged;

            return Result.Succeeded;
        }

        //we need this method to subscribe to events: ViewActivated and DockableFrameVisibilityChanged
        private void OnDockableFrameVisibilityChanged(object sender, EventArgs e)
        {
            //we can not simply use AlignButtonState() method because it requires UIApplication from valid revit context.
            _toggleEvent.Raise();
        }

        //toggles button image and text + switches on/off dock panel visibility
        public void SwitchButtonImage(UIApplication application)
        {
            if (_button_easy.ItemText == "Show Easy")
            {
                ShowDockableWindow(application);
                _button_easy.ItemText = "Hide Easy";
                PushButton pb = _button_easy as PushButton;
                pb.LargeImage = GetEmbeddedImage("Resources.Active.png");
            }
            else
            {
                HideDockableWindow(application);
                _button_easy.ItemText = "Show Easy";
                PushButton pb = _button_easy as PushButton;
                pb.LargeImage = GetEmbeddedImage("Resources.Inactive.png");
            }
        }

        //for the second medium button we only need to switch on and off dock panel visibility, button image and text are aligned by event
        public void SwitchDockPanelVisibility(UIApplication application)
        {
            if (_button_medium.ItemText == "Show Medium")
            {
                ShowDockableWindow(application);
            }
            else
            {
                HideDockableWindow(application);
            }
        }

        public void AlignButtonState(UIApplication application)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid("{C38746CB-C632-4C88-9556-4DAEDB1A6E97}"));

            //Avoids an error when the user cancels document loading
            if (DockablePane.PaneExists(dpid))
            {
                DockablePane dp = application.GetDockablePane(dpid);
                //we are just checking current dock panel state. If it is visible then button is active (green) and the text says "Hide" because it will hide the panel on click.
                //if the panel is not shown then image is inactive (grey) and text says "Show"
                if(dp.IsShown())
                {
                    _button_medium.ItemText = "Hide Medium";
                    PushButton pb = _button_medium as PushButton;
                    pb.LargeImage = GetEmbeddedImage("Resources.Active.png");
                }
                else
                {
                    _button_medium.ItemText = "Show Medium";
                    PushButton pb = _button_medium as PushButton;
                    pb.LargeImage = GetEmbeddedImage("Resources.Inactive.png");
                }
            }
        }

        private void ShowDockableWindow(UIApplication application)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid("{C38746CB-C632-4C88-9556-4DAEDB1A6E97}"));
            DockablePane dp = application.GetDockablePane(dpid);
            dp.Show();
        }

        private void HideDockableWindow(UIApplication application)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid("{C38746CB-C632-4C88-9556-4DAEDB1A6E97}"));
            DockablePane dp = application.GetDockablePane(dpid);
            dp.Hide();
        }

        private void RegisterDockPanel(UIControlledApplication app)
        {
            DockablePaneProviderData data = new DockablePaneProviderData();           
            DockPanelPage dockPanelPage = new DockPanelPage();
            DockablePaneId dpid = new DockablePaneId(new Guid("{C38746CB-C632-4C88-9556-4DAEDB1A6E97}"));
            app.RegisterDockablePane(dpid, "MyDockPanel", dockPanelPage as IDockablePaneProvider);
        }

        private BitmapSource GetEmbeddedImage(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(name);
            return BitmapFrame.Create(stream);
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowHideDockEasy : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            App.Instance.SwitchButtonImage(commandData.Application);
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowHideDockMedium : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            App.Instance.SwitchDockPanelVisibility(commandData.Application);
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Changes dock panel button state and text when user closes dock panel with "x". Aligns button state and text with dockpanel visibility on startup.
    /// </summary>
    class ExternalEventShowHideDockMedium : IExternalEventHandler
    {
        public void Execute(UIApplication uiapp)
        {
            App.Instance.AlignButtonState(uiapp);
        }
        public string GetName()
        {
            return "ShowHideDockMedium";
        }
    }
}
