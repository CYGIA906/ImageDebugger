using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HalconDotNet;
using ImageDebugger.Core.Commands;
using ImageDebugger.Core.IoC.Interface;
using MaterialDesignThemes.Wpf;
using PropertyChanged;

namespace ImageDebugger.Core.ViewModels.Base
{
    /// <summary>
    /// Base class for measurement view models
    /// that provides basic image playback functionality
    /// </summary>
    public class MeasurementPlayerViewModelBase : RecyclableMegaList<string>
    {
           /// <summary>
        /// Display current gray value and image x, y coordinates
        /// </summary>
        /// <param name="o"></param>
           protected void DisplayXYGray(object o)
        {
            double grayvalue = -1;
            int imageX = -1, imageY = -1;
            ShouldImageInfoDisplay = true;
            HSmartWindowControlWPF.HMouseEventArgsWPF e = (HSmartWindowControlWPF.HMouseEventArgsWPF) o;
            try
            {
                imageX = (int) e.Column;
                imageY = (int) e.Row;
                grayvalue = (DisplayImage as HImage)?.GetGrayval(imageY, imageX);
            }
            catch (Exception exception)
            {
                ShouldImageInfoDisplay = false;
            }

            // output grayvalue info
            GrayValueInfo = new ImageInfoViewModel()
            {
                X = imageX, Y = imageY, GrayValue = (int) grayvalue
            };

            // Set the pop-up position
            MouseX = (int) e.X + 20;
            MouseY = (int) e.Y + 20;
        }

           [DoNotNotify]
           public List<HImage> ImageInputs { get; set; }
           
        public HImage DisplayImage => ImageInputs == null || ImageInputs.Count <= IndexToShow ? null : ImageInputs[IndexToShow];

        /// <summary>
        /// Specifies whether image processing is continuously running
        /// </summary>
        public bool IsContinuouslyRunning { get; set; }

        /// <summary>
        /// The underlying list for the UI the choose which image within the input batch
        /// should be shown
        /// </summary>
        public List<int> ImageToShowSelectionList { get; private set; } = new List<int>();

        /// <summary>
        /// The index within the input batch of images to show
        /// </summary>
        public int IndexToShow { get; set; } = 0;

        /// <summary>
        /// X coordinate of the displayed image info view
        /// </summary>
        public int MouseX { get; set; }

        /// <summary>
        /// Y coordinate of the displayed image info view
        /// </summary>
        public int MouseY { get; set; }

        /// <summary>
        /// Specifies whether image processing is running
        /// </summary>
        public bool SystemIsBusy { get; set; }

        /// <summary>
        /// Gray value information of the current image point under the cursor
        /// </summary>
        public ImageInfoViewModel GrayValueInfo { get; set; }

        /// <summary>
        /// The message queue for outputting debugging information to user
        /// </summary>
        public ISnackbarMessageQueue RunStatusMessageQueue { get; set; } =
            new SnackbarMessageQueue(TimeSpan.FromMilliseconds(5000));

        /// <summary>
        /// The command to run next batch of images
        /// </summary>
        public ICommand RunNextCommand { get;protected set; }

        /// <summary>
        /// The command to run previous batch of images
        /// </summary>
        public ICommand RunPreviousCommand { get; protected set; }

        /// <summary>
        /// The command to continuously run all the images from the current index
        /// </summary>
        public ICommand ContinuousRunCommand { get; protected set; }

        /// <summary>
        /// Command to display X,Y and gray value information of the current shown image
        /// </summary>
        public ICommand DisplayXYGrayCommand { get; protected set; }

        /// <summary>
        /// Whether the image info should be displayed
        /// </summary>
        public bool ShouldImageInfoDisplay { get; set; }

        /// <summary>
        /// The window handle to display debugging graphics to the halcon window
        /// </summary>
        public HWindow WindowHandle { get; set; }

        /// <summary>
        /// The image names for the combo box to run selected image to choose from
        /// </summary>
        public List<string> ImageNames { get; set; }

        /// <summary>
        /// Current processed image name to show
        /// </summary>
        public string CurrentImageName { get; protected set; }

        /// <summary>
        /// The command to select a specific image and run image processing
        /// </summary>
        public ICommand ImageNameSelectionChangedCommand { get; set; }

