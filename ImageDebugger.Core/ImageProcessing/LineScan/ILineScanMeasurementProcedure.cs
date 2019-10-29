using System.Collections.Generic;
using HalconDotNet;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public interface ILineScanMeasurementProcedure
    {
        IEnumerable<string> PointNames { get;  }
        
        string Name { get; set; }
        
        int NumImageRequireInSingleRun { get; set; }

        ImageProcessingResults3D Process(List<HImage> images);
    }
}