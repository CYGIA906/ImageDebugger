using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using HalconDotNet;
using UI.Commands;
using UI.ImageProcessing;


namespace UI.ViewModels
{
    public class HalconWindowPageViewModel : ViewModelBase
    {
        public ObservableCollection<FaiItem> FaiItems { get; }

        private HWindow _windowHandle;

        private FindLineConfigs _findLineConfigs;

        private IMeasurementProcedure measurementUnit = new I94TopViewMeasure();

        public ICommand ExecuteCommand { get; }


        public void Process(List<HImage> images)
        {
            measurementUnit.Process(images, _findLineConfigs, _windowHandle);

//            ShowImageAndGraphics(images[0], new HObject());
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

            FaiItems = new ObservableCollection<FaiItem>();
            FaiItems.Add(new FaiItem("FAI2") {MaxBoundary = 15.49, MinBoundary = 15.390});

            FaiItems.Add(new FaiItem("FAI3") {MaxBoundary = 15.906, MinBoundary = 15.806});

            FaiItems.Add(new FaiItem("FAI4.1") {MaxBoundary = 34.92, MinBoundary = 34.82});
            FaiItems.Add(new FaiItem("FAI4.2") {MaxBoundary = 34.92, MinBoundary = 34.82});
            FaiItems.Add(new FaiItem("FAI4.3") {MaxBoundary = 34.92, MinBoundary = 34.82});

            FaiItems.Add(new FaiItem("FAI5.1") {MaxBoundary = 36.15, MinBoundary = 36.05});
            FaiItems.Add(new FaiItem("FAI5.2") {MaxBoundary = 36.15, MinBoundary = 36.05});
            FaiItems.Add(new FaiItem("FAI5.3") {MaxBoundary = 36.15, MinBoundary = 36.05});

            FaiItems.Add(new FaiItem("FAI6.1") {MaxBoundary = 22.153, MinBoundary = 22.053});
            FaiItems.Add(new FaiItem("FAI6.2") {MaxBoundary = 22.153, MinBoundary = 22.053});
            FaiItems.Add(new FaiItem("FAI6.3") {MaxBoundary = 22.153, MinBoundary = 22.053});

            FaiItems.Add(new FaiItem("FAI9.1") {MaxBoundary = 11.231, MinBoundary = 11.131});
            FaiItems.Add(new FaiItem("FAI9.2") {MaxBoundary = 11.231, MinBoundary = 11.131});
            FaiItems.Add(new FaiItem("FAI9.3") {MaxBoundary = 11.231, MinBoundary = 11.131});

            FaiItems.Add(new FaiItem("FAI12.1") {MaxBoundary = 18.805, MinBoundary = 18.705});
            FaiItems.Add(new FaiItem("FAI12.2") {MaxBoundary = 18.805, MinBoundary = 18.705});
            FaiItems.Add(new FaiItem("FAI12.3") {MaxBoundary = 18.805, MinBoundary = 18.705});

            FaiItems.Add(new FaiItem("FAI16.1") {MaxBoundary = 29.599, MinBoundary = 29.499});
            FaiItems.Add(new FaiItem("FAI16.2") {MaxBoundary = 29.599, MinBoundary = 29.499});
            FaiItems.Add(new FaiItem("FAI16.3") {MaxBoundary = 29.599, MinBoundary = 29.499});

            FaiItems.Add(new FaiItem("FAI17.1") {MaxBoundary = 17.154, MinBoundary = 17.054});
            FaiItems.Add(new FaiItem("FAI17.2") {MaxBoundary = 17.154, MinBoundary = 17.054});
            FaiItems.Add(new FaiItem("FAI17.3") {MaxBoundary = 17.154, MinBoundary = 17.054});

            FaiItems.Add(new FaiItem("FAI19.1") {MaxBoundary = 1.658, MinBoundary = 1.558});
            FaiItems.Add(new FaiItem("FAI19.2") {MaxBoundary = 1.658, MinBoundary = 1.558});
            FaiItems.Add(new FaiItem("FAI19.3") {MaxBoundary = 1.658, MinBoundary = 1.558});

            FaiItems.Add(new FaiItem("FAI20.1") {MaxBoundary = 19.353, MinBoundary = 19.253});
            FaiItems.Add(new FaiItem("FAI20.2") {MaxBoundary = 19.353, MinBoundary = 19.253});


            // Init commands
            ExecuteCommand = new RelayCommand(() =>
            {
                string imagePath;
                try
                {
                    imagePath = _imagePaths.Dequeue();
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show("Images all gone.");
//                    return;
                }

                var image = new HImage();
                Process(new List<HImage>() {image});
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

            InitFindLineConfigs();
        }

        private void InitFindLineConfigs()
        {
            List<FindLineParam> findLineParams = GenFindLineParams();
            List<FindLineLocation> findLineLocations = GenFindLineLocations();
            _findLineConfigs = new FindLineConfigs(findLineParams, findLineLocations);
        }

        private List<FindLineLocation> GenFindLineLocations()
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

        private List<FindLineParam> GenFindLineParams()
        {
            return new List<FindLineParam>()
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
        }


        #region Image Providing Logic

        /// <summary>
        /// Provide next image
        /// <exception cref="InvalidDataException">When images are all consumed</exception>
        /// </summary>
        public string NextImage => NumImages == 0 ? string.Empty : _imagePaths.Dequeue();


        /// <summary>
        /// The number of images available
        /// </summary>
        public int NumImages => _imagePaths.Count;

        /// <summary>
        /// Image paths to be read
        /// </summary>
        private Queue<string> _imagePaths = new Queue<string>();

        /// <summary>
        /// Known list of image extensions to filter non-image files
        /// </summary>
        private static readonly List<string> ImageExtensions = new List<string>
            {".JPG", ".JPE", ".BMP", ".TIF", ".PNG"};

        private string _imageDirectory;

        /// <summary>
        /// Directory to images 
        /// </summary>
        public string ImageDirectory
        {
            get { return _imageDirectory; }
            set
            {
                _imageDirectory = value;
                string[] imagePaths = Directory.GetFiles(_imageDirectory);

                foreach (var imagePath in imagePaths)
                {
                    if (IsImageFile(imagePath))
                    {
                        _imagePaths.Enqueue(imagePath);
                    }
                }
            }
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