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
    public class PonitsRecoder
    {
        private HTuple _changeOfBaseInv;

        private static HDevelopExport HalconScripts = new HDevelopExport();

        public static double Offset { get; set; } = 10;

        public TextAlignment Alignment { get; set; } = TextAlignment.Right;

        private Dictionary<string, Point> _points = new Dictionary<string, Point>();

        public PonitsRecoder(HTuple changeOfBaseInv)
        {
            _changeOfBaseInv = changeOfBaseInv;
        }

        public void Record(Point point, string name)
        {
            AssignCoordinatePoint(point);
            _points[name] = point;
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
                windowHandle.DispText($"({point.CoordinateX}, {point.CoordinateY})", "image", point.ImageY, point.ImageX, "red", "corner_radius", 2);
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

    public enum TextAlignment
    {
        Left, Right, Top, Bottom, Center
    }
}