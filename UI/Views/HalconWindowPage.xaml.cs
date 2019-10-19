using System;
using System.Windows;
using System.Windows.Controls;
using HalconDotNet;
using MaterialDesignThemes.Wpf;
using UI.ImageProcessing;
using UI.ImageProcessing.BottomView;
using UI.ImageProcessing.TopView;
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
            SnackBar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(5000));
            
            InjectViewModel(windowHandle, SnackBar.MessageQueue, new I94TopViewMeasure());
        }

        private void InjectViewModel(HWindow windowHandle, SnackbarMessageQueue messageQueue, IMeasurementProcedure measurementProcedure )
        {
            // TODO: make this elegant
            Line.ClearLineGraphics();
            var dataContext = new HalconWindowPageViewModel(windowHandle, measurementProcedure);
            dataContext.RunStatusMessageQueue = messageQueue;
            DataContext = dataContext;
        }

        private void ChangeToTopView(object sender, RoutedEventArgs e)
        {
            InjectViewModel(HalconWindow.HalconWindow, SnackBar.MessageQueue, new I94TopViewMeasure());
            SnackBar.MessageQueue.Enqueue("Switched to top view");
        }

        private void ChangeToBottomView(object sender, RoutedEventArgs e)
        {
            InjectViewModel(HalconWindow.HalconWindow, SnackBar.MessageQueue, new I94BottomViewMeasure());
            SnackBar.MessageQueue.Enqueue("Switched to bottom view");

        }
    }
}