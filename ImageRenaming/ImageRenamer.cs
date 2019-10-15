using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageRenaming
{
    public class ImageRenamer
    {
        public int NumPatterns { get; set; }

        public ImageRenamer(int numPatterns)
        {
            NumPatterns = numPatterns;
        }

        public List<string> FilterNonImagePaths(IEnumerable<string> paths)
        {
            var output = new List<string>();
            foreach (var path in paths)
            {
                if(IsImageFile(path)) output.Add(path);
            }

            return output;
        }



        public List<string> GetNewPaths(List<string> imagePaths)
        {
            var numPaths = imagePaths.Count;
           if (numPaths % NumPatterns != 0) throw new InvalidOperationException("Wrong number of image inputs");
            var output = new List<string>();
            for (int i = 0; i < numPaths; i++)
            {
                
            }
        }

        private bool IsImageFile(string imagePath)
        {
            return ImageExtensions.Contains(Path.GetExtension(imagePath)?.ToUpper());
        }

        /// <summary>
        /// Known list of image extensions to filter non-image files
        /// </summary>
        private static readonly List<string> ImageExtensions = new List<string>
            {".JPG", ".JPE", ".BMP", ".TIF", ".PNG"};
    }
}