using ImageDebugger.Core.ViewModels.Base;

namespace ImageDebugger.Core.ViewModels.LineScan.Parallelism
{
    public class ParallelismItemViewModel : ViewModelBase, ICsvColumnElement
    {
        public string Name { get; set; }

        public string CsvName
        {
            get { return "Parallelism_" + Name; }
        }

        public double Value { get; set; }
    }
}