using ImageDebugger.Core.ViewModels.Base;

namespace ImageDebugger.Core.ViewModels.LineScan.Flatness
{
    public class FlatnessItemViewModel : ViewModelBase, ICsvColumnElement
    {
        public string Name { get; set; }

        public string CsvName => "Flatness " + Name;
        public double Value { get; set; }
    }
}