using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using cyXYZInspector;
using HalconDotNet;
using ImageDebugger.Core.Commands;
using ImageDebugger.Core.Enums;
using ImageDebugger.Core.Helpers;
using ImageDebugger.Core.ImageProcessing.LineScan;
using ImageDebugger.Core.ImageProcessing.LineScan.Procedure;
using ImageDebugger.Core.ImageProcessing.Utilts;
using ImageDebugger.Core.ViewModels.Application;
using ImageDebugger.Core.ViewModels.Base;
using ImageDebugger.Core.ViewModels.LineScan.Flatness;
using ImageDebugger.Core.ViewModels.LineScan.Parallelism;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;
using ImageDebugger.Core.ViewModels.LineScan.Thickness;
using MathNet.Numerics.LinearAlgebra;
using PropertyChanged;

namespace ImageDebugger.Core.ViewModels.LineScan
{
    public class LineScanMeasurementViewModel : MeasurementPlayerViewModelBase
    {
        private bool _currentViewIsBackView;
        private ILineScanMeasurementProcedure LineScanMeasurementProcedure { get; set; } = new I40LineScanMeasurement();

        /// <summary>
        /// Point settings
        /// </summary>
        public List<PointSettingViewModel> PointSettingViewModels { get; set; }


        public string ProcedureName
        {
            get { return LineScanMeasurementProcedure.Name; }
        }


        public HWindow WindowHandleBottomRight { get; set; }

        public HWindow WindowHandleLeftRight { get; set; }
        public List<FlatnessItemViewModel> FlatnessViewModels { get; set; }


        public List<ParallelismItemViewModel> ParallelismItemViewModels { get; set; }

        public List<ThicknessItemViewModel> ThicknessViewModels { get; set; }


        /// <summary>
        /// Specifies whether the currently showing image is back-view or front-view
        /// </summary>
        public bool CurrentViewIsBackView
        {
            get { return _currentViewIsBackView; }
            set
            {
                _currentViewIsBackView = value;
                if (_currentViewIsBackView) ToggleBackView();
                else ToggleFrontView();
            }
        }

        private void ToggleFrontView()
        {
            WindowHandle.DispColor(FrontView);
            Result.Display(WindowHandle);
        }

        private void ToggleBackView()
        {
            WindowHandle.DispColor(BackView);
            Result.Display(WindowHandle);
        }


        /// <summary>
        /// Root directory for serialization
        /// </summary>
        private string ConfigurationBaseDir
        {
            get
            {
                return Path.Combine(ApplicationViewModel.SolutionDirectory + "/Configs/3D/",
                    LineScanMeasurementProcedure.Name);
            }
        }

        private static string CsvDir
        {
            get { return Directory.GetCurrentDirectory() + "/CSV"; }
        }

        /// <summary>
        /// Serialize measurement results
        /// </summary>
        private CsvSerializer CsvSerializer { get; }

        /// <summary>
        /// The content to show in the drawer that sits on the right
        /// </summary>
        public DrawerContentType3D DrawerContent { get; set; } = DrawerContentType3D.PointSettings;

        private string PointSettingSerializationDir
        {
            get { return Path.Combine(ConfigurationBaseDir, "Points"); }
        }

        public override int IndexToShow { get; set; }


        protected override int NumImagesInOneGoRequired
        {
            get { return LineScanMeasurementProcedure.NumImageRequireInSingleRun; }
        }

        public LineScanMeasurementViewModel()
        {
            PointSettingViewModels =
                AutoSerializableHelper.LoadAutoSerializables<PointSettingViewModel>(
                    LineScanMeasurementProcedure.PointNames, PointSettingSerializationDir).ToList();

            ImageProcessStartAsync += OnImageProcessStartAsync;

            // Commands
            ShowPointSettingViewCommand = new RelayCommand(ShowPointSettingView);
            ShowFlatnessViewCommand = new RelayCommand(ShowFlatnessView);
            ShowParallelismViewCommand = new RelayCommand(ShowParallelismView);
            ShowThicknessViewCommand = new RelayCommand(ShowThicknessView);
            OpenCsvDirCommand = new RelayCommand(OpenCsvDir);

            CsvSerializer = new CsvSerializer(CsvDir);
            ContinuousModeFinished += CsvSerializer.SummariseCsv;
        }

        private void OpenCsvDir()
        {
            if (Directory.Exists(CsvDir)) Process.Start(CsvDir);
        }

        private async Task OnImageProcessStartAsync(List<HImage> images)
        {
            var result = await Task.Run(() =>
                LineScanMeasurementProcedure.Process(ImageInputs, PointSettingViewModels, RunStatusMessageQueue));

            InfoImageList = result.Images.Take(3).ToList();


            BackView = result.Images[3];
            FrontView = result.Images[4];
            Result = result;
            ChangeOfBaseInv = result.ChangeOfBaseInv;

            if (CurrentViewIsBackView) ToggleBackView();
            else ToggleFrontView();

            // Calculate results
            UpdatePointSettings(result.PointMarkers);
            LineScanMeasurementProcedure.ConstructPlanes(PointSettingViewModels);
            ParallelismItemViewModels = LineScanMeasurementProcedure.CalParallelism(PointSettingViewModels);
            FlatnessViewModels = LineScanMeasurementProcedure.CalcFlatness(PointSettingViewModels);
            ThicknessViewModels = LineScanMeasurementProcedure.CalcThickness(PointSettingViewModels);

            // Serialize
            var csvSerializables = new List<ICsvColumnElement>();
            csvSerializables.AddRange(PointSettingViewModels);
            csvSerializables.AddRange(FlatnessViewModels);
            csvSerializables.AddRange(ThicknessViewModels);
            csvSerializables.AddRange(ParallelismItemViewModels);
            csvSerializables.AddRange(result.RecordingElements);
            CsvSerializer.Serialize(csvSerializables, CurrentImageName, IsContinuouslyRunning);
        }


        private ImageProcessingResults3D Result { get; set; }

        private HImage FrontView { get; set; }

        private HImage BackView { get; set; }



        private void UpdatePointSettings(List<PointMarker> pointMarkers)
        {
            PointSettingViewModels.StopAutoSerializing();

            foreach (var pointMarker in pointMarkers)
            {
                PointSettingViewModels.ByName(pointMarker.Name).Value = pointMarker.Height;
            }

            PointSettingViewModels.StartAutoSerializing();
        }

        private void ShowThicknessView()
        {
            DrawerContent = DrawerContentType3D.Thickness;
        }

        private void ShowParallelismView()
        {
            DrawerContent = DrawerContentType3D.Parallelism;
        }

        private void ShowFlatnessView()
        {
            DrawerContent = DrawerContentType3D.Flatness;
        }

        private void ShowPointSettingView()
        {
            DrawerContent = DrawerContentType3D.PointSettings;
        }


        public ICommand ShowPointSettingViewCommand { get; private set; }
        public ICommand ShowFlatnessViewCommand { get; private set; }
        public ICommand ShowParallelismViewCommand { get; private set; }
        public ICommand ShowThicknessViewCommand { get; private set; }

        public ICommand OpenCsvDirCommand { get; private set; }
    }
}