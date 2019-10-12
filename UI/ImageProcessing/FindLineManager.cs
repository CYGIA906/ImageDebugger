using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using HalconDotNet;
using MathNet.Numerics.LinearRegression;
using UI.ImageProcessing.Utilts;

namespace UI.ImageProcessing
{
    //Conventions:
    // 1. All the someNumber-someLetter with the same someNumber will come to a single line result, whose name is someNumber
    // 2. All the someNumber.someLetter with the same someNumber will result in individual lines, whose name is someNumber.someLetter

    /// <summary>
    /// 
    /// </summary>
    public class FindLineManager
    {
        private List<HImage> _images;

        public HObject CrossesUsed
        {
            get { return _crossesUsed; }
            set { _crossesUsed = value; }
        }

        public HObject CrossesIgnored
        {
            get { return _crossesIgnored; }
            set { _crossesIgnored = value; }
        }

        public HObject LineRegions
        {
            get { return _lineRegions; }
            set { _lineRegions = value; }
        }

        public HObject FindLineRects
        {
            get { return _findLineRects; }
            set { _findLineRects = value; }
        }

      


        private Dictionary<string, Line> _lines = new Dictionary<string, Line>();

        private int _width = 5120;
        private int _height = 5120;
        private HObject _crossesIgnored = new HObject();
        private HObject _crossesUsed = new HObject();
        private HObject _findLineRects = new HObject();
        private HObject _lineRegions = new HObject();

        private static HDevelopExport HalconScripts = new HDevelopExport();
        private HObject _edges = new HObject();


        public void FindLines(List<HImage> images)
        {
            _images = images;

            DispatchFindLineWorkers();
        }

        private void DispatchFindLineWorkers()
        {
            foreach (var pair in FindLineFeedings)
            {
                var name = pair.Key;
                var feeding = pair.Value;
                var image = _images[feeding.ImageIndex];
                Line line;
                try
                {
                    line = FindLine(image, feeding);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Find line failed at {name}\nMessage:{e}");
                    MessageBox.Show($"Line {name} not found!");
                    line = new Line(1, 0, 2, 0);
                }

                _lines[name] = line;
            }
        }

        public Dictionary<string, FindLineFeeding> FindLineFeedings { get; set; }

