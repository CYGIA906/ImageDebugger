using System;
using System.Collections.Generic;
using System.Windows;
using HalconDotNet;
using MathNet.Numerics.Random;

namespace ImageDebugger.Core.ImageProcessing
{
    public class Line
    {
        public double XStart { get; set; }

        public double YStart { get; set; }

        public double XEnd { get; set; }

        public double YEnd { get; set; }

        public static int ImageWidth { get; set; } = 5120;

        public bool IsDefaulConstructed { get; set; }

        public static int ImageHeight { get; set; } = 5120;

        private static List<Line> LineToDisplay { get; } = new List<Line>();

        private static HDevelopExport HalconScripts = new HDevelopExport();

        public Line(double xStart, double yStart, double xEnd, double yEnd, bool display = false)
        {
            XStart = xStart;
            YStart = yStart;
            XEnd = xEnd;
            YEnd = yEnd;

            IsVisible = display;
            IsDefaulConstructed = false;
        }

        public Line()
        {
            XStart = 1;
            YStart = 0;
            XEnd = 2;
            YEnd = 0;

            IsDefaulConstructed = true;
        }

        public Line(Point pt1, Point pt2, bool display = false) : this(pt1.ImageX, pt1.ImageY, pt2.ImageX, pt2.ImageY,
            display)
        {
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

        /// <summary>
        /// Return a line that point from up to down
        /// </summary>
        /// <returns></returns>
        public Line SortUpDown()
        {
            HTuple xUp, yUp, xDown, yDown;
            HalconScripts.SortLineUpDown(XStart, YStart, XEnd, YEnd, out xUp, out yUp, out xDown, out yDown);

            return new Line(xUp, yUp, xDown, yDown);
        }

        /// <summary>
        /// Return a line that points from left to right
        /// </summary>
        /// <returns></returns>
        public Line SortLeftRight()
        {
            HTuple xLeft, yLeft, xRight, yRight;
            HalconScripts.SortLineLeftRight(XStart, YStart, XEnd, YEnd, out xLeft, out yLeft, out xRight, out yRight);
            return new Line(xLeft, yLeft, xRight, yRight);
        }

        public static void DisplayGraphics(HWindow windowHandle)
        {
            HObject lineRegions = new HObject();
            lineRegions.GenEmptyObj();

            lock (LineToDisplay)
            {
                foreach (var line in LineToDisplay)
                {
                    HObject lineRegion;
                    try
                    {
                        HalconScripts.GenLineRegion(out lineRegion, line.XStart, line.YStart, line.XEnd, line.YEnd,
                            ImageWidth, ImageHeight);
                    }
                    catch (Exception e)
                    {
                        lineRegion = new HObject();
                        lineRegion.GenEmptyObj();
                    }

                    HOperatorSet.ConcatObj(lineRegions, lineRegion, out lineRegions);
                }

                lineRegions.DispObj(windowHandle);

                ClearLineGraphics();
            }
        }

        public static void ClearLineGraphics()
        {
            LineToDisplay.Clear();
        }

        public static string LineColor { get; set; } = "cyan";

        public Point Intersect(Line line)
        {
            HTuple x, y, _;
            HOperatorSet.IntersectionLines(line.YStart, line.XStart, line.YEnd, line.XEnd, YStart, XStart, YEnd, XEnd,
                out y, out x, out _);
            return new Point(x.D, y.D);
        }

        /// <summary>
        /// Angle in dgree
        /// </summary>
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
                lock (LineToDisplay)
                {
                    return LineToDisplay.Contains(this);
                }
            }
            set
            {
                lock (LineToDisplay)
                {
                    if (value)
                    {
                        if (LineToDisplay.Contains(this)) return;
                        LineToDisplay.Add(this);
                    }
                    else
                    {
                        if (LineToDisplay.Contains(this)) LineToDisplay.Remove(this);
                    }
                }
            }
        }

        public Point PointStart
        {
            get { return new Point(XStart, YStart); }
        }

        public Point PointEnd
        {
            get { return new Point(XEnd, YEnd); }
        }

        public Line PerpendicularLineThatPasses(Point point)
        {
            HTuple intersectX, intersectY;
            HalconScripts.get_perpendicular_line_that_passes(XStart, YStart, XEnd, YEnd, point.ImageX, point.ImageY,
                out intersectX, out intersectY);

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
            HalconScripts.get_perpendicular_line_that_passes(XStart, YStart, XEnd, YEnd, point.ImageX, point.ImageY,
                out intersectX, out intersectY);

            return Intersect(new Line(intersectX, intersectY, point.ImageX, point.ImageY));
        }

        public Line Translate(double distance, bool display = false)
        {
            if (IsVertical) SortUpDown();
            else SortLeftRight();
            HTuple x1, x2, y1, y2;
            HalconScripts.GetParallelLine(XStart, YStart, XEnd, YEnd, distance, out x1, out y1, out x2, out y2);

            return new Line(x1, y1, x2, y2, display);
        }

        public Line InvertDirection()
        {
            return new Line(XEnd, YEnd, XStart, YStart);
        }

        /// <summary>
        /// Get unit vector pointing from start to end
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Point GetUnitVector() 
        {
            double xDiff = XEnd - XStart;
            double yDiff = YEnd - YStart;
            double sinVal = yDiff / Len;
            double cosVal = xDiff / Len;
            
            return new Point(cosVal, sinVal);
        }

        public double Len
        {
            get
            {
                double xDiff = XEnd - XStart;
                double yDiff = YEnd - YStart;
                return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            }
        }
    }
}