using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms.VisualStyles;
using HalconDotNet;
using UI.ViewModels;

namespace UI.ImageProcessing
{
    public class I94TopViewMeasure : IMeasurementProcedure
    {
        private static HDevelopExport _halconScripts = new HDevelopExport();
        private string _modelPath;
        private HTuple _shapeModelHandle;
        private CoordinateSolver _coordinateSolver;
        public ObservableCollection<FaiItem> FaiItems { get; }
        public void Process(List<HImage> images, FindLineConfigs findLineConfigs, HWindow windowHandle)
        {
            HObject imageUndistorted;
            HTuple changeOfBase;
            HTuple changeOfBaseInv;
            HTuple rotationMat;
            HTuple rotationMatInv;
            HTuple mapToWorld;
            HTuple mapToImage;
            HTuple xLeft, yLeft, xRight, yRight, xUp, yUp, xDown, yDown;
            // Calculate matrices

            images[0] = new HImage("C:/Users/afterbunny/Desktop/Transfer/Xiaojin/ModelImages/point_extraction_top_view.bmp");
            _halconScripts.I94TopViewChangeBase(images[0], out imageUndistorted, _shapeModelHandle, out changeOfBase,
                out changeOfBaseInv, out rotationMat, out rotationMatInv, out mapToWorld, out mapToImage, out xLeft,
                out yLeft, out xRight, out yRight, out xUp, out yUp, out xDown, out yDown);
            var xAxis = new Line(xLeft.D, yLeft.D, xRight.D, yRight.D, true);
            var yAxis = new Line(xUp.D, yUp.D, xDown.D, yDown.D, true);

            _coordinateSolver = new CoordinateSolver(changeOfBase, changeOfBaseInv, rotationMat, rotationMatInv, mapToWorld, mapToImage);
            // Update absolute find line locations
            findLineConfigs.Solver = _coordinateSolver;
            // Find lines
            var findLineManager = new FindLineManager(findLineConfigs);
            findLineManager.FindLines(images);
            
            // Make parallel lines

            windowHandle.SetPart(0,0,5120,5120);
            windowHandle.SetColored(3);
            imageUndistorted.DispObj(windowHandle);
            findLineManager.DisplayGraphics(windowHandle);
            Line.DisplayGraphics(windowHandle);
        }
        /// <summary>
        /// Path to the shape model in disk
        /// </summary>
        public string ModelPath
        {
            get { return _modelPath; }
            set
            {
                _modelPath = value;
                HOperatorSet.ReadShapeModel(_modelPath, out _shapeModelHandle);
            }
        }

        public I94TopViewMeasure()
        {
            ModelPath = "C:/Users/afterbunny/Desktop/Transfer/Xiaojin/Hdevs/ModelTopViewI94";
        }


    }
}