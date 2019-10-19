using System;
using System.Windows;
using MaterialDesignThemes.Wpf;
using UI.ViewModels;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
//            Snackbar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(3000));
//            ((MainWindowViewModel) DataContext).MessageQueue = Snackbar.MessageQueue;
        }
    }
}