        /// <summary>
        /// Log information to the UI
        /// </summary>
        /// <param name="message"></param>
        protected void PromptUserThreadSafe(string message)
        {
            RunStatusMessageQueue?.Enqueue(message);
        }

        /// <summary>
        /// Get the image name without directory
        /// </summary>
        /// <param name="imagePath">Full path to the image</param>
        /// <returns></returns>
        private string GetImageName(string imagePath)
        {
            return Path.GetFileName(imagePath);
        }


        private List<string> _imagePaths;


        private List<string> ImagePaths
        {
            get => _imagePaths;
            set
            {

                if (value.Count == 0)
                {
                    PromptUserThreadSafe("This folder does not contains any supported images");
                    return;
                }                
                
                bool updateImageListsSuccess = TryAssignImageLists(value);
                if (!updateImageListsSuccess) return;
                
                // Generate image names
                ImageNames = GenImageNames();
                ImageToShowSelectionList = GenImageToShowSelectionList(NumLists);
                _imagePaths = value;
            }
        }

        /// <summary>
        /// Generate a list of image index to show
        /// The size of the list equals to the size of input images
        /// required for each image processing run
        /// </summary>
        /// <param name="numImagesOneGo"></param>
        /// <returns></returns>
        private List<int> GenImageToShowSelectionList(int numImagesOneGo)
        {
            var output = new List<int>();
            for (int i = 0; i < numImagesOneGo; i++)
            {
                output.Add(i);
            }

            return output;
        }

        /// <summary>
        /// Generate the image names to append in the combo box
        /// which can be used to selected an image and run image processing
        /// </summary>
        /// <returns>The generated image names</returns>
        private List<string> GenImageNames()
        {
            var output = new List<string>();
            foreach (var paths in this)
            {
                output.Add(GetImageName(paths[0]));
            }

            return output;
        }

        /// <summary>
        /// The separator to split image name from major index and minor index
        /// One major index corresponds to one image processing run
        /// One minor index corresponds to the order feed into the image processing within each run
        /// </summary>
        private string Separator { get; set; } = "-";

        /// <summary>
        /// Assign and return true only if
        /// all named correctly and all lists have the same count
        /// </summary>
        /// <param name="imagePaths">Paths to the images</param>
        /// <returns>Whether the assignment to the mega image list is successful</returns>
        private bool TryAssignImageLists(List<string> imagePaths)
        {
            int numImagesInOneGo = GetNumImagesInOneGo(imagePaths);

            if (numImagesInOneGo != NumImagesInOneGoRequired)
            {
                PromptUserThreadSafe("Incorrect number of input images, check the image directory!");
                return false;
            }
            
            List<List<string>> tempMegaList = MakeTempMegaList(numImagesInOneGo);


            foreach (var path in imagePaths)
            {
                int imageIndex = 0;

                if (numImagesInOneGo > 1)
                {
                    var imageName = Path.GetFileName(path);
                    var start = imageName.IndexOf(Separator, StringComparison.Ordinal) + 1;
                    var length = 1;
                    var imageIndexString = imageName.Substring(start, length);
                    try
                    {
                        imageIndex = int.Parse(imageIndexString) - 1;
                    }
                    catch (Exception e)
                    {
                        PromptUserThreadSafe($"Incorrect image name: {imageName}");
                        return false;
                    }
                }

                tempMegaList[imageIndex].Add(path);
            }

            var numImagesInFirstList = tempMegaList[0].Count;
            if (tempMegaList.Any(l => l.Count != numImagesInFirstList))
            {
                PromptUserThreadSafe("Count of image lists not equal");
                return false;
            }

            var sortedImageMegaList = new List<List<string>>();
            foreach (var queue in tempMegaList)
            {
                var orderedQueue = new List<string>(queue.OrderBy(Path.GetFileName));
                sortedImageMegaList.Add(orderedQueue);
            }

            Reconstruct(sortedImageMegaList);
            return true;
        }

        /// <summary>
        /// How many images are required for a click of run button
        /// </summary>
        protected virtual int NumImagesInOneGoRequired { get; set; }

        /// <summary>
        /// Make a temporary list for storing image paths before assigning to
        /// the internal image mega list
        /// </summary>
        /// <param name="numImagesInOneGo">How many images are required for each image processing</param>
        /// <returns></returns>
        private List<List<string>> MakeTempMegaList(int numImagesInOneGo)
        {
            var output = new List<List<string>>();
            for (int i = 0; i < numImagesInOneGo; i++)
            {
                output.Add(new List<string>());
            }

            return output;
        }

