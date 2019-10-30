using System.Windows;
using System.Windows.Controls;
using ImageDebugger.Core.ViewModels.Base;

namespace UI._3D.Views.LineScanMeasurement
{
    public partial class LineScanMeasurementView : UserControl
    {
        public LineScanMeasurementView()
        {
            InitializeComponent();
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as MeasurementPlayerViewModelBase;
            dataContext.WindowHandle = HalconWindow.HalconWindow;
        }
    }
}