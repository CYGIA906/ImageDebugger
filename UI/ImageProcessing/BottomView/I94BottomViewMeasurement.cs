﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HalconDotNet;
using MaterialDesignThemes.Wpf;
using UI.ImageProcessing.Utilts;
using UI.Model;
using UI.ViewModels;

namespace UI.ImageProcessing.BottomView
{
    public partial class I94BottomViewMeasurement : IMeasurementProcedure
    {
        public event Action MeasurementResultReady;
        public string Name { get; } = "I94_BOTTOM";
        public event Action MeasurementResultPulled;

        private readonly HDevelopExport HalconScripts = new HDevelopExport();
        private HTuple _shapeModelHandle;

        public async Task<ImageProcessingResult> ProcessAsync(List<HImage> images, FindLineConfigs findLineConfigs,
            ObservableCollection<FaiItem> faiItems, int indexToShow,
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
                out baseLeftCol, out baseLeftRadian, out baseLeftLen1, out baseLeftLen2, out mapToWorld, out mapToImage,
                out camParams
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
            HalconScripts.SortLineLeftRight(lineTopBase.XStart, lineTopBase.YStart, lineTopBase.XEnd, lineTopBase.YEnd,
                out xLeft, out yLeft, out xRight, out yRight);

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
            HalconScripts.SortLineUpDown(lineLeftBase.XStart, lineLeftBase.YStart, lineLeftBase.XEnd, lineLeftBase.YEnd,
                out xUp, out yUp, out xDown, out yDown);


            HalconScripts.GetChangeOfBase(xLeft, yLeft, xRight, yRight, xUp, yUp, xDown, yDown, out changeOfBase,
                out changeOfBaseInv, out rotationMat, out rotationMatInv);
            var coordinateSolver = new CoordinateSolver(changeOfBase, changeOfBaseInv, rotationMat, rotationMatInv,
                mapToWorld, mapToImage);

            // Update absolute find line locations
            findLineConfigs.GenerateLocationsAbs(coordinateSolver);
            // Find lines
            findLineManager.FindLineFeedings = findLineConfigs.GenerateFindLineFeedings();
            await findLineManager.FindLinesParallel(images);

            // Offset lines
            var lineF21Top = coordinateSolver.TranslateLineInWorldUnit(21, lineTopBase, true);
            var lineF21Bottom = coordinateSolver.TranslateLineInWorldUnit(30, lineTopBase, true);

            var lineF23Left = coordinateSolver.TranslateLineInWorldUnit(-5.5, lineLeftBase, true);
            var lineF23Right = coordinateSolver.TranslateLineInWorldUnit(-13.2, lineLeftBase, true);

            var lineF24Top = coordinateSolver.TranslateLineInWorldUnit(7.3, lineTopBase, true);
            var lineF24Bottom = coordinateSolver.TranslateLineInWorldUnit(8.5, lineTopBase, true);


            var lineF26Left =
                coordinateSolver.TranslateLineInWorldUnit(-0.6, findLineManager.GetLine("24.left").SortUpDown(), true);
            var lineF26Right =
                coordinateSolver.TranslateLineInWorldUnit(0.6, findLineManager.GetLine("24.right").SortUpDown(), true);

            var lineF27Left = coordinateSolver.TranslateLineInWorldUnit(-23.5, lineLeftBase, true);
            var lineF27Right = coordinateSolver.TranslateLineInWorldUnit(-25, lineLeftBase, true);

            var lineF29Top = coordinateSolver.TranslateLineInWorldUnit(16, lineTopBase, true);
            var lineF29Bottom = coordinateSolver.TranslateLineInWorldUnit(17.2, lineTopBase, true);

            var lineF32Top = lineF21Top;
            var lineF32Bottom = coordinateSolver.TranslateLineInWorldUnit(30.1, lineTopBase, true);

            var lineF32Left = lineF23Left;
            var lineF32Right = lineF23Right;
            var lineF32IdealHorizontal = coordinateSolver.TranslateLineInWorldUnit(25.392, lineTopBase, true);
            var lineF32IdealVertical = coordinateSolver.TranslateLineInWorldUnit(-9.299, lineLeftBase, true);


            var lineF33Left = lineF26Left;
            var lineF33Right = lineF26Right;

            var lineF123Left = coordinateSolver.TranslateLineInWorldUnit(-5, lineLeftBase, true);
            var lineF123Center = coordinateSolver.TranslateLineInWorldUnit(-9.269, lineLeftBase, true);
            var lineF123Right = coordinateSolver.TranslateLineInWorldUnit(-14.5, lineLeftBase, true);

// line line intersections
            var p1F21 = findLineManager.GetLine("21left").Intersect(lineF21Top);
            var p2F21 = findLineManager.GetLine("21left").Intersect(lineF21Bottom);
            var p3F21 = findLineManager.GetLine("21right").Intersect(lineF21Top);
            var p4F21 = findLineManager.GetLine("21right").Intersect(lineF21Bottom);

            var p1F23 = findLineManager.GetLine("23top").Intersect(lineF23Left);
            var p2F23 = findLineManager.GetLine("23top").Intersect(lineF23Right);
            var p3F23 = findLineManager.GetLine("23bottom").Intersect(lineF23Left);
            var p4F23 = findLineManager.GetLine("23bottom").Intersect(lineF23Right);

            var p1F24 = findLineManager.GetLine("24.left").Intersect(lineF24Top);
            var p2F24 = findLineManager.GetLine("24.left").Intersect(lineF24Bottom);
            var p3F24 = findLineManager.GetLine("24.right").Intersect(lineF24Top);
            var p4F24 = findLineManager.GetLine("24.right").Intersect(lineF24Bottom);

            var pTopLeftF27 = findLineManager.GetLine("27.top").Intersect(lineF27Left);
            var pTopRightF27 = findLineManager.GetLine("27.top").Intersect(lineF27Right);
            var pBottomLeftF27 = findLineManager.GetLine("27.bottom").Intersect(lineF27Left);
            var pBottomRightF27 = findLineManager.GetLine("27.bottom").Intersect(lineF27Right);

            var pLeftTopF29 = findLineManager.GetLine("29.left").Intersect(lineF29Top);
            var pLeftBottomF29 = findLineManager.GetLine("29.left").Intersect(lineF29Bottom);
            var pRightTopF29 = findLineManager.GetLine("29.right").Intersect(lineF29Top);
            var pRightBottomF29 = findLineManager.GetLine("29.right").Intersect(lineF29Bottom);

            //TODO: finish fai31 intersection points

            var pTopLeftF32 = findLineManager.GetLine("23top").Intersect(lineF32Left);
            var pTopRightF32 = findLineManager.GetLine("23top").Intersect(lineF32Right);
            var pBottomLeftF32 = findLineManager.GetLine("23bottom").Intersect(lineF32Left);
            var pBottomRightF32 = findLineManager.GetLine("23bottom").Intersect(lineF32Right);
            var pLeftTopF32 = findLineManager.GetLine("21left").Intersect(lineF32Top);
            var pLeftBottomF32 = findLineManager.GetLine("21left").Intersect(lineF32Bottom);
            var pRightTopF32 = findLineManager.GetLine("21right").Intersect(lineF32Top);
            var pRightBottomF32 = findLineManager.GetLine("21right").Intersect(lineF32Bottom);

            var p1F123 = findLineManager.GetLine("123").Intersect(lineF123Left);
            var p2F123 = findLineManager.GetLine("123").Intersect(lineF123Center);
            var p3F123 = findLineManager.GetLine("123").Intersect(lineF123Right);

// points to measure
            var pLeftF23 = Point.CenterPointInImage(p1F23, p3F23);
            var pRightF23 = Point.CenterPointInImage(p2F23, p4F23);

            var pLeftF24 = Point.CenterPointInImage(p1F24, p2F24);
            var pRightF24 = Point.CenterPointInImage(p3F24, p4F24);

            var rectLeftTop = coordinateSolver.FindLineLocationRelativeToAbsolute(EdgeLocationsRelative["26-leftTop"]);
            var edgeLeftTop = GetContour(image, rectLeftTop);
            var pLeftTopF26 = LineContourIntersection(lineF26Left, edgeLeftTop);
            var rectLeftBottom =
                coordinateSolver.FindLineLocationRelativeToAbsolute(EdgeLocationsRelative["26-leftBottom"]);
            var edgeLeftBottom = GetContour(image, rectLeftBottom);
            var pLeftBottomF26 = LineContourIntersection(lineF26Left, edgeLeftBottom);
            var rectRightTop =
                coordinateSolver.FindLineLocationRelativeToAbsolute(EdgeLocationsRelative["26-rightTop"]);
            var edgeRightTop = GetContour(image, rectRightTop);
            var pRightTopF26 = LineContourIntersection(lineF26Right, edgeRightTop);
            var rectRightBottom =
                coordinateSolver.FindLineLocationRelativeToAbsolute(EdgeLocationsRelative["26-rightBottom"]);
            var edgeRightBottom = GetContour(image, rectRightBottom);
            var pRightBottomF26 = LineContourIntersection(lineF26Right, edgeRightBottom);

            var pF27Top = Point.CenterPointInImage(pTopLeftF27, pTopRightF27);
            var pF27Bottom = Point.CenterPointInImage(pBottomLeftF27, pBottomRightF27);
            
            var pF29Left = Point.CenterPointInImage(pLeftTopF29, pLeftBottomF29);
            var pF29Right = Point.CenterPointInImage(pRightTopF29, pRightBottomF29);

            var pF32Top = Point.CenterPointInImage(pTopLeftF32, pTopRightF32);
            var pF32Bottom = Point.CenterPointInImage(pBottomLeftF32, pBottomRightF32);
            var lineF32Vertical = new Line(pF32Top.ImageX, pF32Top.ImageY, pF32Bottom.ImageX, pF32Bottom.ImageY);

            var pF32Left = Point.CenterPointInImage(pLeftTopF32, pLeftBottomF32);
            var pF32Right = Point.CenterPointInImage(pRightTopF32, pRightBottomF32);
            var lineF32Horizontal = new Line(pF32Left.ImageX, pF32Left.ImageY, pF32Right.ImageX, pF32Right.ImageY);

            var pF32Ideal = lineF32IdealHorizontal.Intersect(lineF32IdealVertical);

            var pF32 = lineF32Horizontal.Intersect(lineF32Vertical);

            var rectCircleLeft =
                coordinateSolver.FindLineLocationRelativeToAbsolute(EdgeLocationsRelative["leftCircle"]);
            HTuple circleXLeft, circleYLeft, circleRadiusLeft;
            HObject leftCircleContour;
            HalconScripts.I94FindLeftCircle(image, out leftCircleContour, rectCircleLeft.Y, rectCircleLeft.X,
                MathUtils.ToRadian(rectCircleLeft.Angle), rectCircleLeft.Len1, rectCircleLeft.Len2, out circleXLeft,
                out circleYLeft, out circleRadiusLeft);

            var rectCircleRight =
                coordinateSolver.FindLineLocationRelativeToAbsolute(EdgeLocationsRelative["rightCircle"]);
            HTuple circleXRight, circleYRight, circleRadiusRight;
            HObject rightCircleContour;
            HalconScripts.I94FindRightCircle(image, out rightCircleContour, rectCircleRight.Y, rectCircleRight.X,
                MathUtils.ToRadian(rectCircleRight.Angle), rectCircleRight.Len1, rectCircleRight.Len2, out circleXRight,
                out circleYRight, out circleRadiusRight);

            // TODO: finish point-locating of fai 31 and 33

            // Measure
            var valueF21Top = coordinateSolver.PointPointDistanceInWorld(p1F21, p3F21, true);
            var valueF21Bottom = coordinateSolver.PointPointDistanceInWorld(p2F21, p4F21, true);

            var valueF23Left = coordinateSolver.PointPointDistanceInWorld(p1F23, p3F23, true);
            var valueF23Right = coordinateSolver.PointPointDistanceInWorld(p2F23, p4F23, true);

            var valueF24 = coordinateSolver.PointPointDistanceInWorld(pLeftF24, pRightF24, true);

            var valueF25_1 = circleRadiusLeft.D * Weight * 2;
            var leftCenter = new Point(circleXLeft, circleYLeft);
            var distYCircleLeft = coordinateSolver.PointLineDistanceInWorld(leftCenter, lineTopBase);
            var distXCircleLeft = coordinateSolver.PointLineDistanceInWorld(leftCenter, lineLeftBase);
            var valueF25_2 = 2.0 * Point.Distance(new Point(distXCircleLeft, distYCircleLeft), new Point(9.299, 7.886));

            var valueF26_1 = coordinateSolver.PointPointDistanceInWorld(pLeftTopF26, pLeftBottomF26, true);
            var valueF26_2 = coordinateSolver.PointPointDistanceInWorld(pRightTopF26, pRightBottomF26, true);
            
            var rightCenter = new Point(circleXRight, circleYRight);
            var value27_1 = coordinateSolver.PointPointDistanceInWorld(rightCenter, pF27Top, true);
            var value27_2 = coordinateSolver.PointPointDistanceInWorld(rightCenter, pF27Bottom, true);

            var valueF28_1 = circleRadiusRight.D * Weight * 2;
            var distYCircleRight = coordinateSolver.PointLineDistanceInWorld(rightCenter, lineTopBase);
            var distXCircleRight = coordinateSolver.PointLineDistanceInWorld(rightCenter, lineLeftBase);
            var valueF28_2 = 2.0 * Point.Distance(new Point(distXCircleRight, distYCircleRight), new Point(24.434, 16.624));    
            
            var valueF29_1 = coordinateSolver.PointPointDistanceInWorld(rightCenter, pF29Left, true);
            var valueF29_2 = coordinateSolver.PointPointDistanceInWorld(rightCenter, pF29Right, true);
            
            // TODO: measure fai31

            var valueF32 = 2 * coordinateSolver.PointPointDistanceInWorld(pF32Ideal, pF32);

            // TODO: measure fai33

            var valueF123_1 = coordinateSolver.PointLineDistanceInWorld(p1F123, lineTopBase);
            var valueF123_2 = coordinateSolver.PointLineDistanceInWorld(p2F123, lineTopBase);
            var valueF123_3 = coordinateSolver.PointLineDistanceInWorld(p3F123, lineTopBase);

            
            // output
            Dictionary<string, double> outputs = new Dictionary<string, double>();
            outputs["21_1"] = valueF21Top;
            outputs["21_2"] = valueF21Bottom;
            outputs["23_1"] = valueF23Left;
            outputs["23_2"] = valueF23Right;
            outputs["24_1"] = valueF24;
            outputs["25_1"] = valueF25_1;
            outputs["25_2"] = valueF25_2;
            outputs["26_1"] = valueF26_1;
            outputs["26_2"] = valueF26_2;
            outputs["27_1"] = value27_1;
            outputs["27_2"] = value27_2;
            outputs["28_1"] = valueF28_1;
            outputs["28_2"] = valueF28_2;
            outputs["29_1"] = valueF29_1;
            outputs["29_2"] = valueF29_2;
            outputs["31_1"] = 0;
            outputs["32_1"] = valueF32;
            outputs["33_1"] = 0;
            outputs["123_1"] = valueF123_1;
            outputs["123_2"] = valueF123_2;
            outputs["123_3"] = valueF123_3;
            
            var graphics = new HalconGraphics()
            {
                CrossesIgnored = findLineManager.CrossesIgnored,
                CrossesUsed = findLineManager.CrossesUsed,
                FindLineRects = findLineManager.FindLineRects,
                LineRegions = findLineManager.LineRegions,
                Edges = HalconHelper.ConcatAll(findLineManager.Edges, edgeLeftTop, edgeLeftBottom, edgeRightTop, edgeRightBottom, leftCircleContour, rightCircleContour),
                PointPointGraphics = coordinateSolver.PointPointDistanceGraphics,
                PointLineGraphics = coordinateSolver.PointLineDistanceGraphics,
                Image = image
            };

            return new ImageProcessingResult()
            {
                DataRecorder = new DataRecorder(changeOfBaseInv),
                FaiDictionary = outputs,
                HalconGraphics = graphics
            };
        }

