using System.Collections.Generic;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public interface ILineScanMeasurementProcedure
    {
        IEnumerable<string> PointNames { get;  }
        
        string Name { get; set; }
    }
}