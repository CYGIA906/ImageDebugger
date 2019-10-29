using System.IO;

namespace ImageDebugger.Core.ViewModels.Application
{
    public class ApplicationViewModel
    {
        private static ApplicationViewModel _instance = new ApplicationViewModel();

        public static string SolutionDirectory
        {
            get { return Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent?.Parent?.FullName; }
        }

        public static ApplicationViewModel Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }
    }
}