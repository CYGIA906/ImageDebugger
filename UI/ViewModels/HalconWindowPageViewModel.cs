using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using HalconDotNet;
using UI.Commands;
using UI.ImageProcessing;

// TODO: Finish displaying part

namespace UI.ViewModels
{
    public class HalconWindowPageViewModel : ViewModelBase, IMeasurementProcedure
    {
        public ObservableCollection<FaiItem> FaiItems { get; }

        private HWindow _windowHandle;

        /// <summary>
        /// Path to the shape model in disk
        /// </summary>
        public string ModelPath
        {
            get => _modelPath;
            set
            {
                _modelPath = value;
                HOperatorSet.ReadShapeModel(_modelPath, out _shapeModelHandle);
            }
        }

        public ICommand ExecuteCommand { get; }

        HDevelopExport _halconScript = new HDevelopExport();
        private HTuple _shapeModelHandle;
        private double[] _thresholds = { 128, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 };
        private string _modelPath;

        public void Process(HImage image)
        {
            if(!image.IsInitialized()) return;

            ShowImageAndGraphics(image, new HObject());
            

        }

        private void ShowImageAndGraphics(HImage image, HObject graphics)
        {
            HOperatorSet.ClearWindow(_windowHandle);
            image.DispObj(_windowHandle);
            if(graphics.IsInitialized()) graphics.DispObj(_windowHandle);
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
            if(numItems != numValues)
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
            FaiItems.Add(new FaiItem("FAI2") { MaxBoundary = 15.49, MinBoundary = 15.390});

            FaiItems.Add(new FaiItem("FAI3") { MaxBoundary = 15.906, MinBoundary = 15.806 });

            FaiItems.Add(new FaiItem("FAI4.1") {  MaxBoundary = 34.92, MinBoundary = 34.82 });
            FaiItems.Add(new FaiItem("FAI4.2") {  MaxBoundary = 34.92, MinBoundary = 34.82 });
            FaiItems.Add(new FaiItem("FAI4.3") {  MaxBoundary = 34.92, MinBoundary = 34.82 });

            FaiItems.Add(new FaiItem("FAI5.1") {  MaxBoundary = 36.15, MinBoundary = 36.05 });
            FaiItems.Add(new FaiItem("FAI5.2") {  MaxBoundary = 36.15, MinBoundary = 36.05 });
            FaiItems.Add(new FaiItem("FAI5.3") {  MaxBoundary = 36.15, MinBoundary = 36.05 });

            FaiItems.Add(new FaiItem("FAI6.1") {MaxBoundary = 22.153, MinBoundary = 22.053 });
            FaiItems.Add(new FaiItem("FAI6.2") {  MaxBoundary = 22.153, MinBoundary = 22.053 });
            FaiItems.Add(new FaiItem("FAI6.3") {  MaxBoundary = 22.153, MinBoundary = 22.053 });

            FaiItems.Add(new FaiItem("FAI9.1") {  MaxBoundary = 11.231, MinBoundary = 11.131 });
            FaiItems.Add(new FaiItem("FAI9.2") {  MaxBoundary = 11.231, MinBoundary = 11.131 });
            FaiItems.Add(new FaiItem("FAI9.3") {  MaxBoundary = 11.231, MinBoundary = 11.131 });

            FaiItems.Add(new FaiItem("FAI12.1") {  MaxBoundary = 18.805, MinBoundary = 18.705 });
            FaiItems.Add(new FaiItem("FAI12.2") {  MaxBoundary = 18.805, MinBoundary = 18.705 });
            FaiItems.Add(new FaiItem("FAI12.3") {  MaxBoundary = 18.805, MinBoundary = 18.705 });

            FaiItems.Add(new FaiItem("FAI16.1") {  MaxBoundary = 29.599, MinBoundary = 29.499});
            FaiItems.Add(new FaiItem("FAI16.2") {  MaxBoundary = 29.599, MinBoundary = 29.499});
            FaiItems.Add(new FaiItem("FAI16.3") {  MaxBoundary = 29.599, MinBoundary = 29.499});

            FaiItems.Add(new FaiItem("FAI17.1") {  MaxBoundary = 17.154, MinBoundary = 17.054});
            FaiItems.Add(new FaiItem("FAI17.2") {  MaxBoundary = 17.154, MinBoundary = 17.054});
            FaiItems.Add(new FaiItem("FAI17.3") {  MaxBoundary = 17.154, MinBoundary = 17.054});

            FaiItems.Add(new FaiItem("FAI19.1") {  MaxBoundary = 1.658, MinBoundary = 1.558});
            FaiItems.Add(new FaiItem("FAI19.2") {  MaxBoundary = 1.658, MinBoundary = 1.558});
            FaiItems.Add(new FaiItem("FAI19.3") {  MaxBoundary = 1.658, MinBoundary = 1.558});

            FaiItems.Add(new FaiItem("FAI20.1") {  MaxBoundary = 19.353, MinBoundary = 19.253});
            FaiItems.Add(new FaiItem("FAI20.2") {  MaxBoundary = 19.353, MinBoundary = 19.253});


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
                    return;
                }

                var image = new HImage(imagePath);
                Process(image);
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

            //TODO: make this elegant
            ModelPath = @"C:\Users\afterbunny\Documents\Projects\Hdevs\ModelTopViewI94";
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
        private static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".TIF", ".PNG" };

        private string _imageDirectory;

        /// <summary>
        /// Directory to images 
        /// </summary>
        public string ImageDirectory
        {
            get => _imageDirectory;
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