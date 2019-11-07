using ImageDebugger.Core.ViewModels.Base;

namespace ImageDebugger.Core.ViewModels
{
    public class ImageInfoViewModel : ViewModelBase
    {
        /// <summary>
        /// X coordinate of the cursor in image
        /// </summary>
        public string X { get; set; }

        /// <summary>
        /// Y coordinate of the cursor in image
        /// </summary>
        public string Y { get; set; }


        /// <summary>
        /// Gray value of the image point
        /// </summary>
        public double GrayValue { get; set; }
        
    }
}