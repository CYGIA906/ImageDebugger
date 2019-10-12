using System.Collections.Generic;
using System.Security.Cryptography;
using HalconDotNet;
using UI.ImageProcessing;
using UI.ImageProcessing.Utilts;

namespace UI.Model
{
    public class HalconGraphics
    {
        public HObject CrossesUsed { get; set; }
        public HObject CrossesIgnored { get; set; }
        public List<Line> PointLineGraphics { get; set; }
        public List<Line> PointPointGraphics { get; set; }
        public HObject FindLineRects { get; set; }
        public HObject LineRegions { get; set; }

        public HImage Image { get; set; }

        
        private static readonly HDevelopExport HalconScripts = new HDevelopExport();

        public void DisplayGraphics(HWindow windowHandle)
        {

            
            windowHandle.DispImage(Image);
            DisplayCrosses(windowHandle);
            DisplayPointLineDistanceGraphics(windowHandle);
            DisplayPointPointDistanceGraphics(windowHandle);
            DisplayEdges(windowHandle);
            Line.DisplayGraphics(windowHandle);
        }

        private void DisplayEdges(HWindow windowHandle)
        {
            windowHandle.SetLineWidth(5);
            windowHandle.SetColor("orange");
            Edges.DispObj(windowHandle);
            windowHandle.SetLineWidth(1);
        }


        public int ImageHeight { get; set; } = 5120;

        public int ImageWidth { get; set; } = 5120;


        private void DisplayPointLineDistanceGraphics(HWindow windowHandle)
        {
            windowHandle.SetColor("yellow");
            windowHandle.SetLineWidth(3);
            foreach (var line in PointLineGraphics)
            {
                windowHandle.DispArrow(line.YStart, line.XStart, line.YEnd, line.XEnd, ArrowSize);
            }
            windowHandle.SetLineWidth(1);

        }
        private void DisplayPointPointDistanceGraphics(HWindow windowHandle)
        {
            windowHandle.SetColor("orange");
            windowHandle.SetDraw("fill");

            HObject draw = new HObject();
            draw.GenEmptyObj();
            foreach (var line in PointPointGraphics)
            {
                HObject circle1, circle2, lineSeg;
                HOperatorSet.GenCircle(out circle1, line.YStart, line.XStart, EndPointRadius);
                HOperatorSet.GenCircle(out circle2, line.YEnd, line.XEnd, EndPointRadius);
                HOperatorSet.GenRegionLine(out lineSeg, line.YStart, line.XStart, line.YEnd, line.XEnd);
                draw = HalconHelper.ConcatAll(draw, circle1, circle2, lineSeg);
            }

            windowHandle.DispObj(draw);
            windowHandle.SetDraw("margin");
        }

        public int EndPointRadius { get; set; } = 10;

        public double ArrowSize { get; set; } = 10;
        public HObject Edges { get; set; }

        private void DisplayCrosses(HWindow windowHandle)
        {
            windowHandle.SetDraw("margin");
            windowHandle.SetLineWidth(1);
            windowHandle.SetColor("green");
            CrossesUsed.DispObj(windowHandle);
            

            windowHandle.SetColor("red");
            CrossesIgnored.DispObj(windowHandle);

            windowHandle.SetColor("magenta");
            windowHandle.SetLineWidth(3);
            FindLineRects.DispObj(windowHandle);

            
            windowHandle.SetColor("blue");
            LineRegions.DispObj(windowHandle);
        }
    }
}