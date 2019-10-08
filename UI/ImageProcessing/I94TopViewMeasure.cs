using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using HalconDotNet;
using UI.ImageProcessing.Utilts;
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
        public event Action MeasurementResultPulled;

        public void Process(List<HImage> images, FindLineConfigs findLineConfigs, HWindow windowHandle,
            ObservableCollection<FaiItem> faiItems)
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
            HalconScripts.I94TopViewChangeBase(images[0], out imageUndistorted, _shapeModelHandle, out changeOfBase,
                out changeOfBaseInv, out rotationMat, out rotationMatInv, out mapToWorld, out mapToImage, out xLeft,
                out yLeft, out xRight, out yRight, out xUp, out yUp, out xDown, out yDown);
            var xAxis = new Line(xLeft.D, yLeft.D, xRight.D, yRight.D, true);
            var yAxis = new Line(xUp.D, yUp.D, xDown.D, yDown.D, true);

           var coordinateSolver = new CoordinateSolver(changeOfBase, changeOfBaseInv, rotationMat, rotationMatInv, mapToWorld, mapToImage);
            // Update absolute find line locations
            findLineConfigs.Solver = coordinateSolver;
            // Find lines
            var findLineManager = new FindLineManager(findLineConfigs);
            findLineManager.FindLines(images);
            
            // Make parallel lines
            var lineFai2and3P2 = coordinateSolver.TranslateLineInWorldUnit(9, yAxis, true);
            
            var lineFai4P1 = coordinateSolver.TranslateLineInWorldUnit(3, yAxis, true);
            var lineFai4P2 = coordinateSolver.TranslateLineInWorldUnit(9.269, yAxis, true);
            var lineFai4P3 = coordinateSolver.TranslateLineInWorldUnit(15.5, yAxis, true);
            
            var lineFai5P1 = coordinateSolver.TranslateLineInWorldUnit(5.299, yAxis, true);
            var lineFai5P2 = coordinateSolver.TranslateLineInWorldUnit(9.299, yAxis, true);
            var lineFai5P3 = coordinateSolver.TranslateLineInWorldUnit(13.299, yAxis, true);

            var lineFai6P1 = coordinateSolver.TranslateLineInWorldUnit(22.024, yAxis, true);
            var lineFai6P2 = coordinateSolver.TranslateLineInWorldUnit(24.024, yAxis, true);
            var lineFai6P3 = coordinateSolver.TranslateLineInWorldUnit(26.024, yAxis, true);
            
            var lineFai9P1 = coordinateSolver.TranslateLineInWorldUnit(21.753, yAxis, true);
            var lineFai9P2 = coordinateSolver.TranslateLineInWorldUnit(23.753, yAxis, true);
            var lineFai9P3 = coordinateSolver.TranslateLineInWorldUnit(25.753, yAxis, true);
            
            var lineFai12P1 = coordinateSolver.TranslateLineInWorldUnit(23.213, xAxis, true);
            var lineFai12P2 = coordinateSolver.TranslateLineInWorldUnit(34.97, xAxis, true);
      
            var lineFai16P1 = coordinateSolver.TranslateLineInWorldUnit(13.071, xAxis, true);
            var lineFai16P2 = coordinateSolver.TranslateLineInWorldUnit(20.213, xAxis, true);
            
            var lineFai17P1 = coordinateSolver.TranslateLineInWorldUnit(1.53, xAxis, true);
            var lineFai17P2 = coordinateSolver.TranslateLineInWorldUnit(9.827, xAxis, true);
            
            var lineFai19P1 = coordinateSolver.TranslateLineInWorldUnit(2.157, xAxis, true);
            var lineFai19P2 = coordinateSolver.TranslateLineInWorldUnit(13.791, xAxis, true);
            
            var lineFai20CenterX = coordinateSolver.TranslateLineInWorldUnit(9.299, yAxis, true);
            var lineFai20CenterY = coordinateSolver.TranslateLineInWorldUnit(7.886, xAxis, true);
            
            // Intersections
            var p2F2 = lineFai2and3P2.Intersect(findLineManager.GetLine("2"));

            var p2F3 = lineFai2and3P2.Intersect(findLineManager.GetLine("3"));

            var p1F4 = lineFai4P1.Intersect((findLineManager.GetLine("4")));
            var p2F4 = lineFai4P2.Intersect((findLineManager.GetLine("4")));
            var p3F4 = lineFai4P3.Intersect((findLineManager.GetLine("4")));
            
            var p1F5 = lineFai5P1.Intersect((findLineManager.GetLine("5")));
            var p2F5 = lineFai5P2.Intersect((findLineManager.GetLine("5")));
            var p3F5 = lineFai5P3.Intersect((findLineManager.GetLine("5")));
            
            var p1F6 = lineFai6P1.Intersect((findLineManager.GetLine("6")));
            var p2F6 = lineFai6P2.Intersect((findLineManager.GetLine("6")));
            var p3F6 = lineFai6P3.Intersect((findLineManager.GetLine("6")));

            var p1F9 = lineFai9P1.Intersect((findLineManager.GetLine("9")));
            var p2F9 = lineFai9P2.Intersect((findLineManager.GetLine("9")));
            var p3F9 = lineFai9P3.Intersect((findLineManager.GetLine("9")));
            
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
            var valueF2P2 = coordinateSolver.PointLineDistanceInWorld(p2F2, xAxis);
            
            var valueF3P2 = coordinateSolver.PointLineDistanceInWorld(p2F3, xAxis);

            var valueF4P1 = coordinateSolver.PointLineDistanceInWorld(p1F4, xAxis);
            var valueF4P2 = coordinateSolver.PointLineDistanceInWorld(p2F4, xAxis);
            var valueF4P3 = coordinateSolver.PointLineDistanceInWorld(p3F4, xAxis);

            var valueF5P1 = coordinateSolver.PointLineDistanceInWorld(p1F5, xAxis);
            var valueF5P2 = coordinateSolver.PointLineDistanceInWorld(p2F5, xAxis);
            var valueF5P3 = coordinateSolver.PointLineDistanceInWorld(p3F5, xAxis);
            
            var valueF6P1 = coordinateSolver.PointLineDistanceInWorld(p1F6, xAxis);
            var valueF6P2 = coordinateSolver.PointLineDistanceInWorld(p2F6, xAxis);
            var valueF6P3 = coordinateSolver.PointLineDistanceInWorld(p3F6, xAxis);

            var valueF9P1 = coordinateSolver.PointLineDistanceInWorld(p1F9, xAxis);
            var valueF9P2 = coordinateSolver.PointLineDistanceInWorld(p2F9, xAxis);
            var valueF9P3 = coordinateSolver.PointLineDistanceInWorld(p3F9, xAxis);

            var valueF12P1 = coordinateSolver.PointLineDistanceInWorld(p1F12, yAxis);
            var valueF12P2 = coordinateSolver.PointLineDistanceInWorld(p2F12, yAxis);

            var valueF16P1 = coordinateSolver.PointLineDistanceInWorld(p1F16, yAxis);
            var valueF16P2 = coordinateSolver.PointLineDistanceInWorld(p2F16, yAxis);
            
            var valueF17P1 = coordinateSolver.PointLineDistanceInWorld(p1F17, yAxis);
            var valueF17P2 = coordinateSolver.PointLineDistanceInWorld(p2F17, yAxis);
            
            var valueF19P1 = coordinateSolver.PointLineDistanceInWorld(p1F19, yAxis);
            var valueF19P2 = coordinateSolver.PointLineDistanceInWorld(p2F19, yAxis);
            
            
            
            //Fai 20.1
            var ptOrigin = xAxis.Intersect(yAxis);
            HTuple rotatedX1, rotatedX2, rotatedY1, rotatedY2;
            HObject lineRegion;
            HalconScripts.PivotLineAroundPoint(out lineRegion, xLeft, yLeft, xRight, yRight, ptOrigin.X, ptOrigin.Y,
                MathUtils.ToRadian(45), "right", 5120, 5120, out rotatedX1, out rotatedY1, out rotatedX2,
                out rotatedY2);
            var lineRotated = new Line(rotatedX1.D, rotatedY1.D, rotatedX2.D, rotatedY2.D);
            lineRotated.IsVisible = true;            

            HalconScripts.PivotLineAroundPoint(out lineRegion, xLeft, yLeft, xRight, yRight, ptOrigin.X, ptOrigin.Y,
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
            var midPoint = new Point((pointOnBottomLeft.X + pointOnTopRight.X) / 2.0,
                (pointOnBottomLeft.Y + pointOnTopRight.Y) / 2.0);
            var pointMidProjected = lineProject.ProjectPoint(midPoint);
            var distanceToOrigin = coordinateSolver.PointPointDistanceInWorld(pointMidProjected, ptOrigin, true);
            var valueF20P2 = Math.Abs(12.202 - distanceToOrigin) * 2;




            // outputs
            
            //Stop auto-serialization of FaiItems
            OnMeasurementResultReady();
            
            faiItems.ByName("02_2").Value = valueF2P2;
            
            faiItems.ByName("03_2").Value = valueF3P2;

            faiItems.ByName("04_1").Value = valueF4P1;
            faiItems.ByName("04_2").Value = valueF4P2;
            faiItems.ByName("04_3").Value = valueF4P3;
            
            faiItems.ByName("05_1").Value = valueF5P1;
            faiItems.ByName("05_2").Value = valueF5P2;
            faiItems.ByName("05_3").Value = valueF5P3;
            
            faiItems.ByName("06_1").Value = valueF6P1;
            faiItems.ByName("06_2").Value = valueF6P2;
            faiItems.ByName("06_3").Value = valueF6P3;
            
            faiItems.ByName("09_1").Value = valueF9P1;
            faiItems.ByName("09_2").Value = valueF9P2;
            faiItems.ByName("09_3").Value = valueF9P3;
            
            faiItems.ByName("12_1").Value = valueF12P1;
            faiItems.ByName("12_2").Value = valueF12P2;

            faiItems.ByName("16_1").Value = valueF16P1;
            faiItems.ByName("16_2").Value = valueF16P2;

            faiItems.ByName("17_1").Value = valueF17P1;
            faiItems.ByName("17_2").Value = valueF17P2;

            faiItems.ByName("19_1").Value = valueF19P1;
            faiItems.ByName("19_2").Value = valueF19P2;

            faiItems.ByName("20_1").Value = valueF20P1;

            faiItems.ByName("20_2").Value = valueF20P2;     
            
            // Restart FaiItems auto-serialization
            OnMeasurementResultPulled();

            windowHandle.DispImage(images[0]);
            findLineManager.DisplayGraphics(windowHandle);
            coordinateSolver.DisplayGraphics(windowHandle);
            Line.DisplayGraphics(windowHandle);
            windowHandle.SetColored(3);
            windowHandle.SetPart(0,0,5120,5120);
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