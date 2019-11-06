using System.Collections.Generic;
using System.Reflection;
using HalconDotNet;
using ImageDebugger.Core.Models;
using ImageDebugger.Core.ViewModels.Application;

namespace ImageDebugger.Core.ImageProcessing.LineScan.Procedure
{
    public partial class I40LineScanMeasurement : ILineScanMeasurementProcedure
    {
        /// <summary>
        /// The name of the points to be found
        /// </summary>
        public IEnumerable<string> PointNames { get; }
        public string Name { get; set; } = "I40";
        public int NumImageRequireInSingleRun { get; set; } = 3;
        
        private HDevelopExport _halconScripts = new HDevelopExport();
    

        private HTuple _shapeModelHandleRight;

        public string ShapeModelPath
        {
            get { return ApplicationViewModel.SolutionDirectory + "/Configs/3D/I40/ShapeModels/ModelRight"; }
        }

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
                "18-E1", "18-E2", "18-E3", "18-E4", "18-E5", "18-E6", "18-E7", "18-E8", "18-E9", "18-E10", "18-E11", "18-E12", "18-E13", "18-E14", "18-E15", "18-E16",
                "19-F1", "19-F2", "19-F3", "19-F4", "19-F5", "19-F6", "19-F7", "19-F8", "19-F9", "19-F10", "19-F11", "19-F12", "19-F13", "19-F14",
                "19-A1", "19-A2", "19-A3", "19-A4", "19-A5", "19-A6", "19-A7", "19-A8",
                "20.1-F", "20.2-F", "20.3-F", "20.4-F",  
                "20.1-P", "20.2-P", "20.3-P", "20.4-P",
                "22-P1", "22-P2", "22-P3", "22-P4"
            };

            return output;
        }
        
        
        private FindLineParam _findLineParamH = new FindLineParam()
        {
            CannyHigh = 200, CannyLow = 100, Threshold = 100, ErrorThreshold = 0.5
        };
        
        
        private FindLineParam _findLineParamV = new FindLineParam()
        {
            CannyHigh = 200, CannyLow = 100, Threshold = 500, ErrorThreshold = 0.2, NewWidth = 2, NumSubRects = 30, Sigma1 = 4
        };


        private HImage VisualizeAlignment(HImage image1, HImage image2)
        {
            HTuple width, height;
            image1.GetImageSize(out width, out height);
            var emptyImage = new HImage();
            emptyImage.GenImageConst("byte", width, height);
            return emptyImage.Compose3(image1.ScaleImageMax().ScaleImage(0.5,0), image2.ScaleImageMax().ScaleImage(0.5, 0));
        }
        
 

        private readonly double _yCoeff = 0.01248;

        private readonly double _xCoeff = 0.0398;

        private HTuple GenMapToWorld()
        {
            var arr = new double[]
            {
                _yCoeff, 0, 0,
                0, _xCoeff, 0,
                0, 0, 1
            };
            return new HTuple(arr);
        }
        
        private HTuple GenMapToImage()
        {
            var arr = new double[]
            {
                1.0/_yCoeff, 0, 0,
                0, 1.0/_xCoeff, 0,
                0, 0, 1
            };
            return new HTuple(arr);
        }
    }
}