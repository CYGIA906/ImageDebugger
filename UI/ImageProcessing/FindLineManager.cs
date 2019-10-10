using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using HalconDotNet;

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
            HTuple xsUsed;
            HTuple ysUsed;
            HTuple xsIgnored;
            HTuple ysIgnored;
            HTuple lineX1;
            HTuple lineY1;
            HTuple lineX2;
            HTuple lineY2;


            // using pair
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
            } // using single edge
            else
            {
                var findLineRectCount = feeding.Row.TupleLength();


                if (feeding.FirstAttemptOnly)
                {
                    HalconScripts.FindLineGradiant(image, out findLineRegion, out lineRegion, feeding.Row, feeding.Col,
                        feeding.Radian, feeding.Len1, feeding.Len2, feeding.NumSubRects, feeding.IgnoreFraction,
                        feeding.Transition, feeding.Threshold, feeding.Sigma1, feeding.WhichEdge, out xsUsed,
                        out ysUsed, out xsIgnored, out ysIgnored, out lineX1, out lineY1, out lineX2, out lineY2);
                }
                else if (findLineRectCount == 1) // Adaptive find line, can only accept one find line rect
                {
                    HObject lineContour;
                    HalconScripts.FindLineAdaptiveSingle(image, out findLineRegion, out lineRegion, out lineContour,
                        feeding.Row, feeding.Col, feeding.Radian, feeding.Len1, feeding.Len2, feeding.NumSubRects,
                        feeding.Transition, feeding.Threshold, feeding.Sigma1, feeding.Sigma2, feeding.WhichEdge, 0.7,
                        feeding.IgnoreFraction, feeding.NewWidth, feeding.CannyLow, feeding.CannyHigh, out lineX1,
                        out lineY1, out lineX2, out lineY2, out xsUsed, out ysUsed, out xsIgnored, out ysIgnored);
                    findLineRegion.ConcatObj(lineContour);
                }
                else

                {
                    HalconScripts.VisionProStyleFindLineOneStep(image, out findLineRegion, out lineRegion,
                        feeding.Transition, feeding.Row, feeding.Col, feeding.Radian, feeding.Len1, feeding.Len2,
                        feeding.NumSubRects, feeding.Threshold, feeding.WhichEdge, feeding.IgnoreFraction,
                        feeding.IsVertical, feeding.Sigma1, feeding.Sigma2, _width, _height, feeding.NewWidth,
                        feeding.CannyHigh, feeding.CannyLow, out lineX1, out lineY1, out lineX2, out lineY2, out xsUsed,
                        out ysUsed, out xsIgnored, out ysIgnored);
                }
            }


            // Generate debugging graphics 
            HObject crossesUsed;
            HOperatorSet.GenCrossContourXld(out crossesUsed, ysUsed, xsUsed, CrossSize, CrossAngle);
            HObject crossesIgnored;
            HOperatorSet.GenCrossContourXld(out crossesIgnored, ysIgnored, xsIgnored, CrossSize, CrossAngle);

            HOperatorSet.ConcatObj(_crossesUsed, crossesUsed, out _crossesUsed);
            HOperatorSet.ConcatObj(_crossesIgnored, crossesIgnored, out _crossesIgnored);
            HOperatorSet.ConcatObj(_findLineRects, findLineRegion, out _findLineRects);
            HOperatorSet.ConcatObj(_lineRegions, lineRegion, out _lineRegions);

            return new Line(lineX1.D, lineY1.D, lineX2.D, lineY2.D);
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
        }


        public int CrossSize { get; set; } = 100;

        public double CrossAngle { get; set; } = 0.8;
    }
}