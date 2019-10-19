using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.ViewModels
{
    public class MainWindowViewModel
    {
        /// <summary>
        /// Command to switch to bottom view page
        /// </summary>
        public ICommand SwitchBottomViewCommand { get; set; }

        /// <summary>
        /// Command to switch to top view page
        /// </summary>
        public ICommand SwitchTopViewCommand { get; set; }
        

        public MainWindowViewModel()
        {
            
        }
        
    }
}