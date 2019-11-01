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
                "19-F1", "19-F2", "19-F3", "19-F4", "19-F5", "19-F6", "19-F7", "19-F8", "19-F9", "19-F10", "19-F11",
                "19-F12", "19-F13", "19-F14",
                "19-A1", "19-A2", "19-A3", "19-A4", "19-A5", "19-A6", "19-A7", "19-A8"
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

        private readonly double _horizontalCoeff = 0.01248;

        private readonly double _verticalCoeff = 0.0497;

        private HTuple GenMapToWorld()
        {
            var arr = new double[]
            {
                _horizontalCoeff, 0, 0,
                0, _verticalCoeff, 0,
                0, 0, 1
            };
            return new HTuple(arr);
        }
        
        private HTuple GenMapToImage()
        {
            var arr = new double[]
            {
                1.0/_horizontalCoeff, 0, 0,
                0, 1.0/_verticalCoeff, 0,
                0, 0, 1
            };
            return new HTuple(arr);
        }
    }
}