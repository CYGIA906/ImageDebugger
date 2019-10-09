using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using HalconDotNet;
using UI.Model;

namespace UI.ViewModels
{
    public partial class HalconWindowPageViewModel
    {
                #region Image Providing Logic

                public string CurrentImageName { get; set; }

        /// <summary>
        /// Provide next image
        /// <exception cref="InvalidDataException">When images are all consumed</exception>
        /// </summary>
        private List<HImage> ImageInputs
        {
            get
            {
                if (CountOfQueuesNotEqual)
                    throw new InvalidOperationException("Image queues do not agree on their counts");

                if (ImagesRunOut) return null;

                var outputs = new List<HImage>();
                foreach (var queue in ImageQueues)
                {
                    var imagePath = queue.Dequeue();
                    outputs.Add(new HImage(imagePath));
                    CurrentImageName = GetImageName(imagePath);
                }

                NumImages = ImageQueues[0].Count;
                return outputs;
            }
        }

        private string GetImageName(string imagePath)
        {
            return Path.GetFileName(imagePath);
        }

        private bool ImagesRunOut => NumImages == 0;

        private bool CountOfQueuesNotEqual
        {
            get
            {
                var countOfFirstQueue = ImageQueues[0].Count;
                return ImageQueues.Any(ele => ele.Count != countOfFirstQueue);
            }
        }

        private List<CountAwareQueue<string>> ImageQueues { get; set; } = new List<CountAwareQueue<string>>();

        /// <summary>
        /// The number of images available
        /// </summary>
        public int NumImages { get; set; }


        /// <summary>
        /// Known list of image extensions to filter non-image files
        /// </summary>
        private static readonly List<string> ImageExtensions = new List<string>
            {".JPG", ".JPE", ".BMP", ".TIF", ".PNG"};

        private string _imageDirectory;

        /// <summary>
        /// Directory to images 
        /// </summary>
        private string ImageDirectory
        {
            get { return _imageDirectory; }
            set
            {
                _imageDirectory = value;

                string[] filePaths = Directory.GetFiles(_imageDirectory);
                var imagePaths = new List<string>();

                foreach (var imagePath in filePaths)
                {
                    if (IsImageFile(imagePath))
                    {
                        imagePaths.Add(imagePath);
                    }
                }

                QueueUpImages(imagePaths);
            }
        }

        private string Separator { get; set; } = "-";
        
        private void QueueUpImages(List<string> imagePaths)
        {
            int numImagesInOneGo = GetNumImagesInOneGo(imagePaths);
            ResetImageQueues(numImagesInOneGo);


            foreach (var path in imagePaths)
            {
                int imageIndex = 0;

                if (numImagesInOneGo > 1)
                {
                    var imageName = Path.GetFileName(path);
                    var start = imageName.IndexOf(Separator, StringComparison.Ordinal) + 1;
                    var length = 1;
                    var imageIndexString = imageName.Substring(start, length);
                    imageIndex = int.Parse(imageIndexString) - 1;
                }

                ImageQueues[imageIndex].Enqueue(path);
            }

            if (numImagesInOneGo == 1) return;

            var sortedImageQueues = new List<CountAwareQueue<string>>();
            foreach (var queue in ImageQueues)
            {
                var orderedQueue = new CountAwareQueue<string>(queue.OrderBy(Path.GetFileName));
                sortedImageQueues.Add(orderedQueue);
            }

            ImageQueues = sortedImageQueues;

            NumImages = ImageQueues[0].Count;
        }

        private void ResetImageQueues(int numImagesInOneGo)
        {
            ImageQueues.Clear();
            for (int i = 0; i < numImagesInOneGo; i++)
            {
                ImageQueues.Add(new CountAwareQueue<string>());
            }

        }

       

        /// <summary>
        /// Determine how many images should be provided within one button hit
        /// </summary>
        /// <param name="imagePaths"></param>
        /// <returns></returns>
        private int GetNumImagesInOneGo(List<string> imagePaths)
        {
            var allImageNames = imagePaths.Select(Path.GetFileName);
            var nameToTest = Path.GetFileName(imagePaths[0]);

            // Naming convention: images belong to the same group will have the same prefix
            // for example: 02_1 and 02_2 have the same prefix 02_
            if (!nameToTest.Contains(Separator)) return 1;

            var testPrefix = nameToTest.Substring(0, nameToTest.IndexOf(Separator, StringComparison.Ordinal) + 1);

            return allImageNames.Count(ele => ele.StartsWith(testPrefix));
        }

        /// <summary>
        /// Filter image files based on file extension
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private bool IsImageFile(string imagePath)
        {
            return ImageExtensions.Contains(Path.GetExtension(imagePath)?.ToUpper());
        }

        public ICommand SelectImageDirCommand { get; private set; }

        #endregion

    }
}