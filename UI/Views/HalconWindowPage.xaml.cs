using System;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
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
            windowHandle.SetColored(3);
            windowHandle.SetPart(0, 0, -2, -2);
            var dataContext = new HalconWindowPageViewModel(windowHandle);
            SnackBar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(5000));
            dataContext.RunStatusMessageQueue = SnackBar.MessageQueue;
            DataContext = dataContext;

        }
    }
}