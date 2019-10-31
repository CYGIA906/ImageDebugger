using System.Collections.Generic;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public class PointLocator
    {
        public List<PointMarker> LocatePoints(List<PointSettingViewModel> pointSettings)
        {
            var output = new List<PointMarker>();
            foreach (var pointSetting in pointSettings)
            {
                PointMarker pointMarker = LocatePoint(pointSetting);
                output.Add(pointMarker);
            }

            return output;
        }

        private PointMarker LocatePoint(PointSettingViewModel pointSetting)
        {
            var lineX = YAxis.Translate(pointSetting.X / XCoeff);
            var lineY = XAxis.Translate(pointSetting.Y / YCoeff);
            var intersection = lineX.Intersect(lineY);
            return new PointMarker()
            {
                ImageX = intersection.ImageX,
                ImageY = intersection.ImageY
            };
        }

        public PointLocator(Line xAxis, Line yAxis, double xCoeff, double yCoeff)
        {
            XAxis = xAxis;
            YAxis = yAxis;
            XCoeff = xCoeff;
            YCoeff = yCoeff;
        }

        public Line XAxis { get; set; }
        public Line YAxis { get; set; }
        public double XCoeff { get; set; }
        public double YCoeff { get; set; }
    }
}