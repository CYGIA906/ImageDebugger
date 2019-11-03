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

        public List<PointMarker> PointMarkers { get; set; }
        public HObject Edges { get; set; }

        public HObject CrossedUsed { get; set; }
        
        public List<HImage> Images { get; set; }


        public void AddLineRegion(HObject lineRegion)
        {
            LineRegions = HalconHelper.ConcatAll(LineRegions, lineRegion);
        }

 

        public void AddFindLineRects(HObject rect)
        {
            FindLineRects = HalconHelper.ConcatAll(FindLineRects, rect);
        }
        
        public void AddEdges(HObject edges)
        {
            Edges = HalconHelper.ConcatAll(Edges, edges);
        }


        private void DisplayPointMarkers(HWindow windowHandle)
        {
            if (PointMarkers == null || PointMarkers.Count == 0) return; 
            HObject crosses = new HObject();
            var offset = 5;
            foreach (var pointMarker in PointMarkers)
            {
                HObject cross;
                HOperatorSet.GenCrossContourXld(out cross, pointMarker.ImageY, pointMarker.ImageX, 10, 0.5);
                crosses = HalconHelper.ConcatAll(crosses, cross);
                
//                windowHandle.DispText($"{pointMarker.Name}{Environment.NewLine}{pointMarker.Height.ToString("f3")}", "image", pointMarker.ImageY + offset, pointMarker.ImageX + offset, "red", "border_radius", 2);
            }
            
            windowHandle.DispObj(crosses);
            
            
        }


        public void Display(HWindow windowHandle)
        {
            
            windowHandle.SetColor("magenta");
            LineRegions?.DispObj(windowHandle);
            
       
            windowHandle.SetColor("green");
            CrossedUsed?.DispObj(windowHandle);
            
            windowHandle.SetDraw("margin");
            windowHandle.SetColor("orange");
            FindLineRects?.DispObj(windowHandle);
            
            windowHandle.SetColor("medium slate blue");
            windowHandle.SetLineWidth(3);
            Edges?.DispObj(windowHandle);
            windowHandle.SetLineWidth(1);
            
            windowHandle.SetColor("red");
            Line.DisplayGraphics(windowHandle);

            windowHandle.SetColor("firebrick");
            DisplayPointMarkers(windowHandle);
        }
        
    }
}