        /// <summary>
        /// Convert image paths to <see cref="HImage"/>
        /// </summary>
        /// <param name="imagePaths">Paths to images</param>
        /// <returns></returns>
        private List<HImage> ConvertPathsToImages(List<string> imagePaths)
        {
            var output = new List<HImage>();
            foreach (var path in imagePaths)
            {
                output.Add(new HImage(path));
            }

            return output;
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
        /// Time span measured for each run of image processing
        /// </summary>
        public string TimeElapsed { get; set; }

        /// <summary>
        /// Command to open a image directory and assign mega image list
        /// </summary>
        public ICommand SelectImageDirCommand { get; protected set; }

        /// <summary>
        /// Specifies whether there are images available for image processing 
        /// </summary>
        public bool HasImages => Count > 0;

        public MeasurementPlayerViewModelBase()
        {
            // Update current image name to show
            CurrentIndexChanged += index => { CurrentImageName = index < 0 ? "" : ImageNames[index]; };
            
            // Commands
            SelectImageDirCommand = new SimpleCommand(o => { ImagePaths = IoC.IoC.Get<IImageProvider>().GetImages(); },
                o => !SystemIsBusy);
            
            ImageNameSelectionChangedCommand = new SimpleCommand( async o=>
            {
                int index = (int) o;
                ImageInputs = ConvertPathsToImages(JumpTo(index));
                await RunOnlySingleFireIsAllowedEachTimeCommand(() => SystemIsBusy,
                    async () => await TimedProcessAsync(ImageInputs));

            }, o => HasImages);
            
            // Prompt user when the current index overflows
            UpperIndexExceeded += () => PromptUserThreadSafe("Reached the end of list, start over");
            // Prompt user when the current index jumps to end of list
            LowerIndexExceeded += () => PromptUserThreadSafe("Jump to the end of image list");

            RunNextCommand = new SimpleCommand(async o =>
            {
                if (SystemIsBusy) return;
                ImageInputs = ConvertPathsToImages(NextUnbounded());
                await RunOnlySingleFireIsAllowedEachTimeCommand(() => SystemIsBusy,
                    async () => { await TimedProcessAsync(ImageInputs); });
            }, o => HasImages);

            RunPreviousCommand = new SimpleCommand(async o =>
            {
                if (SystemIsBusy) return;
                ImageInputs = ConvertPathsToImages(PreviousUnbounded());
                await RunOnlySingleFireIsAllowedEachTimeCommand(() => SystemIsBusy,
                    async () => { await TimedProcessAsync(ImageInputs); });
            }, o => HasImages);

       

            ContinuousRunCommand = new SimpleCommand(async o =>
            {
                if (SystemIsBusy) return;

                await RunOnlySingleFireIsAllowedEachTimeCommand(() => SystemIsBusy, async () =>
                    {
                        while (IsContinuouslyRunning)
                        {
                            var inputPaths = NextBounded();
                            if (inputPaths == null) break;
                            ImageInputs = ConvertPathsToImages(inputPaths);

                            await TimedProcessAsync(ImageInputs);
                        }
                    })
                    ;

                IsContinuouslyRunning = false;
            }, o => HasImages);

      
            
            DisplayXYGrayCommand = new SimpleCommand(DisplayXYGray, o=> DisplayImage!=null);

        }

             /// <summary>
             /// Do timing for a single image process run
             /// </summary>
             /// <param name="images">A batch of input images</param>
             /// <returns></returns>
             private async Task TimedProcessAsync(List<HImage> images)
             {
                 Stopwatch stopwatch = new Stopwatch();
                 stopwatch.Start();
     
                  await OnImageProcessStartAsync(images);
     
                 stopwatch.Stop();
                 TimeElapsed = stopwatch.ElapsedMilliseconds.ToString();
             }

             protected event Func<List<HImage>, Task> ImageProcessStartAsync;

             private async Task OnImageProcessStartAsync(List<HImage> obj)
             {
                 if (ImageProcessStartAsync != null) await ImageProcessStartAsync?.Invoke(obj);
             }
    }
}