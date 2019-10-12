using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using HalconDotNet;

namespace UI.ImageProcessing
{
    public class Line
    {
        public double XStart { get; set; }

        public double YStart { get; set; }

        public double XEnd { get; set; }

        public double YEnd { get; set; }

        public static int ImageWidth { get; set; } = 5120;

        public static int ImageHeight { get; set; } = 5120;

        private static List<Line>  LineToDisplay = new List<Line>();

        private static HDevelopExport HalconScripts = new HDevelopExport();

        public Line(double xStart, double yStart, double xEnd, double yEnd, bool display = false)
        {
            XStart = xStart;
            YStart = yStart;
            XEnd = xEnd;
            YEnd = yEnd;

            IsVisible = display;
        }

        /// <summary>
        /// Compute the angle between two lines
        /// </summary>
        /// <param name="line"></param>
        /// <returns>angle in radian uint</returns>
        public double AngleWithLine(Line line)
        {
            HTuple angle;
            HOperatorSet.AngleLl(YStart, XStart, YEnd, XEnd, line.YStart, line.XStart, line.YEnd, line.XEnd, out angle);
            return angle;
        }
        
        public static void DisplayGraphics(HWindow windowHandle)
        {
            windowHandle.SetColor(LineColor);
            HObject lineRegions = new HObject();
            lineRegions.GenEmptyObj();

            foreach (var line in LineToDisplay)
            {
                HObject lineRegion;
                HalconScripts.GenLineRegion(out lineRegion, line.XStart, line.YStart, line.XEnd, line.YEnd, ImageWidth, ImageHeight);
                HOperatorSet.ConcatObj(lineRegions, lineRegion, out lineRegions);
            }
            lineRegions.DispObj(windowHandle);
            
            LineToDisplay.Clear();
        }

        public static string LineColor { get; set; } = "cyan";

        public Point Intersect(Line line)
        {
            HTuple x, y, _;
            HOperatorSet.IntersectionLines(line.YStart, line.XStart, line.YEnd, line.XEnd, YStart, XStart, YEnd, XEnd, out y, out x, out _);
            return new Point(x.D, y.D);
        }

        public double Angle
        {
            get
            {
                HTuple xUnit, yUnit, degree;
                HalconScripts.GetLineUnitVector(XStart, YStart, XEnd, YEnd, out xUnit, out yUnit);
                HalconScripts.GetVectorDegree(xUnit, yUnit, out degree);
                return degree.D;
            }
        }

        public static double Epslon { get; set; } = 0.001;

        public bool IsVertical
        {
            get { return Math.Abs(Angle) - 90 < Epslon; }
        }

        public bool IsVisible
        {
            get
            {
                return LineToDisplay.Contains(this);
            }
            set
            {
                if (!value) return;
                if (LineToDisplay.Contains(this)) return;
                LineToDisplay.Add(this);
            }
        }

        public Line PerpendicularLineThatPasses(Point point)
        {
            HTuple intersectX, intersectY;
            HalconScripts.get_perpendicular_line_that_passes(XStart, YStart, XEnd, YEnd, point.ImageX, point.ImageY, out intersectX, out intersectY);
            
            return new Line(point.ImageX, point.ImageY, intersectX.D, intersectY.D);
        }

        /// <summary>
        /// Project point on this line
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Point ProjectPoint(Point point)
        {
            HTuple intersectX, intersectY;
            HalconScripts.get_perpendicular_line_that_passes(XStart, YStart, XEnd, YEnd, point.ImageX, point.ImageY, out intersectX, out intersectY);

            return Intersect(new Line(intersectX, intersectY, point.ImageX, point.ImageY));
        }
    }
}  