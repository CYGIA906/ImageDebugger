using System.Windows;
using ImageDebugger.Core.IoC;
using ImageDebugger.Core.IoC.Interface;

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
            IoC.Kernel.Bind<IImageProvider>().ToConstant(new ImageProvider());
            IoC.Kernel.Bind<ISerializationManager>().ToConstant(new SerializationManager());
            IoC.Setup();
            
            // Open main window
            var window = new MainWindow();
            Current.MainWindow = window;
            window.Show();
        }
    }
}