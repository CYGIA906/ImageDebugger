using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HalconDotNet;
using ImageDebugger.Core.Commands;
using ImageDebugger.Core.Helpers;
using ImageDebugger.Core.ImageProcessing;
using ImageDebugger.Core.ImageProcessing.Utilts;
using ImageDebugger.Core.IoC.Interface;
using ImageDebugger.Core.Models;
using ImageDebugger.Core.ViewModels.Application;
using ImageDebugger.Core.ViewModels.Base;
using MaterialDesignThemes.Wpf;

namespace ImageDebugger.Core.ViewModels.CameraMeasurement
{
    public class CameraMeasurementViewModel : MeasurementPlayerViewModelBase
    {
        /// <summary>
        /// The measurement procedure class that image processing is going to happen
        /// </summary>
        private IMeasurementProcedure _measurementUnit;

        /// <summary>
        /// The fai item list for displaying result to the UI
        /// </summary>
        public List<FaiItem> FaiItems { get; private set; }

        /// <summary>
        /// The find line parameters to display for editing
        /// and will be apply to tweak haw the lines will be found
        /// </summary>
        public List<FindLineParam> FindLineParams { get; private set; }


        /// <summary>
        /// Name of the image processing procedure
        /// </summary>
        public string ProcedureName
        {
            get { return MeasurementUnit == null ? "" : MeasurementUnit.Name; }
        }


        /// <summary>
        /// The measurement procedure that image processing happens
        /// </summary>
        public IMeasurementProcedure MeasurementUnit
        {
            get { return _measurementUnit; }
            set
            {
                _measurementUnit = value;
                CsvSerializer = new CsvSerializer(CsvDir);
                // Init fai items
                FaiItems = LoadFaiItemsFromDisk().ToList();


                // Init find line params
                FindLineParams = LoadFindLineParamsFromDisk().ToList();


                // Init find line locations
                FindLineLocationsRelativeValues = MeasurementUnit.GenFindLineLocationValues();
            }
        }


        /// <summary>
        /// The directory for outputting find-line parameter config files
        /// </summary>
        public string ParamSerializationBaseDir
        {
            get { return SerializationDir + "/FindLineParams"; }
        }


        /// <summary>
        /// Update fai items to display after image processing
        /// </summary>
        /// <param name="results">Results return from image processing</param>
        private void UpdateFaiItems(Dictionary<string, double> results)
        {
            FaiItems.StopAutoSerializing();

            foreach (var item in FaiItems)
            {
                item.ValueUnbiased = results[item.Name];
            }
            
            FaiItems.StartAutoSerializing();
        }

        /// <summary>
        /// The directory to output measurement values
        /// as well as some debugging information specific to image processing 
        /// </summary>
        public string CsvDir
        {
            get { return SerializationDir + "/CSV"; }
        }


        /// <summary>
        /// The serializer that manages all the serialization logic for serializing fai items
        /// </summary>
        public CsvSerializer CsvSerializer { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public CameraMeasurementViewModel()
        {
            ImageProcessStartAsync += ProcessAsync;
        }

        protected override int NumImagesInOneGoRequired
        {
            get { return MeasurementUnit.NumImagesInOneGoRequired; }
        }


        /// <summary>
        /// Find line locations that are relative to the selected origin
        /// These locations will not change once from run to run
        /// </summary>
        public List<FindLineLocation> FindLineLocationsRelativeValues { get; set; }

        /// <summary>
        /// Process a batch of images and handle feedback
        /// </summary>
        /// <param name="images">A batch of input images</param>
        /// <returns></returns>
        private async Task ProcessAsync(List<HImage> images)
        {
            var findLineConfigs = new FindLineConfigs(FindLineParams.ToList(), FindLineLocationsRelativeValues);

            var result =
                await Task.Run(() =>
                    MeasurementUnit.ProcessAsync(images, findLineConfigs, FaiItems, IndexToShow,
                        RunStatusMessageQueue));


            InfoImage.DispImage(WindowHandle);

            if (WindowHandle != null)
            {
                result.HalconGraphics.DisplayGraphics(WindowHandle);
                result.DataRecorder.DisplayPoints(WindowHandle);
            }

            result.DataRecorder.Serialize(CsvDir + "/DebuggingData.csv");
            UpdateFaiItems(result.FaiDictionary);
            CsvSerializer.Serialize(FaiItems, ImageNames[CurrentIndex]);
        }
        

        /// <summary>
        /// The base directory for serializing everything
        /// </summary>
        public string SerializationDir
        {
            get { return ApplicationViewModel.SolutionDirectory + "/Configs/2D/" + ProcedureName; }
        }

        /// <summary>
        /// The directory to serialize fai item settings
        /// </summary>
        public string FaiItemSerializationDir
        {
            get { return SerializationDir + "/FaiItems"; }
        }

        /// <summary>
        /// Try to load fai item setting from disk if any
        /// </summary>
        /// <returns>The loaded fai items</returns>
        private IEnumerable<FaiItem> LoadFaiItemsFromDisk()
        {
            var itemNames = MeasurementUnit.GenFaiItemValues("").Select(item => item.Name);
            return AutoSerializableHelper.LoadAutoSerializables<FaiItem>(itemNames, FaiItemSerializationDir);
        }

        /// <summary>
        /// Try to load find-line parameters from disk if any
        /// </summary>
        /// <returns>The loaded find-line parameters</returns>
        private IEnumerable<FindLineParam> LoadFindLineParamsFromDisk()
        {
            var paramNames = MeasurementUnit.GenFindLineParamValues("").Select(item => item.Name);
            return AutoSerializableHelper.LoadAutoSerializables<FindLineParam>(paramNames, ParamSerializationBaseDir);
        }
    }
}