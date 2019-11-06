using System;
using System.Collections.Generic;
using System.Diagnostics;
using HalconDotNet;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public class PointLocator
    {
        
        private static HDevelopExport _halconScripts = new HDevelopExport();

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
            double grayValue = 0;
            try
            {
                grayValue =  (double) image.GetGrayval((int)intersection.ImageY, (int)intersection.ImageX) / 1000;
            }
            catch (Exception e)
            {
                image.WriteImage("tiff", 0, "image.tif");
                Debugger.Break();
            }
            return new PointMarker()
            {
                ImageX = intersection.ImageX,
                ImageY = intersection.ImageY,
//                Height = pointSetting.KernelSize < 2? (double)image.GetGrayval(intersection.ImageY, intersection.ImageX) : SmoothAndGetValueAtPoint(intersection.ImageX, intersection.ImageY, image, pointSetting.KernelSize, pointSetting.TrimPercent),
                Height = grayValue,
                Name = pointSetting.Name
            };
        }

        private double SmoothAndGetValueAtPoint(double x, double y, HImage image, int kernelSize = 3, double trimPercent = 0.2)
        {
            HTuple grayValue;
            _halconScripts.FilterAndGetPointValue(image, kernelSize, (int)x, (int)y, trimPercent, out grayValue);
            return grayValue;
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