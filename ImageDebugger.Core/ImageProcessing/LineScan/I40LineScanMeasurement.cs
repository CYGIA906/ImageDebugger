using System.Collections.Generic;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public class I40LineScanMeasurement : ILineScanMeasurementProcedure
    {
        public IEnumerable<string> PointNames { get; }

        public I40LineScanMeasurement()
        {
            PointNames = GenPointNames();
        }

        private IEnumerable<string> GenPointNames()
        {
            var output = new List<string>()
            {
                "16.3-1", "16.3-2", "16.3-3", "16.3-4", "16.3-5", "16.3-6", "16.3-7", "16.3-8",
                "16.5-1", "16.5-2", "16.5-3", "16.5-4", "16.5-5", "16.5-6", "16.5-7", "16.5-8",
                "17.1-1", "17.1-2",
                "17.2-1", "17.2-2",
                "17.3-1", "17.3-2",
                "17.4-1", "17.4-2",
                "19-F1", "19-F2", "19-F3", "19-F4", "19-F5", "19-F6", "19-F7", "19-F8", "19-F9", "19-F10", "19-F11",
                "19-F12", "19-F13", "19-F14",
                "19-A1", "19-A2", "19-A3", "19-A4", "19-A5", "19-A6", "19-A7", "19-A8"
            };

            return output;
        }
    }
}