using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HalconDotNet;
using MaterialDesignThemes.Wpf;
using UI.ImageProcessing.Utilts;
using UI.Model;
using UI.ViewModels;

namespace UI.ImageProcessing
{
    public partial class I94BottomViewMeasurement : IMeasurementProcedure
    {
        public event Action MeasurementResultReady;
        public string Name { get; }
        public event Action MeasurementResultPulled;

        private readonly HDevelopExport HalconScripts = new HDevelopExport();
        private HTuple _shapeModelHandle;

        public async Task<ImageProcessingResult> ProcessAsync(List<HImage> images, FindLineConfigs findLineConfigs, ObservableCollection<FaiItem> faiItems, int indexToShow,
            SnackbarMessageQueue messageQueue)
        {

            HObject imageUndistorted;
            HTuple changeOfBase;
            HTuple changeOfBaseInv;
            HTuple rotationMat;
            HTuple rotationMatInv;
            HTuple mapToWorld;
            HTuple mapToImage;
            HTuple xLeft, yLeft, xRight, yRight, xUp, yUp, xDown, yDown;
            HTuple baseLeftCol,
                baseLeftLen1,
                baseLeftLen2,
                baseLeftRadian,
                baseLeftRow,
                baseTopCol,
                baseTopLen1,
                baseTopLen2,
                baseTopRadian,
                baseTopRow,
                camParams;


            // Calculate matrices
            var image = images[0];
            HalconScripts.GetI94BottomViewBaseRects(image, out imageUndistorted, _shapeModelHandle, out baseTopRow,
                    out baseTopCol, out baseTopRadian, out baseTopLen1, out baseTopLen2, out baseLeftRow,
                    out baseLeftCol, out baseLeftRadian, out baseLeftLen1, out baseLeftLen2, out mapToWorld, out mapToImage, out camParams
                );
            images[0] = imageUndistorted.HobjectToHimage();
        

            var findLineManager = new FindLineManager(messageQueue);

            // Top base
            var findLineParamTop = findLineConfigs.FindLineParamsDict["TopBase"];
            var findLineFeedingsTop = findLineParamTop.ToFindLineFeeding();
            findLineFeedingsTop.Col = baseTopCol;
            findLineFeedingsTop.Row = baseTopRow;
            findLineFeedingsTop.Radian = baseTopRadian;
            findLineFeedingsTop.Len1 = baseTopLen1;
            findLineFeedingsTop.Len2 = baseTopLen2;
            findLineFeedingsTop.Transition = "negative";
            var lineTopBase = findLineManager.TryFindLine("X-aixs", image, findLineFeedingsTop);
            HalconScripts.SortLineLeftRight(lineTopBase.XStart, lineTopBase.YStart, lineTopBase.XEnd, lineTopBase.YEnd, out xLeft, out yLeft, out xRight, out yRight);

            // Right base
            var findLineParamRight = findLineConfigs.FindLineParamsDict["LeftBase"];
            var findLineFeedingsRight = findLineParamRight.ToFindLineFeeding();
            findLineFeedingsRight.Col = baseLeftCol;
            findLineFeedingsRight.Row = baseLeftRow;
            findLineFeedingsRight.Radian = baseLeftRadian;
            findLineFeedingsRight.Len1 = baseLeftLen1;
            findLineFeedingsRight.Len2 = baseLeftLen2;
            findLineFeedingsRight.Transition = "negative";
            var lineLeftBase = findLineManager.TryFindLine("Y-axis", image, findLineFeedingsRight);
            HalconScripts.SortLineUpDown(lineLeftBase.XStart, lineLeftBase.YStart, lineLeftBase.XEnd, lineLeftBase.YEnd, out xUp, out yUp, out xDown, out yDown);


            HalconScripts.GetChangeOfBase(xLeft, yLeft, xRight, yRight, xUp, yUp, xDown, yDown, out changeOfBase, out changeOfBaseInv, out rotationMat, out rotationMatInv);
            var coordinateSolver = new CoordinateSolver(changeOfBase, changeOfBaseInv, rotationMat, rotationMatInv, mapToWorld, mapToImage);

            // Update absolute find line locations
            findLineConfigs.GenerateLocationsAbs(coordinateSolver);
            // Find lines
            findLineManager.FindLineFeedings = findLineConfigs.GenerateFindLineFeedings();
            await findLineManager.FindLinesParallel(images);


            Dictionary<string, double> outputs = new Dictionary<string, double>();
            outputs["21_1"] = 0;
            outputs["21_2"] = 0;
            outputs["23_1"] = 0;
            outputs["23_2"] = 0;
            outputs["24_1"] = 0;
            outputs["25_1"] = 0;
            outputs["25_2"] = 0;
            outputs["26_1"] = 0;
            outputs["26_2"] = 0;
            outputs["27_1"] = 0;
            outputs["27_2"] = 0;
            outputs["28_1"] = 0;
            outputs["28_2"] = 0;
            outputs["29_1"] = 0;
            outputs["29_2"] = 0;
            outputs["31_1"] = 0;
            outputs["32_1"] = 0;
            outputs["33_1"] = 0;
            outputs["123_1"] = 0;
            outputs["123_2"] = 0;
            outputs["123_3"] = 0;
            var graphics = new HalconGraphics()
            {
                CrossesIgnored = findLineManager.CrossesIgnored,
                CrossesUsed = findLineManager.CrossesUsed,
                FindLineRects = findLineManager.FindLineRects,
                LineRegions = findLineManager.LineRegions,
                Edges = findLineManager.Edges,
//                PointPointGraphics = coordinateSolver.PointPointDistanceGraphics,
//                PointLineGraphics = coordinateSolver.PointLineDistanceGraphics,
                Image = image
            };

            return new ImageProcessingResult()
            {
                DataRecorder = new DataRecorder(changeOfBaseInv),
                FaiDictionary = outputs,
                HalconGraphics = graphics
            };
        }


        public I94BottomViewMeasurement(string name)
        {
            Name = name;

            HOperatorSet.ReadShapeModel("./backViewModel", out _shapeModelHandle);
        }
    }
}