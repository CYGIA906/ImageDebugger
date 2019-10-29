using System.Windows;
using ImageDebugger.Core.IoC;
using ImageDebugger.Core.IoC.Interface;
using UI._2D.DataAccess;
using UI._3D.Views;

namespace UI._3D
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set up IoC
            IoC.RegisterAsSingleton<IImageProvider, ImageProvider>();
            IoC.RegisterAsSingleton<ISerializationManager, SerializationManager>();
            IoC.Setup();
            
            // Open main window
            var window = new MainWindow();
            Current.MainWindow = window;
            window.Show();
        }
    }
}