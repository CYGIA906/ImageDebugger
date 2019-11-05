using ImageDebugger.Core.ViewModels.Base;

namespace ImageDebugger.Core.ViewModels
{
    public class ImageInfoViewModel : ViewModelBase
    {
        /// <summary>
        /// X coordinate of the cursor in image
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y coordinate of the cursor in image
        /// </summary>
        public double Y { get; set; }


        /// <summary>
        /// Gray value of the image point
        /// </summary>
        public double GrayValue { get; set; }
        
    }
}