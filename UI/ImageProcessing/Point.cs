using System.Collections.Generic;
using HalconDotNet;

namespace UI.ImageProcessing
{
    public class Point
    {
        public static List<HObject> GraphicPoints { get; set; } = new List<HObject>();

        public static int PointSize { get; set; } = 100;

        public static double PointAngle { get; set; } = 0.8;

        public double ImageX { get; set; }

        public double ImageY { get; set; }

        public double CoordinateX { get; set; }

        public double CoordinateY { get; set; }

        public void Display(HWindow windowHandle)
        {
            foreach (var graphicPoint in GraphicPoints)
            {
                graphicPoint.DispObj(windowHandle);
            }

            GraphicPoints.Clear();
        }

        public Point(double imageX, double imageY, bool display = false)
        {
            ImageX = imageX;
            ImageY = imageY;
            if (display)
            {
                HObject cross;
                HOperatorSet.GenCrossContourXld(out cross, imageY, imageX, PointSize, PointAngle);
                GraphicPoints.Add(cross);
            }
        }
    }
}