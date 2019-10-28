using System.Windows;
using System.Windows.Controls;
using ImageDebugger.Core.ViewModels.CameraMeasurement;

namespace UI.Views
{
    public partial class CameraMeasurementView : UserControl
    {

        
        public CameraMeasurementView()
        {
            InitializeComponent();
        } 


        private void HalconWindowPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var windowHandle = HalconWindow.HalconWindow;
            windowHandle.SetColored(3);
            windowHandle.SetPart(0, 0, -2, -2);

            ((CameraMeasurementViewModel)DataContext).WindowHandle = windowHandle;
//            HalconWindow.HMouseMove += HalconWindowOnHMouseMove;
        }

//        private void HalconWindowOnHMouseMove(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
//        {
//            throw new System.NotImplementedException();
//        }
    }
}