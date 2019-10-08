using System.Collections.Generic;
using HalconDotNet;

namespace UI.ImageProcessing
{
    public class Point
    {
        public static List<HObject> GraphicPoints { get; set; } = new List<HObject>();

        public static int PointSize { get; set; } = 100;

        public static double PointAngle { get; set; } = 0.8;

        public double X { get; set; }

        public double Y { get; set; }


        public void Display(HWindow windowHandle)
        {
            foreach (var graphicPoint in GraphicPoints)
            {
                graphicPoint.DispObj(windowHandle);
            }

            GraphicPoints.Clear();
        }

        public Point(double x, double y, bool display = false)
        {
            X = x;
            Y = y;
            if (display)
            {
                HObject cross;
                HOperatorSet.GenCrossContourXld(out cross, y, x, PointSize, PointAngle);
                GraphicPoints.Add(cross);
            }
        }
    }
}