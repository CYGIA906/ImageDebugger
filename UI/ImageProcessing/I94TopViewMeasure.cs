﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using HalconDotNet;
using UI.ImageProcessing.Utilts;
using UI.Model;
using UI.ViewModels;

namespace UI.ImageProcessing
{
    public class I94TopViewMeasure : IMeasurementProcedure
    {
        private static HDevelopExport HalconScripts = new HDevelopExport();
        private string _modelPath;
        private HTuple _shapeModelHandle;
        public ObservableCollection<FaiItem> FaiItems { get; }
        public event Action MeasurementResultReady;
        public string Name { get; }
        public event Action MeasurementResultPulled;

        public Dictionary<string, double> Process(List<HImage> images, FindLineConfigs findLineConfigs,
            ObservableCollection<FaiItem> faiItems, out HalconGraphics graphics, out PointsRecorder recorder)
        {
            HObject imageUndistorted;
            HTuple changeOfBase;
            HTuple changeOfBaseInv;
            HTuple rotationMat;
            HTuple rotationMatInv;
            HTuple mapToWorld;
            HTuple mapToImage;
            HTuple xLeft, yLeft, xRight, yRight, xUp, yUp, xDown, yDown;
            HTuple baseRightCol,
                baseRightLen1,
                baseRightLen2,
                baseRightRadian,
                baseRightRow,
                baseTopCol,
                baseTopLen1,
                baseTopLen2,
                baseTopRadian,
                baseTopRow,
                camParams;


            // Calculate matrices


            HalconScripts.GetI94TopViewBaseRects(images[0], out imageUndistorted, _shapeModelHandle, out baseTopRow,
                out baseTopCol, out baseTopRadian, out baseTopLen1, out baseTopLen2, out baseRightRow, 
                out baseRightCol, out baseRightRadian, out baseRightLen1, out baseRightLen2, out mapToWorld, out mapToImage, out camParams
                );
            // Undistort images
            images[0] = imageUndistorted.HobjectToHimage();
            HTuple _;
            HalconScripts.UndistortImage(images[1], out imageUndistorted, camParams, out _);
            images[1] = imageUndistorted.HobjectToHimage();
            
            // Top base
            HObject edgesTop, findLineRegionTop;
            var XsYs = HalconHelper.FindLineSubPixel(images[0], baseTopRow, baseTopCol,
                baseTopRadian, baseTopLen1, baseTopLen2, "negative",
                10, 20, "first", 0,
                20, 40, 3, 3, 5,
                out edgesTop, out findLineRegionTop);

            var lineTopBase = HalconHelper.leastSquareAdaptLine(XsYs.Item1, XsYs.Item2);
            HalconScripts.SortLineLeftRight(lineTopBase.XStart, lineTopBase.YStart, lineTopBase.XEnd, lineTopBase.YEnd, out xLeft, out yLeft, out xRight, out yRight);
            lineTopBase = new Line(xRight, yRight, xLeft, yLeft, true);

            
             // Right base
             HObject edgesRight, findLineRegionRight;
              XsYs = HalconHelper.FindLineSubPixel(images[0], baseRightRow, baseRightCol,
                 baseRightRadian, baseRightLen1, baseRightLen2, "negative",
                 10, 20, "first", 0,
                 20, 40, 3, 3, 5,
                 out edgesRight, out findLineRegionRight);
              var lineRightBase = HalconHelper.leastSquareAdaptLine(XsYs.Item1, XsYs.Item2);
             HalconScripts.SortLineUpDown(lineRightBase.XStart, lineRightBase.YStart, lineRightBase.XEnd, lineRightBase.YEnd, out xUp, out yUp, out xDown, out yDown);
             lineRightBase = new Line(xUp, yUp, xDown, yDown, true);

             
            HalconScripts.GetChangeOfBase(xRight, yRight, xLeft, yLeft, xUp, yUp, xDown, yDown, out changeOfBase, out changeOfBaseInv, out rotationMat, out rotationMatInv);
            var coordinateSolver = new CoordinateSolver(changeOfBase, changeOfBaseInv, rotationMat, rotationMatInv, mapToWorld, mapToImage);
            recorder = new PointsRecorder(changeOfBaseInv);

           // Update absolute find line locations
           findLineConfigs.GenerateLocationsAbs(coordinateSolver);
           // Find lines
            var findLineManager = new FindLineManager(findLineConfigs.GenerateFindLineFeedings());
            findLineManager.FindLines(images);
            
            // Make parallel lines
            var lineFai2and3P2 = coordinateSolver.TranslateLineInWorldUnit(9, lineRightBase, true);
            
            var lineFai4P1 = coordinateSolver.TranslateLineInWorldUnit(3, lineRightBase, true);
            var lineFai4P2 = coordinateSolver.TranslateLineInWorldUnit(9.269, lineRightBase, true);
            var lineFai4P3 = coordinateSolver.TranslateLineInWorldUnit(15.5, lineRightBase, true);
            
            var lineFai5P1 = coordinateSolver.TranslateLineInWorldUnit(5.299, lineRightBase, true);
            var lineFai5P2 = coordinateSolver.TranslateLineInWorldUnit(9.299, lineRightBase, true);
            var lineFai5P3 = coordinateSolver.TranslateLineInWorldUnit(13.299, lineRightBase, true);

            var lineFai6P1 = coordinateSolver.TranslateLineInWorldUnit(22.024, lineRightBase, true);
            var lineFai6P2 = coordinateSolver.TranslateLineInWorldUnit(24.024, lineRightBase, true);
            var lineFai6P3 = coordinateSolver.TranslateLineInWorldUnit(26.024, lineRightBase, true);
            
            var lineFai9P1 = coordinateSolver.TranslateLineInWorldUnit(21.753, lineRightBase, true);
            var lineFai9P2 = coordinateSolver.TranslateLineInWorldUnit(23.753, lineRightBase, true);
            var lineFai9P3 = coordinateSolver.TranslateLineInWorldUnit(25.753, lineRightBase, true);
            
            var lineFai12P1 = coordinateSolver.TranslateLineInWorldUnit(23.213, lineTopBase, true);
            var lineFai12P2 = coordinateSolver.TranslateLineInWorldUnit(34.97, lineTopBase, true);
      
            var lineFai16P1 = coordinateSolver.TranslateLineInWorldUnit(13.071, lineTopBase, true);
            var lineFai16P2 = coordinateSolver.TranslateLineInWorldUnit(20.213, lineTopBase, true);
            
            var lineFai17P1 = coordinateSolver.TranslateLineInWorldUnit(1.53, lineTopBase, true);
            var lineFai17P2 = coordinateSolver.TranslateLineInWorldUnit(9.827, lineTopBase, true);
            
            var lineFai19P1 = coordinateSolver.TranslateLineInWorldUnit(2.157, lineTopBase, true);
            var lineFai19P2 = coordinateSolver.TranslateLineInWorldUnit(13.791, lineTopBase, true);
            
            var lineFai20CenterX = coordinateSolver.TranslateLineInWorldUnit(9.299, lineRightBase, true);
            var lineFai20CenterY = coordinateSolver.TranslateLineInWorldUnit(7.886, lineTopBase, true);
            
            // Intersections
            var p2F2 = lineFai2and3P2.Intersect(findLineManager.GetLine("02"));

            var p2F3 = lineFai2and3P2.Intersect(findLineManager.GetLine("03"));

            var p1F4 = lineFai4P1.Intersect((findLineManager.GetLine("04")));
            var p2F4 = lineFai4P2.Intersect((findLineManager.GetLine("04")));
            var p3F4 = lineFai4P3.Intersect((findLineManager.GetLine("04")));
            
            var p1F5 = lineFai5P1.Intersect((findLineManager.GetLine("05")));
            var p2F5 = lineFai5P2.Intersect((findLineManager.GetLine("05")));
            var p3F5 = lineFai5P3.Intersect((findLineManager.GetLine("05")));
            
            var p1F6 = lineFai6P1.Intersect((findLineManager.GetLine("06")));
            var p2F6 = lineFai6P2.Intersect((findLineManager.GetLine("06")));
            var p3F6 = lineFai6P3.Intersect((findLineManager.GetLine("06")));

            var p1F9 = lineFai9P1.Intersect((findLineManager.GetLine("09")));
            var p2F9 = lineFai9P2.Intersect((findLineManager.GetLine("09")));
            var p3F9 = lineFai9P3.Intersect((findLineManager.GetLine("09")));
            
            var p1F12 = lineFai12P1.Intersect((findLineManager.GetLine("12")));
            var p2F12 = lineFai12P2.Intersect((findLineManager.GetLine("12")));
            
            var p1F16 = lineFai16P1.Intersect((findLineManager.GetLine("16")));
            var p2F16 = lineFai16P2.Intersect((findLineManager.GetLine("16")));
            
            var p1F17 = lineFai17P1.Intersect((findLineManager.GetLine("17")));
            var p2F17 = lineFai17P2.Intersect((findLineManager.GetLine("17")));
            
            var p1F19 = lineFai19P1.Intersect((findLineManager.GetLine("19")));
            var p2F19 = lineFai19P2.Intersect((findLineManager.GetLine("19")));

            var p20Center = lineFai20CenterX.Intersect(lineFai20CenterY);
       
            // Measure point-line distances
            var valueF2P2 = coordinateSolver.PointLineDistanceInWorld(p2F2, lineTopBase);
            
            var valueF3P2 = coordinateSolver.PointLineDistanceInWorld(p2F3, lineTopBase);

            var valueF4P1 = coordinateSolver.PointLineDistanceInWorld(p1F4, lineTopBase);
            var valueF4P2 = coordinateSolver.PointLineDistanceInWorld(p2F4, lineTopBase);
            var valueF4P3 = coordinateSolver.PointLineDistanceInWorld(p3F4, lineTopBase);

            var valueF5P1 = coordinateSolver.PointLineDistanceInWorld(p1F5, lineTopBase);
            var valueF5P2 = coordinateSolver.PointLineDistanceInWorld(p2F5, lineTopBase);
            var valueF5P3 = coordinateSolver.PointLineDistanceInWorld(p3F5, lineTopBase);
            
            var valueF6P1 = coordinateSolver.PointLineDistanceInWorld(p1F6, lineTopBase);
            var valueF6P2 = coordinateSolver.PointLineDistanceInWorld(p2F6, lineTopBase);
            var valueF6P3 = coordinateSolver.PointLineDistanceInWorld(p3F6, lineTopBase);

            var valueF9P1 = coordinateSolver.PointLineDistanceInWorld(p1F9, lineTopBase);
            var valueF9P2 = coordinateSolver.PointLineDistanceInWorld(p2F9, lineTopBase);
            var valueF9P3 = coordinateSolver.PointLineDistanceInWorld(p3F9, lineTopBase);

            var valueF12P1 = coordinateSolver.PointLineDistanceInWorld(p1F12, lineRightBase);
            var valueF12P2 = coordinateSolver.PointLineDistanceInWorld(p2F12, lineRightBase);

            var valueF16P1 = coordinateSolver.PointLineDistanceInWorld(p1F16, lineRightBase);
            var valueF16P2 = coordinateSolver.PointLineDistanceInWorld(p2F16, lineRightBase);
            
            var valueF17P1 = coordinateSolver.PointLineDistanceInWorld(p1F17, lineRightBase);
            var valueF17P2 = coordinateSolver.PointLineDistanceInWorld(p2F17, lineRightBase);
            
            var valueF19P1 = coordinateSolver.PointLineDistanceInWorld(p1F19, lineRightBase);
            var valueF19P2 = coordinateSolver.PointLineDistanceInWorld(p2F19, lineRightBase);


            // Record points
            recorder.Record(findLineManager.GetLine("02"), lineRightBase, "XAxis-02");
            recorder.Record(findLineManager.GetLine("03"), lineRightBase, "YAxis-03");
            
            
            
            //Fai 20.1
            var ptOrigin = lineTopBase.Intersect(lineRightBase);
            HTuple rotatedX1, rotatedX2, rotatedY1, rotatedY2;
            HObject lineRegion;
            HalconScripts.PivotLineAroundPoint(out lineRegion, xLeft, yLeft, xRight, yRight, ptOrigin.ImageX, ptOrigin.ImageY,
                MathUtils.ToRadian(45), "right", 5120, 5120, out rotatedX1, out rotatedY1, out rotatedX2,
                out rotatedY2);
            var lineRotated = new Line(rotatedX1.D, rotatedY1.D, rotatedX2.D, rotatedY2.D);
            lineRotated.IsVisible = true;            

            HalconScripts.PivotLineAroundPoint(out lineRegion, xLeft, yLeft, xRight, yRight, ptOrigin.ImageX, ptOrigin.ImageY,
                MathUtils.ToRadian(-45), "right", 5120, 5120, out rotatedX1, out rotatedY1, out rotatedX2,
                out rotatedY2);
            var lineProject = new Line(rotatedX1.D, rotatedY1.D, rotatedX2.D, rotatedY2.D) {IsVisible = true};
            var lineFai20TopRight = findLineManager.GetLine("20.topRight");
            var pointOnTopRight = new Point((lineFai20TopRight.XStart + lineFai20TopRight.XEnd)/2.0, (lineFai20TopRight.YStart + lineFai20TopRight.YEnd)/2.0);
            var lineFai20BottomLeft = findLineManager.GetLine("20.bottomLeft");
            var valueF20P1 = coordinateSolver.PointLineDistanceInWorld(pointOnTopRight, lineFai20BottomLeft);
            var linePerpendiculerToBottomLeft =
                lineFai20BottomLeft.PerpendicularLineThatPasses(pointOnTopRight);
            linePerpendiculerToBottomLeft.IsVisible = true;
            var angle = linePerpendiculerToBottomLeft.AngleWithLine(lineProject);
            var cosValue = Math.Cos(angle);
            valueF20P1 = valueF20P1 * cosValue;
            
            // Fai 20.2
            var pointOnBottomLeft = linePerpendiculerToBottomLeft.Intersect(lineFai20BottomLeft);
            var midPoint = new Point((pointOnBottomLeft.ImageX + pointOnTopRight.ImageX) / 2.0,
                (pointOnBottomLeft.ImageY + pointOnTopRight.ImageY) / 2.0);
            var pointMidProjected = lineProject.ProjectPoint(midPoint);
            var distanceToOrigin = coordinateSolver.PointPointDistanceInWorld(pointMidProjected, ptOrigin, true);
            var valueF20P2 = Math.Abs(12.202 - distanceToOrigin) * 2;




            // outputs
            var outputs = new Dictionary<string, double>();
            
            //Stop auto-serialization of FaiItems
            OnMeasurementResultReady();
            
            outputs["02_2"] = valueF2P2;
            
            outputs["03_2"] = valueF3P2;

            outputs["04_1"] = valueF4P1;
            outputs["04_2"] = valueF4P2;
            outputs["04_3"] = valueF4P3;
            
            outputs["05_1"] = valueF5P1;
            outputs["05_2"] = valueF5P2;
            outputs["05_3"] = valueF5P3;
            
            outputs["06_1"] = valueF6P1;
            outputs["06_2"] = valueF6P2;
            outputs["06_3"] = valueF6P3;
            
            outputs["09_1"] = valueF9P1;
            outputs["09_2"] = valueF9P2;
            outputs["09_3"] = valueF9P3;
            
            outputs["12_1"] = valueF12P1;
            outputs["12_2"] = valueF12P2;

            outputs["16_1"] = valueF16P1;
            outputs["16_2"] = valueF16P2;

            outputs["17_1"] = valueF17P1;
            outputs["17_2"] = valueF17P2;

            outputs["19_1"] = valueF19P1;
            outputs["19_2"] = valueF19P2;

            outputs["20_1"] = valueF20P1;

            outputs["20_2"] = valueF20P2;     


             graphics = new HalconGraphics()
             {
                 CrossesIgnored = findLineManager.CrossesIgnored,
                 CrossesUsed = findLineManager.CrossesUsed,
                 FindLineRects = findLineManager.FindLineRects,
                 LineRegions = findLineManager.LineRegions,
                 Edges = findLineManager.Edges,
                 PointPointGraphics = coordinateSolver.PointPointDistanceGraphics,
                 PointLineGraphics = coordinateSolver.PointLineDistanceGraphics, 
                 Image = images[1]
             };

            return outputs;
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

        public I94TopViewMeasure(string name)
        {
            Name = name;
            ModelPath = "./ModelTopViewI94";
        }


        protected virtual void OnMeasurementResultReady()
        {
            MeasurementResultReady?.Invoke();
        }

        protected virtual void OnMeasurementResultPulled()
        {
            MeasurementResultPulled?.Invoke();
        }
    }
}