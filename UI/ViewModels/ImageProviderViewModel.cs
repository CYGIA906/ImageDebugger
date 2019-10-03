using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using HalconDotNet;
using UI.Commands;
using UI.Interfaces;

namespace UI.ViewModels
{
    public class ImageProviderViewModel : ViewModelBase, IImageProvider
    {
        /// <summary>
        /// Provide next image
        /// <exception cref="InvalidDataException">When images are all consumed</exception>
        /// </summary>
        public HImage NextImage
        {
            get
            {
                if(_imageDirectory == null) return new HImage();

                var imagePath = _imagePaths.Dequeue();
                return new HImage(imagePath);
            }
        }

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


        public ImageProviderViewModel()
        {
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
    }
}