        private Line FindLine(HImage image, FindLineFeeding feeding)
        {
            HObject lineRegion, findLineRegion;
            HTuple xsUsed = new HTuple();
            HTuple ysUsed = new HTuple();
            HTuple xsIgnored = new HTuple();
            HTuple ysIgnored = new HTuple();
            HTuple lineX1 = new HTuple();
            HTuple lineY1 = new HTuple();
            HTuple lineX2 = new HTuple();
            HTuple lineY2 = new HTuple();
            List<double> ys, xs;


            // using pair
            HObject edges = new HObject();
            edges.GenEmptyObj();
            if (feeding.UsingPair)
            {
                if (feeding.FirstAttemptOnly)
                {
                    HalconScripts.FindLineGradiant_Pair(image, out findLineRegion, out lineRegion, feeding.Row,
                        feeding.Col, feeding.Radian, feeding.Len1, feeding.Len2, feeding.NumSubRects,
                        feeding.IgnoreFraction, feeding.Transition, feeding.Threshold, feeding.Sigma1,
                        feeding.WhichEdge, feeding.WhichPair, feeding.MinWidth, feeding.MaxWidth, out xsUsed,
                        out ysUsed, out xsIgnored, out ysIgnored, out lineX1, out lineY1, out lineX2, out lineY2);
                }
                else
                {
                    HalconScripts.VisionProStyleFindLineOneStep_Pairs(image, out findLineRegion, out lineRegion,
                        feeding.Row, feeding.Col, feeding.Radian, feeding.Len1, feeding.Len2, feeding.Transition,
                        feeding.NumSubRects, feeding.Threshold, feeding.Sigma1, feeding.Sigma2, feeding.WhichEdge,
                        feeding.IsVertical, feeding.IgnoreFraction, feeding.WhichPair, feeding.MinWidth,
                        feeding.MaxWidth, _width, _height, feeding.CannyHigh, feeding.CannyLow, "true",
                        feeding.NewWidth, out xsUsed, out ysUsed, out xsIgnored, out ysIgnored, out lineX1, out lineY1,
                        out lineX2, out lineY2);

                   
                }
                xs = xsUsed.DArr.ToList();
                ys = ysUsed.DArr.ToList();
            } // using single edge
            else
            {
                if (feeding.FirstAttemptOnly)
                {
                    HalconScripts.FindLineGradiant(image, out findLineRegion, out lineRegion, feeding.Row, feeding.Col,
                        feeding.Radian, feeding.Len1, feeding.Len2, feeding.NumSubRects, feeding.IgnoreFraction,
                        feeding.Transition, feeding.Threshold, feeding.Sigma1, feeding.WhichEdge, out xsUsed,
                        out ysUsed, out xsIgnored, out ysIgnored, out lineX1, out lineY1, out lineX2, out lineY2);
                    ys = ysUsed.DArr.ToList();
                    xs = xsUsed.DArr.ToList();
                }
                else

                {
//                    HalconScripts.VisionProStyleFindLineOneStep(image, out findLineRegion, out lineRegion,
//                        feeding.Transition, feeding.Row, feeding.Col, feeding.Radian, feeding.Len1, feeding.Len2,
//                        feeding.NumSubRects, feeding.Threshold, feeding.WhichEdge, feeding.IgnoreFraction,
//                        feeding.IsVertical, feeding.Sigma1, feeding.Sigma2, _width, _height, feeding.NewWidth,
//                        feeding.CannyHigh, feeding.CannyLow, out lineX1, out lineY1, out lineX2, out lineY2, out xsUsed,
//                        out ysUsed, out xsIgnored, out ysIgnored);

                    var xsys = HalconHelper.FindLineSubPixel(image, feeding.Row.DArr, feeding.Col.DArr,
                        feeding.Radian.DArr, feeding.Len1.DArr, feeding.Len2.DArr, feeding.Transition.S,
                        feeding.NumSubRects.I, feeding.Threshold.I, feeding.WhichEdge.S, feeding.IgnoreFraction.D,
                        feeding.CannyLow.I, feeding.CannyHigh.I, feeding.Sigma1.D, feeding.Sigma2.D, feeding.NewWidth.I,
                        out edges, out findLineRegion);

                    xs = xsys.Item1;
                    ys = xsys.Item2;


                }
                
            }

            var line = HalconHelper.leastSquareAdaptLine(xs, ys);
            HalconScripts.GenLineRegion(out lineRegion, line.XStart, line.YStart, line.XEnd, line.YEnd, _width, _height);
            lineX1 = line.XStart;
            lineY1 = line.YStart;
            lineX2 = line.XEnd;
            lineY2 = line.YEnd;

            // Generate debugging graphics 
            HObject crossesUsed;
            HOperatorSet.GenCrossContourXld(out crossesUsed, ysUsed, xsUsed, CrossSize, CrossAngle);
            HObject crossesIgnored;
            HOperatorSet.GenCrossContourXld(out crossesIgnored, ysIgnored, xsIgnored, CrossSize, CrossAngle);

            HOperatorSet.ConcatObj(_crossesUsed, crossesUsed, out _crossesUsed);
            HOperatorSet.ConcatObj(_crossesIgnored, crossesIgnored, out _crossesIgnored);
            HOperatorSet.ConcatObj(_findLineRects, findLineRegion, out _findLineRects);
            HOperatorSet.ConcatObj(_lineRegions, lineRegion, out _lineRegions);
            HOperatorSet.ConcatObj(Edges, edges, out _edges);


            return new Line(lineX1.D, lineY1.D, lineX2.D, lineY2.D);
        }

        private void FitLineRegression(double[] xs, double[] ys, out HTuple lineX1, out HTuple lineY1, out HTuple lineX2, out HTuple lineY2)
        {
           var lineResult = SimpleRegression.Fit(xs, ys);
           var bias = lineResult.Item1;
           var weight = lineResult.Item2;
           HalconScripts.ImageLineIntersections(weight, bias, _width, _height, out lineX1, out lineY1, out lineX2, out lineY2);
        }


        public Line GetLine(string lineName)
        {
            return _lines[lineName];
        }

        public FindLineManager(Dictionary<string, FindLineFeeding> findLineFeedings)
        {
            FindLineFeedings = findLineFeedings;
            CrossesIgnored.GenEmptyObj();
            CrossesUsed.GenEmptyObj();
            LineRegions.GenEmptyObj();
            FindLineRects.GenEmptyObj();
            Edges.GenEmptyObj();
        }


        public int CrossSize { get; set; } = 100;

        public double CrossAngle { get; set; } = 0.8;

        public HObject Edges
        {
            get { return _edges; }
            set { _edges = value; }
        }
    }
}