using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views
{
    public partial class ImageProviderView : UserControl
    {
        public ImageProviderView()
        {
            InitializeComponent();
            DataContext = new ImageProviderViewModel();
        }
    }
}