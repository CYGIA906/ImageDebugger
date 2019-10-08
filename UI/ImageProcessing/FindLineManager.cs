using System;
using System.Collections.Generic;
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
        private FindLineConfigs _findLineConfigs;

        private List<HImage> _images;

        private HObject _crossesUsed;
        private HObject _crossesIgnored;

        private List<HObject> _lineRegions = new List<HObject>();

        private List<HObject> _findLineRects = new List<HObject>();

        private Dictionary<string, Line> _lines = new Dictionary<string, Line>();

        private int _width = 5120;
        private int _height = 5120;

        private static HDevelopExport HalconScripts = new HDevelopExport();

  

        public void FindLines(List<HImage> images)
        {
            _images = images;

            DispatchFindLineWorkers();
        }

        private void DispatchFindLineWorkers()
        {
            foreach (var pair in _findLineConfigs.FindLineFeedings)
            {
                var name = pair.Key;
                var feeding = pair.Value;
                var image = _images[feeding.ImageIndex];
//                try
//                {
                 var line = FindLine(image, feeding);
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine($"Find line failed at {name}\nMessage:{e}");
//                    throw;
//                }

                _lines[name] = line;
            }
        }

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
            }// using single edge
            else
            {
                if (feeding.FirstAttemptOnly)
                {
                    HalconScripts.FindLineGradiant(image, out findLineRegion, out lineRegion, feeding.Row, feeding.Col,
                        feeding.Radian, feeding.Len1, feeding.Len2, feeding.NumSubRects, feeding.IgnoreFraction,
                        feeding.Transition, feeding.Threshold, feeding.Sigma1, feeding.WhichEdge, out xsUsed,
                        out ysUsed, out xsIgnored, out ysIgnored, out lineX1, out lineY1, out lineX2, out lineY2);

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
            HOperatorSet.GenCrossContourXld(out _crossesUsed, ysUsed, xsUsed, CrossSize, CrossAngle);
            HOperatorSet.GenCrossContourXld(out _crossesIgnored, ysIgnored, xsIgnored, CrossSize, CrossAngle);
            _findLineRects.Add(findLineRegion);
            _lineRegions.Add(lineRegion);

            return new Line(lineX1.D, lineY1.D, lineX2.D, lineY2.D);
        }


        public Line GetLine(string lineName)
        {
            return _lines[lineName];
        }

        public FindLineManager(FindLineConfigs findLineConfigs)
        {
            _findLineConfigs = findLineConfigs;
        }

        public void DisplayGraphics(HWindow windowHandle)
        {
            windowHandle.SetDraw("margin");
            windowHandle.SetLineWidth(1);
            windowHandle.SetColor("green");
            _crossesUsed.DispObj(windowHandle);

            windowHandle.SetColor("red");
            _crossesIgnored.DispObj(windowHandle);

            windowHandle.SetColor("magenta");
            windowHandle.SetLineWidth(3);
            foreach (var rect in _findLineRects)
            {
                rect.DispObj(windowHandle);
            }

            windowHandle.SetColor("blue");
            foreach (var lineRegion in _lineRegions)
            {
                lineRegion.DispObj(windowHandle);
            }

            _findLineRects.Clear();
            _lineRegions.Clear();
        }


        public int CrossSize { get; set; } = 100;

        public double CrossAngle { get; set; } = 0.8;
    }
}