        public double Weight { get; set; } = 0.0076;


        public I94BottomViewMeasurement()
        {
            HOperatorSet.ReadShapeModel("./backViewModel", out _shapeModelHandle);
        }

        /// <summary>
        /// Get the longest contour with in a region
        /// </summary>
        /// <param name="image"></param>
        /// <param name="location"></param>
        /// <param name="cannyLow"></param>
        /// <param name="cannyHigh"></param>
        /// <returns></returns>
        private HObject GetContour(HImage image, FindLineLocation location, int cannyLow = 20, int cannyHigh = 40)
        {
            HObject region;
            HOperatorSet.GenRectangle2(out region, location.Y, location.X, MathUtils.ToRadian(location.Angle),
                location.Len1, location.Len2);
            var imageEdge = image.ReduceDomain(new HRegion(region));
            return imageEdge.EdgesSubPix("canny", 3, cannyLow, cannyHigh);
        }

        /// <summary>
        /// Return a single intersection point of a line and a contour
        /// </summary>
        /// <param name="line"></param>
        /// <param name="contour"></param>
        /// <returns></returns>
        private Point LineContourIntersection(Line line, HObject contour)
        {
            HTuple x, y, _, contourLength;
            HalconScripts.LongestXLD(contour, out contour, out contourLength);
            HOperatorSet.IntersectionLineContourXld(contour, line.YStart, line.XStart, line.YEnd, line.XEnd, out y,
                out x, out _);

            return new Point(x.D, y.D);
        }
    }
}