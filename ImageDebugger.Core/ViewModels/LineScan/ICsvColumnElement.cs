namespace ImageDebugger.Core.ViewModels.LineScan
{
    public interface ICsvColumnElement
    {
         string CsvName { get; }
         
         double Value { get; set; }
    }
}