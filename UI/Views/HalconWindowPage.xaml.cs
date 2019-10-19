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
    public partial class HalconWindowPage : PageBase<HalconWindowPageViewModel>
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
            
            ViewModel.WindowHandle = windowHandle;
        }
        
    }
}