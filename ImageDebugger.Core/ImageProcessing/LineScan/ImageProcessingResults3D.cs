using System;
using System.Collections.Generic;
using Accord.MachineLearning;
using HalconDotNet;
using ImageDebugger.Core.ImageProcessing.Utilts;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public class ImageProcessingResults3D
    {
        private HObject LineRegions { get; set; }


        private HObject FindLineRects { get; set; }

        private List<PointMarker> PointMarkers { get; set; }

        public HImage Image { get; set; }


        public void AddLineRegion(HObject lineRegion)
        {
            LineRegions = HalconHelper.ConcatAll(LineRegions, lineRegion);
        }

 

        public void AddFindLineRects(HObject rect)
        {
            FindLineRects = HalconHelper.ConcatAll(FindLineRects, rect);
        }

        private void DisplayPointMarkers(HWindow windowHandle)
        {
            if (PointMarkers == null || PointMarkers.Count == 0) return; 
            windowHandle.SetColor("blue");
            HObject crosses = new HObject();
            var offset = 5;
            foreach (var pointMarker in PointMarkers)
            {
                HObject cross;
                HOperatorSet.GenCrossContourXld(out cross, pointMarker.ImageY, pointMarker.ImageX, 10, 0.5);
                crosses = HalconHelper.ConcatAll(crosses, cross);
                
                windowHandle.DispText($"{pointMarker.Name}{Environment.NewLine}{pointMarker.Height.ToString("f3")}", "image", pointMarker.ImageY + offset, pointMarker.ImageX + offset, "red", "border_radius", 2);
            }
            
            windowHandle.DispObj(crosses);
            
            
        }


        public void Display(HWindow windowHandle)
        {
            Image?.DispImage(windowHandle);
            
            windowHandle.SetColor("magenta");
            LineRegions?.DispObj(windowHandle);
            
            windowHandle.SetColor("green");
            Line.DisplayGraphics(windowHandle);
            
            windowHandle.SetColor("orange");
            FindLineRects?.DispObj(windowHandle);
            
            windowHandle.SetColor("blue");
            DisplayPointMarkers(windowHandle);
        }
        
    }
}