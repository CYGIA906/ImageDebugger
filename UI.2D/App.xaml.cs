using System.Windows;
using ImageDebugger.Core.IoC;
using ImageDebugger.Core.IoC.Interface;

namespace UI._2D
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Set up IoC
        /// </summary>
        /// <param name="e"></param>
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