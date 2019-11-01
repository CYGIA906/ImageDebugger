using ImageDebugger.Core.ViewModels.Base;

namespace ImageDebugger.Core.ViewModels.LineScan.Flatness
{
    public class FlatnessItemViewModel : ViewModelBase
    {
        public string Name { get; set; }

        public double Value { get; set; }
    }
}