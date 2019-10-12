using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml;
using Accord;
using HalconDotNet;

namespace UI.ImageProcessing
{
    public class PointsRecorder
    {
        private HTuple _changeOfBaseInv;

        private static HDevelopExport HalconScripts = new HDevelopExport();

        public static Point Offset { get; set; } = new Point(10, 0);

        private Dictionary<string, Point> _points = new Dictionary<string, Point>();

        public PointsRecorder(HTuple changeOfBaseInv)
        {
            _changeOfBaseInv = changeOfBaseInv;
        }

        /// <summary>
        /// Record a point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="name"></param>
        public void Record(Point point, string name)
        {
            AssignCoordinatePoint(point);
            _points[name] = point;
        }

        /// <summary>
        /// Record a point from intersecting lines
        /// </summary>
        /// <param name="lineA"></param>
        /// <param name="lineB"></param>
        /// <param name="name"></param>
        public void Record(Line lineA, Line lineB, string name)
        {
            var point = lineA.Intersect(lineB);
            Record(point, name);
        }

        private void AssignCoordinatePoint(Point point)
        {
            HTuple xOut, yOut;
            HOperatorSet.AffineTransPoint2d(_changeOfBaseInv, point.ImageX, point.ImageY, out xOut, out yOut);
            point.CoordinateX = xOut;
            point.CoordinateY = yOut;
        }

        public void DisplayPoints(HWindow windowHandle)
        {
            foreach (var pair in _points)
            {
                var point = pair.Value;
                windowHandle.DispText($"({point.CoordinateX.ToString("f2")}, {point.CoordinateY.ToString("f3")})", "image", point.ImageY + Offset.ImageY, point.ImageX + Offset.ImageX, "red", "border_radius", 2);
            }
        }


        public void Serialize(string path)
        {
            var headerNames = new List<string>();
            foreach (var pair in _points)
            {
                var xName = pair.Key + "_X";
                var yName = pair.Key + "_Y";
                headerNames.Add(xName);
                headerNames.Add(yName);
            }

            var header = string.Join(",", headerNames);

            var valueStrings = new List<string>();

            foreach (var pair in _points)
            {
                var xValue = pair.Value.CoordinateX.ToString("f3");
                var yValue = pair.Value.CoordinateY.ToString("f3");
                valueStrings.Add(xValue);
                valueStrings.Add(yValue);
            }

            var line = string.Join(",", valueStrings);

            var fileExists = File.Exists(path);
            var lineToWrite = fileExists ? line : header + Environment.NewLine + line;
            using (var fs = new StreamWriter(path, fileExists))
            {
                fs.WriteLine(lineToWrite);
            }
        }
    }


}