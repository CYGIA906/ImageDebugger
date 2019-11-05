using System.Collections.Generic;
using System.Windows.Media;
using HalconDotNet;
using ImageDebugger.Core.ImageProcessing.Utilts;
using ImageDebugger.Core.Models;
using ImageDebugger.Core.ViewModels.LineScan;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;
using MaterialDesignThemes.Wpf;

namespace ImageDebugger.Core.ImageProcessing.LineScan.Procedure
{
    public partial class I40LineScanMeasurement
    {
        public ImageProcessingResults3D Process(List<HImage> images, List<PointSettingViewModel> pointSettings,
            ISnackbarMessageQueue messageQueue)
        {
            var bottomImage = images[0];
            var leftImage = images[1];
            var rightImage = images[2];
            HTuple imageWidth, imageHeight;
            rightImage.GetImageSize(out imageWidth, out imageHeight);


            HObject leftImageAligned, bottomImageAligned, _;
            HTuple rowBeginB, colBeginB, rowEndB, colEndB, rowBeginC, colBeginC, rowEndC, colEndC;
            _halconScripts.I40AlignAllAndGetBaseLines(rightImage, leftImage, bottomImage, out leftImageAligned,
                out bottomImageAligned, out _, out _, _shapeModelHandleRight,out rowBeginB, out colBeginB, 
                out rowEndB, out colEndB, out rowBeginC, out colBeginC, out rowEndC, out colEndC);
            

         
            // Translate base
           var lineB = new Line(colBeginB, rowBeginB, colEndB, rowEndB, true).SortUpDown();
           var lineC  = new Line(colBeginC, rowBeginC, colEndC, rowEndC, true).SortLeftRight();
           
            var xAxis = lineB.Translate(-1.0 / _horizontalCoeff * 6.788).InvertDirection();
            xAxis.IsVisible = true;
            var yAxis = lineC.Translate(-1.0 / _verticalCoeff * 19.605);
            yAxis.IsVisible = true;

            // Record debugging data
            var recordings = new List<ICsvColumnElement>();
            recordings.Add(new AngleItem()
            {
                Name = "BC",
                Value = lineB.AngleWithLine(lineC)
            });
            recordings.Add(new AngleItem()
            {
                Name = "C",
                Value = lineC.AngleWithLine(new Line(1,0,2,0))
            });



            var imageB = bottomImageAligned.HobjectToHimage();
            var imageL = leftImageAligned.HobjectToHimage();
            var imageR = rightImage;
            
            var imageBHeight = imageB.ToKeyenceHeightImage();
            var imageLHeight = imageL.ToKeyenceHeightImage();
            var imageRHeight = imageR.ToKeyenceHeightImage();
            
            var visualBR = VisualizeAlignment(rightImage, imageB);
            var visualLR = VisualizeAlignment(rightImage, imageL);

            
            var pointLocator = new PointLocator(xAxis, yAxis, _verticalCoeff, _horizontalCoeff);
            var pointMarkers = pointLocator.LocatePoints(pointSettings, new List<HImage>()
            {
                imageBHeight, imageLHeight, imageRHeight
            });
            


            var output = new ImageProcessingResults3D()
            {
                Images = new List<HImage>()
                {
                     imageBHeight, imageLHeight, imageRHeight, visualBR, visualLR
                },
                PointMarkers = pointMarkers,
                RecordingElements = recordings,
                //TODO: fix this buzzard -1
                ChangeOfBaseInv = MathUtils.GetChangeOfBaseInv(xAxis, yAxis, -1/_verticalCoeff, 1/_horizontalCoeff)
            };
            

            return output;
        }
    }
}