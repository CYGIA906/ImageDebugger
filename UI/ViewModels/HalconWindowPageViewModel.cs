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
    public class HalconWindowPageViewModel : ViewModelBase
    {
        public ObservableCollection<FaiItem> FaiItems { get; private set; }

        public ObservableCollection<FindLineParam> FindLineParams { get; private set; }

        private HWindow _windowHandle;

        private FindLineConfigs _findLineConfigs;

        public HObject DisplayImage { get; set; }

        private readonly IMeasurementProcedure measurementUnit = new I94TopViewMeasure();

        public ICommand ExecuteCommand { get; }

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
            ExecuteCommand = new RelayCommand(() =>
            {
                var inputs = ImageInputs;
                if (inputs == null) return;
                
                Process(inputs);
            });

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
        }

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

        #region Hard Code Values

        private ObservableCollection<FaiItem> FaiItemHardCodeValues()
        {
            var outputs = new ObservableCollection<FaiItem>();
            outputs.Add(new FaiItem("02_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 15.49, MinBoundary = 15.390});

            outputs.Add(new FaiItem("03_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 15.906, MinBoundary = 15.806});

            outputs.Add(new FaiItem("04_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 34.92, MinBoundary = 34.82});
            outputs.Add(new FaiItem("04_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 34.92, MinBoundary = 34.82});
            outputs.Add(new FaiItem("04_3")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 34.92, MinBoundary = 34.82});

            outputs.Add(new FaiItem("05_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 36.15, MinBoundary = 36.05});
            outputs.Add(new FaiItem("05_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 36.15, MinBoundary = 36.05});
            outputs.Add(new FaiItem("05_3")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 36.15, MinBoundary = 36.05});

            outputs.Add(new FaiItem("06_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 22.153, MinBoundary = 22.053});
            outputs.Add(new FaiItem("06_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 22.153, MinBoundary = 22.053});
            outputs.Add(new FaiItem("06_3")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 22.153, MinBoundary = 22.053});

            outputs.Add(new FaiItem("09_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 11.231, MinBoundary = 11.131});
            outputs.Add(new FaiItem("09_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 11.231, MinBoundary = 11.131});
            outputs.Add(new FaiItem("09_3")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 11.231, MinBoundary = 11.131});

            outputs.Add(new FaiItem("12_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 18.805, MinBoundary = 18.705});
            outputs.Add(new FaiItem("12_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 18.805, MinBoundary = 18.705});

            outputs.Add(new FaiItem("16_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 29.599, MinBoundary = 29.499});
            outputs.Add(new FaiItem("16_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 29.599, MinBoundary = 29.499});

            outputs.Add(new FaiItem("17_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 17.154, MinBoundary = 17.054});
            outputs.Add(new FaiItem("17_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 17.154, MinBoundary = 17.054});

            outputs.Add(new FaiItem("19_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 1.658, MinBoundary = 1.558});
            outputs.Add(new FaiItem("19_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 1.658, MinBoundary = 1.558});

            outputs.Add(new FaiItem("20_1")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 19.353, MinBoundary = 19.253});
            outputs.Add(new FaiItem("20_2")
                {SerializationDir = FaiItemSerializationDir, MaxBoundary = 0.06, MinBoundary = 0});

            return outputs;
        }


        private List<FindLineLocation> FindLineLocationHardCodeValues()
        {
            return new List<FindLineLocation>()
            {
                new FindLineLocation()
                {
                    Name = "2-left", Angle = 90, X = 694, Y = 2017.3, Len2 = 218, ImageIndex = 0
                },

                new FindLineLocation()
                {
                    Name = "2-right", Angle = 90, X = 1712, Y = 2017, Len2 = 218, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "3-left", Angle = -90, X = 1759, Y = 2077, Len2 = 274, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "3-right", Angle = -90, X = 679, Y = 2077, Len2 = 274, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "4", Angle = -90, X = 1224, Y = 4562, Len2 = 1024, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "5-1", Angle = -90, X = 2030, Y = 4724, Len2 = 250, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "5-2", Angle = -90, X = 1166, Y = 4724, Len2 = 210, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "5-3", Angle = -90, X = 366, Y = 4724, Len2 = 210, ImageIndex = 0
                },
                // upper left h line
                new FindLineLocation()
                {
                    Name = "9", Angle = 90, X = 3171, Y = 1466, Len2 = 317, ImageIndex = 0
                },
                // lower left h line
                new FindLineLocation()
                {
                    Name = "6", Angle = -90, X = 3162, Y = 2895.5, Len2 = 310, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "12-1", Angle = 180, X = 2451, Y = 3149, Len2 = 95, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "12-2", Angle = 180, X = 2451, Y = 3767, Len2 = 95, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "12-3", Angle = 180, X = 2451, Y = 4300, Len2 = 130, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "17-1", Angle = 180, X = 2241, Y = 326, Len2 = 95, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "17-2", Angle = 180, X = 2241, Y = 736, Len2 = 95, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "17-3", Angle = 180, X = 2241, Y = 1149, Len2 = 95, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Angle = 180,
                    Name = "16", ImageIndex = 0, Len2 = 366, X = 3870.5, Y = 2147.5
                },
                new FindLineLocation()
                {
                    Name = "19", Angle = 0, X = 206.5, Y = 705, Len2 = 375, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "20.bottomLeft", Angle = -135, X = 2094, Y = 1937, Len2 = 85, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "20.topRight", Angle = -135, X = 348.5, Y = 134, Len2 = 73, ImageIndex = 0
                }
            };
        }

        private ObservableCollection<FindLineParam> FindLineParamsHardCodeValues()
        {
            var outputs = new ObservableCollection<FindLineParam>()
            {
                new FindLineParam()
                {
                    Name = "2-left", UsingPair = true, Polarity = FindLinePolarity.Positive, Threshold = 3,
                    MinWidth = 1, MaxWidth = 10, FirstAttemptOnly = true, WhichEdge = EdgeSelection.Last
                },
                new FindLineParam()
                {
                    Name = "2-right", UsingPair = true, Polarity = FindLinePolarity.Positive, Threshold = 3,
                    MinWidth = 1, MaxWidth = 10, FirstAttemptOnly = true, WhichEdge = EdgeSelection.Last
                },
                new FindLineParam()
                {
                    Name = "3-left", UsingPair = false, Polarity = FindLinePolarity.Positive, Threshold = 10,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "3-right", UsingPair = false, Polarity = FindLinePolarity.Positive, Threshold = 10,
                    FirstAttemptOnly = true
                },

                new FindLineParam()
                {
                    Name = "4", Polarity = FindLinePolarity.Negative, Threshold = 10, FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "5-1", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "5-2", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "5-3", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "6", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15
                },
                new FindLineParam()
                {
                    Name = "9", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15
                },
                new FindLineParam()
                {
                    Name = "12-1", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "12-2", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "12-3", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "17-1", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "17-2", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },
                new FindLineParam()
                {
                    Name = "17-3", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                    FirstAttemptOnly = true
                },


                new FindLineParam()
                {
                    Name = "16", UsingPair = false, Polarity = FindLinePolarity.Negative, Threshold = 10
                },
                new FindLineParam()
                {
                    Name = "19", Polarity = FindLinePolarity.Negative, Threshold = 10, CannyHigh = 30, CannyLow = 15,
                },
                new FindLineParam()
                {
                    Name = "20.bottomLeft", Polarity = FindLinePolarity.Negative, Threshold = 10, NewWidth = 3
                },
                new FindLineParam()
                {
                    Name = "20.topRight", Polarity = FindLinePolarity.Positive, Threshold = 10, NewWidth = 3
                }
            };

            foreach (var param in outputs)
            {
                param.SerializationDir = ParamSerializationBaseDir;
            }

            return outputs;
        }

        #endregion

        #region Image Providing Logic

        /// <summary>
        /// Provide next image
        /// <exception cref="InvalidDataException">When images are all consumed</exception>
        /// </summary>
        public List<HImage> ImageInputs {
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
            }}

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
        public int NumImages => ImageQueues[0]?.Count ?? 0;


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
                        var imageIndexString = imageName.Substring(imageName.IndexOf("_", StringComparison.Ordinal) + 1, imageName.Length); 
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
            var nameToTest = imagePaths[0];
            
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