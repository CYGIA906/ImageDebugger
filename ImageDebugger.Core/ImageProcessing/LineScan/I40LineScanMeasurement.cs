using System.Collections.Generic;
using System.IO;
using HalconDotNet;
using ImageDebugger.Core.Models;
using ImageDebugger.Core.ViewModels.Application;
using MaterialDesignThemes.Wpf;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public class I40LineScanMeasurement : ILineScanMeasurementProcedure
    {
        /// <summary>
        /// The name of the points to be found
        /// </summary>
        public IEnumerable<string> PointNames { get; }
        public string Name { get; set; } = "I40";
        public int NumImageRequireInSingleRun { get; set; } = 1;
        
        private HDevelopExport _halconScripts = new HDevelopExport();
        public ImageProcessingResults3D Process(List<HImage> images, ISnackbarMessageQueue messageQueue)
        {
            var image = images[0];

           HTuple realTimeModelHandle, rowV, colV, radianV, len1V, len2V, rowH, colH, radianH, len1H, len2H;
           _halconScripts.I40GetBaseRects(image, _shapeModelHandleRight, out realTimeModelHandle, out rowV, out colV,
               out radianV, out len1V, out len2V, out rowH, out colH, out radianH, out len1H, out len2H);

            var heightImage = ToKeyenceHeightImage(image);
            

           var findLineFeedingH = _findLineParamH.ToFindLineFeeding();
           findLineFeedingH.Row = rowH;
           findLineFeedingH.Col = colH;
           findLineFeedingH.Radian = radianH;
           findLineFeedingH.Len1 = len1H;
           findLineFeedingH.Len2 = len2H;
           findLineFeedingH.Transition = "negative";
           
           var findLineFeedingV = _findLineParamV.ToFindLineFeeding();
           findLineFeedingV.Row = rowV;
           findLineFeedingV.Col = colV;
           findLineFeedingV.Radian = radianV;
           findLineFeedingV.Len1 = len1V;
           findLineFeedingV.Len2 = len2V;
           findLineFeedingV.Transition = "negative";

           var findLineManager = new FindLineManager(messageQueue);
           var lineH = findLineManager.TryFindLine("lineH", heightImage, findLineFeedingH);
           lineH.IsVisible = true;
           var lineV = findLineManager.TryFindLine("lineV", heightImage, findLineFeedingV);
           lineV.IsVisible = true;
           
           
           
           
          var output = new ImageProcessingResults3D()
          {
              Image = heightImage
          };

          var rects = findLineManager.FindLineRects;
          int count = rects.CountObj();
          output.AddLineRegion(findLineManager.LineRegions);
          output.AddFindLineRects(rects);
          output.AddEdges(findLineManager.Edges);
          findLineManager.CrossesUsed

          return output;

        }

        private HTuple _shapeModelHandleRight;

        public string ShapeModelPath =>
            ApplicationViewModel.SolutionDirectory + "/Configs/3D/I40/ShapeModels/ModelRight";

        public I40LineScanMeasurement()
        {
            PointNames = GenPointNames();
            
            HOperatorSet.ReadShapeModel(ShapeModelPath, out _shapeModelHandleRight);
        }

        private IEnumerable<string> GenPointNames()
        {
            var output = new List<string>()
            {
                "16.3-1", "16.3-2", "16.3-3", "16.3-4", "16.3-5", "16.3-6", "16.3-7", "16.3-8",
                "16.5-1", "16.5-2", "16.5-3", "16.5-4", "16.5-5", "16.5-6", "16.5-7", "16.5-8",
                "17.1-1", "17.1-2",
                "17.2-1", "17.2-2",
                "17.3-1", "17.3-2",
                "17.4-1", "17.4-2",
                "19-F1", "19-F2", "19-F3", "19-F4", "19-F5", "19-F6", "19-F7", "19-F8", "19-F9", "19-F10", "19-F11",
                "19-F12", "19-F13", "19-F14",
                "19-A1", "19-A2", "19-A3", "19-A4", "19-A5", "19-A6", "19-A7", "19-A8"
            };

            return output;
        }
        
        
        private FindLineParam _findLineParamH = new FindLineParam()
        {
            CannyHigh = 1, CannyLow = 0.5, Threshold = 0.5, ErrorThreshold = 0.5
        };
        
        
        private FindLineParam _findLineParamV = new FindLineParam()
        {
            CannyHigh = 2, CannyLow = 1, Threshold = 1, ErrorThreshold = 0.5, NewWidth = 2
        };


        /// <summary>
        /// Convert 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static HImage ToKeyenceHeightImage(HImage image)
        {
            image = image.ConvertImageType("real");
            image = image.ScaleImage(1.0, -32768.0);
            return image.ScaleImage(1.6 * 0.001, 0);
        }
    }
}