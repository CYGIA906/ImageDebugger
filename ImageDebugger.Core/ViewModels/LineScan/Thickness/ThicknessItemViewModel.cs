using ImageDebugger.Core.ViewModels.Base;

namespace ImageDebugger.Core.ViewModels.LineScan.Thickness
{
    public class ThicknessItemViewModel : ViewModelBase, ICsvColumnElement
    {
        public string Name { get; set; }

        public string CsvName => "Thickness " + Name; 
        public double Value { get; set; }
    }
}