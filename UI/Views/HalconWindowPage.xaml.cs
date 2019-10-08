using System.Windows;
using System.Windows.Controls;
using UI.ViewModels;

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
            DataContext = new HalconWindowPageViewModel(windowHandle);
 
        }
    }
}