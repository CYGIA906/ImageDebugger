using System.Collections.Generic;
using HalconDotNet;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public class PointLocator
    {
        public List<PointMarker> LocatePoints(List<PointSettingViewModel> pointSettings, List<HImage> images)
        {
            var output = new List<PointMarker>();
            foreach (var pointSetting in pointSettings)
            {
                PointMarker pointMarker = LocatePoint(pointSetting, images);
                output.Add(pointMarker);
            }

            return output;
        }

        private PointMarker LocatePoint(PointSettingViewModel pointSetting, List<HImage> images)
        {
            var lineX = YAxis.Translate(pointSetting.X / XCoeff);
            var lineY = XAxis.Translate(pointSetting.Y / YCoeff);
            var intersection = lineX.Intersect(lineY);
            var image = images[pointSetting.ImageIndex];
            return new PointMarker()
            {
                ImageX = intersection.ImageX,
                ImageY = intersection.ImageY,
                Height = image.GetGrayval(intersection.ImageY, intersection.ImageX),
                Name = pointSetting.Name
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