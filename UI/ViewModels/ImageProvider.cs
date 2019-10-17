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

                public int CurrentImageIndex { get; set; }

                public bool EndOfOneRound
                {
            get
            {
                if (ImageMegaList.Count == 0) return true;
              return  CurrentImageIndex == ImageMegaList[0].Count;
            }
                }

        /// <summary>
        /// Provide next image
        /// <exception cref="InvalidDataException">When images are all consumed</exception>
        /// </summary>
        private List<HImage> ImageInputs
        {
            get
            {

                if (TotalImages == 0) return null;

                if (EndOfOneRound)
                {
                    CurrentImageIndex = 0;
                    return null;
                }

                var outputs = new List<HImage>();
                foreach (var list in ImageMegaList)
                {
                    var imagePath = list[CurrentImageIndex];
                    outputs.Add(new HImage(imagePath));
                    CurrentImageName = GetImageName(imagePath);
                }

                CurrentImageIndex++;

                return outputs;
            }
        }

        public int TotalImages { get; set; }

        private string GetImageName(string imagePath)
        {
            return Path.GetFileName(imagePath);
        }

    

        private bool CountOfQueuesNotEqual
        {
            get
            {
                var countOfFirstQueue = ImageMegaList[0].Count;
                return ImageMegaList.Any(ele => ele.Count != countOfFirstQueue);
            }
        }

        private List<List<string>> ImageMegaList { get; set; } = new List<List<string>>();



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
                CurrentImageIndex = 0;
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

                if(imagePaths.Count == 0)
                {
                    RunStatusMessageQueue.Enqueue("This folder does not contains any supported images");
                    return;
                }

                QueueUpImages(imagePaths);

                _imageDirectory = string.Empty;
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

                ImageMegaList[imageIndex].Add(path);
            }

            var numImagesInFirstList = ImageMegaList[0].Count;
            if (ImageMegaList.Any(l => l.Count != numImagesInFirstList))
            {
                RunStatusMessageQueue.Enqueue("Incorrect file names of the input directory");
                return;
            }

            TotalImages = numImagesInFirstList;

            var sortedImageMegaList = new List<List<string>>();
            foreach (var queue in ImageMegaList)
            {
                var orderedQueue = new List<string>(queue.OrderBy(Path.GetFileName));
                sortedImageMegaList.Add(orderedQueue);
            }

            ImageMegaList = sortedImageMegaList;
        }

        private void ResetImageQueues(int numImagesInOneGo)
        {
            ImageMegaList.Clear();
            for (int i = 0; i < numImagesInOneGo; i++)
            {
                ImageMegaList.Add(new List<string>());
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