using System.Windows;
using System.Windows.Controls;
using HalconWindowPageViewModel = ImageDebugger.Core.ViewModels.HalconWindowViewModel.HalconWindowPageViewModel;

namespace UI.Views
{
    public partial class HalconWindowPage : Page
    {

        
        public HalconWindowPage()
        {
            InitializeComponent();
        } 


        private void HalconWindowPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var windowHandle = HalconWindow.HalconWindow;
            windowHandle.SetColored(3);
            windowHandle.SetPart(0, 0, -2, -2);

            ((HalconWindowPageViewModel)DataContext).WindowHandle = windowHandle;
        }
        
    }
}