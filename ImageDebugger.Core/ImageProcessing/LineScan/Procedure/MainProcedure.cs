using System.Collections.Generic;
using HalconDotNet;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;
using MaterialDesignThemes.Wpf;

namespace ImageDebugger.Core.ImageProcessing.LineScan.Procedure
{
    public partial class I40LineScanMeasurement
    {
        public ImageProcessingResults3D Process(List<HImage> images, List<PointSettingViewModel> pointSettings,
            ISnackbarMessageQueue messageQueue)
        {
            var bottomImage = images[0].MirrorImage("column");
            var leftImage = images[1];
            var rightImage = images[2];
            HTuple imageWidth, imageHeight;
            rightImage.GetImageSize(out imageWidth, out imageHeight);

            #region Right View    

            HTuple realTimeModelHandle, rowV, colV, radianV, len1V, len2V, rowH, colH, radianH, len1H, len2H, rowRightModel, colRightModel, radianRightModel;
            _halconScripts.I40GetBaseRectsAndCreateNewModel(rightImage, _shapeModelHandleRight, out realTimeModelHandle, out rowV,
                out colV, out radianV, out len1V, out len2V, out rowH, out colH, out radianH, out len1H, out len2H,
                out rowRightModel, out colRightModel, out radianRightModel);

            var findLineFeedingH = _findLineParamH.ToFindLineFeeding();
            findLineFeedingH.Row = rowH;
            findLineFeedingH.Col = colH;
            findLineFeedingH.Radian = radianH;
            findLineFeedingH.Len1 = len1H;
            findLineFeedingH.Len2 = len2H;
            findLineFeedingH.Transition = "negative";
            findLineFeedingH.IsVertical = "true";

            var findLineFeedingV = _findLineParamV.ToFindLineFeeding();
            findLineFeedingV.Row = rowV;
            findLineFeedingV.Col = colV;
            findLineFeedingV.Radian = radianV;
            findLineFeedingV.Len1 = len1V;
            findLineFeedingV.Len2 = len2V;
            findLineFeedingV.Transition = "negative";
            findLineFeedingV.IsVertical = "false";


            var findLineManager = new FindLineManager(messageQueue);
            var lineHRight = findLineManager.TryFindLine("lineHRight", rightImage, findLineFeedingH);
            lineHRight.IsVisible = true;
            var lineVRight = findLineManager.TryFindLine("lineVRight", rightImage, findLineFeedingV);
            lineVRight.IsVisible = true;

            // Translate base
            lineHRight = lineHRight.SortUpDown();
            lineVRight  = lineVRight.SortLeftRight();
            
            var alignPointRight = lineHRight.Intersect(lineVRight);

            var xAxis = lineHRight.Translate(-1.0 / _horizontalCoeff * 6.788);
            xAxis.IsVisible = true;
            var yAxis = lineVRight.Translate(-1.0 / _verticalCoeff * 19.605);
            yAxis.IsVisible = true;

            #endregion


            #region Left View

            var leftImageMirrored = leftImage.MirrorImage("column");
            _halconScripts.I40GetBaseRects(leftImageMirrored, realTimeModelHandle, out rowV,
                out colV,
                out radianV, out len1V, out len2V, out rowH, out colH, out radianH, out len1H, out len2H);
            
     

            findLineFeedingH.Row = rowH;
            findLineFeedingH.Col = colH;

            findLineFeedingV.Row = rowV;
            findLineFeedingV.Col = colV;


            var lineHLeft= findLineManager.TryFindLine("lineHLeft", leftImageMirrored, findLineFeedingH, false);
//            lineHLeft.IsVisible = true;
            var lineVLeft= findLineManager.TryFindLine("lineVLeft", leftImageMirrored, findLineFeedingV, false);
//            lineVLeft.IsVisible = true;

            lineHLeft= lineHLeft.SortUpDown();
            lineVLeft= lineVLeft.SortLeftRight();

            var alignPointLeft = lineHLeft.Intersect(lineVLeft);
//            var rotationLeft = MathUtils.ToRadian(lineVLeft.Angle)
            
//             Align left view to right view
            HTuple mapFromLeftToRight;
            HOperatorSet.VectorAngleToRigid(alignPointLeft.ImageY, alignPointLeft.ImageX, 0, alignPointRight.ImageY,
                alignPointRight.ImageX, 0, out mapFromLeftToRight);
            var leftImageAligned =
                leftImageMirrored.AffineTransImage(new HHomMat2D(mapFromLeftToRight), "constant", "false");

            #endregion

            #region Bottom View

            HTuple rowBottom, colBottom, angleBottom, scoreBottom, mapBottomToRight;
            bottomImage.FindShapeModel(new HShapeModel(realTimeModelHandle.H), -0.3, 0.6, 0.2, 1,0.5,"least_squares", 0, 0.9, out rowBottom, out colBottom, out angleBottom, out scoreBottom);
            HOperatorSet.VectorAngleToRigid(rowBottom, colBottom, angleBottom, rowRightModel, colRightModel, radianRightModel, out mapBottomToRight);
            var bottomImageAligned = bottomImage.AffineTransImage(new HHomMat2D(mapBottomToRight), "constant", "false");
            
            
            #endregion
            

            //Debug: Compose image
            HImage emptyImage = new HImage();
            emptyImage.GenImageConst("byte", imageWidth, imageHeight);
           var imageLeftRight =  emptyImage.Compose3(rightImage.ScaleImageMax(), leftImageAligned.ScaleImageMax());
           var imageBottomRight = emptyImage.Compose3(rightImage.ScaleImageMax(), bottomImageAligned.ScaleImageMax());

           var bottomImageHeight = ToKeyenceHeightImage(bottomImageAligned);
           var leftImageHeight = ToKeyenceHeightImage(leftImageAligned);
           var rightImageHeight = ToKeyenceHeightImage(rightImage);


           var pointLocator = new PointLocator(xAxis.InvertDirection(), yAxis, _verticalCoeff, _horizontalCoeff);
            var pointMarkers = pointLocator.LocatePoints(pointSettings, new List<HImage>()
            {
                bottomImageHeight, leftImageHeight, rightImageHeight
            });


            var output = new ImageProcessingResults3D()
            {
                Images = new List<HImage>()
                {
                    rightImageHeight, imageLeftRight, imageBottomRight
                },
                CrossedUsed = findLineManager.GenCrossesUsed(),
                PointMarkers = pointMarkers
            };

            var rects = findLineManager.FindLineRects;
            output.AddLineRegion(findLineManager.LineRegions);
            output.AddFindLineRects(rects);
            output.AddEdges(findLineManager.Edges);

            return output;
        }
    }
}