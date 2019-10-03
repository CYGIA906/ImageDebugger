using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views
{
    public class PageBase<ViewModelType> : Page
        where ViewModelType : ViewModelBase, new()
    {
        private ViewModelType _viewModel;

        public ViewModelType ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                DataContext = value;
            }
        }

        /// <summary>
        /// Constructor that set DataContext
        /// </summary>
        public PageBase()
        {
            ViewModel = new ViewModelType();
        }
    }
}