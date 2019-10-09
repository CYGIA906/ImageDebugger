using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using HalconDotNet;
using MathNet.Numerics.LinearAlgebra;
using UI.Commands;
using UI.ImageProcessing;

// TODO: add faiitem serialize logic
namespace UI.ViewModels
{
    public partial class HalconWindowPageViewModel : ViewModelBase
    {
        public ObservableCollection<FaiItem> FaiItems { get; private set; }

        public ObservableCollection<FindLineParam> FindLineParams { get; private set; }

        private HWindow _windowHandle;

        private FindLineConfigs _findLineConfigs;

        public HObject DisplayImage { get; set; }

        private readonly IMeasurementProcedure measurementUnit = new I94TopViewMeasure();

        public ICommand ExecuteCommand { get; }
        public ICommand ContinuousRunCommand { get; }

        public string ParamSerializationBaseDir => SerializationDir + "/FindLineParams";


        public void Process(List<HImage> images)
        {
            measurementUnit.Process(images, _findLineConfigs, _windowHandle, FaiItems);
        }


        private void ShowImageAndGraphics(HImage image, HObject graphics)
        {
//            HOperatorSet.ClearWindow(_windowHandle);
            image.DispObj(_windowHandle);
            if (graphics.IsInitialized()) graphics.DispObj(_windowHandle);
        }


        /// <summary>
        /// Take measured values from halcon script and fill them into <see cref="FaiItems"/>
        /// </summary>
        /// <param name="measuredValues"></param>
        private void FillFaiItems(HTuple measuredValues)
        {
            var values = measuredValues.ToDArr();
            var numValues = values.Length;
            var numItems = FaiItems.Count;
            if (numItems != numValues)
                throw new InvalidOperationException($"Expect {numItems} but get {numValues} measured values");

            for (int i = 0; i < numValues; i++)
            {
                FaiItems[i].Value = values[i];
            }
        }


        public HalconWindowPageViewModel(HWindow windowHandle)
        {
            _windowHandle = windowHandle;
            // Init fai items
            var faiItemsFromDisk = TryLoadFaiItemsFromDisk();
            FaiItems = faiItemsFromDisk ?? FaiItemHardCodeValues();
            foreach (var item in FaiItems)
            {
                item.ResumeAutoSerialization();
            }

            // Init find line params
            var findLineParamsFromDisk = TryLoadFindLineParamsFromDisk();
            FindLineParams = findLineParamsFromDisk ?? FindLineParamsHardCodeValues();
            foreach (var param in FindLineParams)
            {
                param.ResumeAutoSerialization();
            }

            // Init find line locations
            var findLineLocations = FindLineLocationHardCodeValues();
            _findLineConfigs = new FindLineConfigs(FindLineParams.ToList(), findLineLocations);

            // Listen for user changes of data grid
            measurementUnit.MeasurementResultReady += FaiItemsStopListeningToChange;
            measurementUnit.MeasurementResultPulled += FaiItemsRestartListeningToChange;


            // Init commands
            ExecuteCommand = new RelayCommand(() => { ProcessOnce(); });

            SelectImageDirCommand = new RelayCommand(() =>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        ImageDirectory = fbd.SelectedPath;
                    }
                }
            });

            ContinuousRunCommand = new RelayCommand(() =>
            {
                while (RunContinuously)
                {
                    ProcessOnce();
                }
            });
        }

        private void ProcessOnce()
        {
            var inputs = ImageInputs;
            if (inputs == null) return;

            Process(inputs);
        }

        public bool RunContinuously { get; set; } = false;

        private void FaiItemsRestartListeningToChange()
        {
            foreach (var item in FaiItems)
            {
                item.ResumeAutoSerialization();
            }
        }

        private void FaiItemsStopListeningToChange()
        {
            foreach (var item in FaiItems)
            {
                item.StopAutoSerialization();
            }
        }

        public string SerializationDir { get; set; } = Application.StartupPath + "/I94";

        public string FaiItemSerializationDir => SerializationDir + "/FaiItems";


        private ObservableCollection<FaiItem> TryLoadFaiItemsFromDisk()
        {
            var directoryInfo = Directory.CreateDirectory(FaiItemSerializationDir);
            var xmls = directoryInfo.GetFiles("*.xml");
            if (xmls.Length == 0) return null;

            var outputs = new ObservableCollection<FaiItem>();
            foreach (var fileInfo in xmls)
            {
                string filePath = fileInfo.FullName;
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(FaiItem));
                    FaiItem item = (FaiItem) serializer.Deserialize(fs);
                    outputs.Add(item);
                }
            }

            foreach (var item in outputs)
            {
                item.ResumeAutoSerialization();
            }

            return outputs;
        }


        private ObservableCollection<FindLineParam> TryLoadFindLineParamsFromDisk()
        {
            var directoryInfo = Directory.CreateDirectory(ParamSerializationBaseDir);
            var xmls = directoryInfo.GetFiles("*.xml");
            if (xmls.Length == 0) return null;

            var outputs = new ObservableCollection<FindLineParam>();
            foreach (var fileInfo in xmls)
            {
                string filePath = fileInfo.FullName;
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(FindLineParam));
                    FindLineParam item = (FindLineParam) serializer.Deserialize(fs);
                    outputs.Add(item);
                }
            }

            foreach (var item in outputs)
            {
                item.ResumeAutoSerialization();
            }

            return outputs;
        }

        

        #region Image Providing Logic

        /// <summary>
        /// Provide next image
        /// <exception cref="InvalidDataException">When images are all consumed</exception>
        /// </summary>
        public List<HImage> ImageInputs
        {
            get
            {
                if (CountOfQueuesNotEqual)
                    throw new InvalidOperationException("Image queues do not agree on their counts");

                if (ImagesRunOut) return null;

                var outputs = new List<HImage>();
                foreach (var queue in ImageQueues)
                {
                    outputs.Add(new HImage(queue.Dequeue()));
                }

                return outputs;
            }
        }

        private bool ImagesRunOut => ImageQueues[0].Count == 0;

        private bool CountOfQueuesNotEqual
        {
            get
            {
                var countOfFirstQueue = ImageQueues[0].Count;
                return ImageQueues.Any(ele => ele.Count != countOfFirstQueue);
            }
        }

        public List<Queue<string>> ImageQueues { get; set; } = new List<Queue<string>>();

        /// <summary>
        /// The number of images available
        /// </summary>
        public int NumImages => ImageQueues.Count == 0 ? 0 : ImageQueues[0].Count;


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
                    var imageIndexString = imageName.Substring(imageName.IndexOf("_", StringComparison.Ordinal) + 1,
                        imageName.Length);
                    imageIndex = int.Parse(imageIndexString) - 1;
                }

                ImageQueues[imageIndex].Enqueue(path);
            }

            if (numImagesInOneGo == 1) return;

            var sortedImageQueues = new List<Queue<string>>();
            foreach (var queue in ImageQueues)
            {
                var orderedQueue = new Queue<string>(queue.OrderBy(Path.GetFileName));
                sortedImageQueues.Add(orderedQueue);
            }

            ImageQueues = sortedImageQueues;
        }

        private void ResetImageQueues(int numImagesInOneGo)
        {
            ImageQueues.Clear();
            for (int i = 0; i < numImagesInOneGo; i++)
            {
                ImageQueues.Add(new Queue<string>());
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
            if (!nameToTest.Contains("_")) return 1;

            var testPrefix = nameToTest.Substring(0, nameToTest.IndexOf("_", StringComparison.Ordinal) + 1);

            return allImageNames.Count(ele => ele.Contains(testPrefix));
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