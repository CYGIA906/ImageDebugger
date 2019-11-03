using System.Collections.Generic;
using System.Windows.Media;
using HalconDotNet;
using ImageDebugger.Core.ImageProcessing.Utilts;
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

            #region Right View    

            HObject leftImageAligned, bottomImageAligned, _;
            HTuple rowBeginB, colBeginB, rowEndB, colEndB, rowBeginC, colBeginC, rowEndC, colEndC;
            _halconScripts.I40AlignAllAndGetBaseLines(rightImage, leftImage, bottomImage, out leftImageAligned,
                out bottomImageAligned, out _, out _, out rowBeginB, out colBeginB, 
                out rowEndB, out colEndB, out rowBeginC, out colBeginC, out rowEndC, out colEndC);
            

         
            // Translate base
           var lineB = new Line(colBeginB, rowBeginB, colEndB, rowEndB, true).SortUpDown();
           var lineC  = new Line(colBeginC, rowBeginC, colEndC, rowEndC, true).SortLeftRight();
           
            var xAxis = lineB.Translate(-1.0 / _horizontalCoeff * 6.788);
            xAxis.IsVisible = true;
            var yAxis = lineC.Translate(-1.0 / _verticalCoeff * 19.605);
            yAxis.IsVisible = true;

            #endregion




            var imageB = bottomImageAligned.HobjectToHimage();
            var imageL = leftImageAligned.HobjectToHimage();
            var imageR = rightImage;
            var imageRHeight = imageR.ToKeyenceHeightImage();
            var visualBR = VisualizeAlignment(rightImage, imageB);
            var visualLR = VisualizeAlignment(rightImage, imageL);


            var pointLocator = new PointLocator(xAxis.InvertDirection(), yAxis, _verticalCoeff, _horizontalCoeff);
            var pointMarkers = pointLocator.LocatePoints(pointSettings, new List<HImage>()
            {
                imageB.ToKeyenceHeightImage(), imageL.ToKeyenceHeightImage(), imageRHeight
            });


            var output = new ImageProcessingResults3D()
            {
                Images = new List<HImage>()
                {
                    imageRHeight, visualBR, visualLR
                },
                PointMarkers = pointMarkers
            };
            

            return output;
        }
    }
}