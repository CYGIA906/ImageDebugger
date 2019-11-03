using ImageDebugger.Core.ViewModels.LineScan;

namespace ImageDebugger.Core.Models
{
    public class AngleItem : ICsvColumnElement
    {
        public string Name { get; set; }
        public string CsvName => "Angle_" + Name;
        public double Value { get; set; }
    }
}