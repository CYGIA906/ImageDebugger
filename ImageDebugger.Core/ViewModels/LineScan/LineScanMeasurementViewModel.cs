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
        private ILineScanMeasurementProcedure LineScanMeasurementProcedure { get; set; } = new  I40LineScanMeasurement();

        /// <summary>
        /// Point settings
        /// </summary>
        public List<PointSettingViewModel> PointSettingViewModels { get; set; }


        public string ProcedureName
        {
            get { return LineScanMeasurementProcedure.Name; }
        }

        private Dictionary<string, Plane3D> Planes { get; set; } = new Dictionary<string, Plane3D>();
 
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


        private Dictionary<string, string> ThicknessPointPlaneMatches { get; set; } = new Dictionary<string, string>()
        {
            ["19-A1"] = "19-F",
            ["19-A2"] = "19-F",
            ["19-A3"] = "19-F",
            ["19-A4"] = "19-F",
            ["19-A5"] = "19-F",
            ["19-A6"] = "19-F",
            ["19-A7"] = "19-F",
            ["19-A8"] = "19-F",
        };
        
        
        private List<string> ThicknessPointPointMatches { get; set; } = new List<string>()
        {
            "17.1", "17.2", "17.3", "17.4", "20.1", "20.2", "20.3", "20.4"
        };
        /// <summary>
        /// Map from a point sets to a name of <see cref="FlatnessItemViewModel"/>
        /// </summary>
        private Dictionary<string, string> FlatnessNameMaps { get; set; } = new Dictionary<string, string>
        {
            ["16.3"] = "16.3", ["16.5"] = "16.5", ["19-F"] = "21"
        };

        private List<string> PlaneNames { get; set; } = new List<string>()
        {
                "16.3", "16.5","19-F", 
                "19-A"
        };
        
        /// <summary>
        /// Root directory for serialization
        /// </summary>
        private string ConfigurationBaseDir
        {
            get { return Path.Combine(ApplicationViewModel.SolutionDirectory + "/Configs/3D/", LineScanMeasurementProcedure.Name); }
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
            if(Directory.Exists(CsvDir)) Process.Start(CsvDir);
        }

        private async Task OnImageProcessStartAsync(List<HImage> images)
        {
            var result = await Task.Run(() => LineScanMeasurementProcedure.Process(ImageInputs, PointSettingViewModels, RunStatusMessageQueue));

            InfoImageList = result.Images.Take(3).ToList();
            

            BackView = result.Images[3];
            FrontView = result.Images[4];
            Result = result;
            ChangeOfBaseInv = result.ChangeOfBaseInv;
            
            if (CurrentViewIsBackView) ToggleBackView();
            else ToggleFrontView();
            
            // Calculate results
            UpdatePointSettings(result.PointMarkers);
            ConstructPlanes(PointSettingViewModels);
            ParallelismItemViewModels = CalParallelism(PointSettingViewModels);
            FlatnessViewModels = CalcFlatness();
            ThicknessViewModels = CalcThickness();
            
            // Serialize
            var csvSerializables = new List<ICsvColumnElement>();
            csvSerializables.AddRange(PointSettingViewModels);
            csvSerializables.AddRange(FlatnessViewModels);
            csvSerializables.AddRange(ThicknessViewModels);
            csvSerializables.AddRange(result.RecordingElements);
            CsvSerializer.Serialize(csvSerializables, CurrentImageName, IsContinuouslyRunning);
        }

        private List<ParallelismItemViewModel> CalParallelism(List<PointSettingViewModel> pointSettingViewModels)
        {
            // Fai 16.1
            var output = new List<ParallelismItemViewModel>();
            
            var pointsA = GrabPointsThatStartWith(pointSettingViewModels, "19-A");
            var planeF = Planes["19-F"];
            var settingViewModels = pointsA as PointSettingViewModel[] ?? pointsA.ToArray();
            
            var f16Dot1 = planeF.MeasureParallelism(planeF, settingViewModels.Select(ele => ele.X).ToArray(),
                settingViewModels.Select(ele => ele.Y).ToArray(), settingViewModels.Select(ele => ele.Value).ToArray());
            output.Add(new ParallelismItemViewModel()
            {
                Name = "16.1",
                Value = f16Dot1
            });
            
            // Fai 22
            var points22p = GrabPointsThatStartWith(pointSettingViewModels, "22-P");
            var planeA = Planes["19-A"];
            var pointArray = points22p as PointSettingViewModel[] ?? points22p.ToArray();
            
            var f22 = planeF.MeasureParallelism(planeF, pointArray.Select(ele => ele.X).ToArray(),
                pointArray.Select(ele => ele.Y).ToArray(), pointArray.Select(ele => ele.Value).ToArray());
            output.Add(new ParallelismItemViewModel()
            {
                Name = "22",
                Value = f22
            });

            return output;
        }

        private ImageProcessingResults3D Result { get; set; }

        private HImage FrontView { get; set; }

        private HImage BackView { get; set; }

        private List<ThicknessItemViewModel> CalcThickness()
        {
            var output = new List<ThicknessItemViewModel>();
            
            // Point-point thickness
            foreach (var name in ThicknessPointPointMatches)
            {
                var pointPair = PointSettingViewModels.Where(p => p.Name.StartsWith(name)).ToList();
                Trace.Assert(pointPair.Count == 2, $"Expected 2 points but get {pointPair.Count}");
            
                
                output.Add(new ThicknessItemViewModel()
                {
                    Name = name,
                    Value = Math.Abs(pointPair[0].Value - pointPair[1].Value)
                });
            }
            
            // Point-plane thickness
            foreach (var match in ThicknessPointPlaneMatches)
            {
                var point = PointSettingViewModels.ByName(match.Key);
                var plane = Planes[match.Value];
                output.Add(new ThicknessItemViewModel()
                {
                    Name = point.Name,
                    Value = plane.GetDistance(point.X, point.Y, point.Value)
                });
            }

            // Cal fai18
            ThicknessItemViewModel f18c = CalFai18C(PointSettingViewModels);
            IEnumerable<ThicknessItemViewModel> f18m = CalFai18M(PointSettingViewModels);

            output.Add(f18c);
            output.AddRange(f18m);

            return output;
        }

        private IEnumerable<ThicknessItemViewModel> CalFai18M(List<PointSettingViewModel> pointSettingViewModels)
        {
            List<List<string>> names2D = new List<List<string>>
            {
               new List<string>() {"18-E16", "18-E1", "18-E3"},
                new List<string>(){"18-E4", "18-E6","18-E7",},
                new List<string>(){"18-E8", "18-E9","18-E11",},
                new List<string>(){"18-E12", "18-E14","18-E15",},
            };

            string[] outputNames = new[] {"18E-topRight", "18E-bottomRight", "18E-bottomLeft", "18E-topLeft"};

            var plane = Planes["19-A"];
            
            var output = new List<ThicknessItemViewModel>();
            
            for (var index = 0; index < names2D.Count; index++)
            {
                var names = names2D[index];
                double max = 0;
                foreach (var name in names)
                {
                    var point = PointSettingViewModels.ByName(name);
                    var height = Math.Abs(plane.GetDistance(point.X, point.Y, point.Value) - 2.612);
                    if (height > max) max = height;
                }

                output.Add(new ThicknessItemViewModel()
                {
                    Name = outputNames[index],
                    Value = max
                });
            }

            return output;
        }

        private ThicknessItemViewModel CalFai18C(List<PointSettingViewModel> pointSettingViewModels)
        {

            var plane = Planes["19-A"];
            var pointNames = new string[]
            {
                "18-E2", "18-E5", "18-E10", "18-E13"
            };
            double max = 0;
            foreach (var name in pointNames)
            {
                var point = PointSettingViewModels.ByName(name);
                var height = Math.Abs(plane.GetDistance(point.X, point.Y, point.Value) - 2.612);
                if (height > max) max = height;
            }
            
            
            return new ThicknessItemViewModel()
            {
                Name = "18-C",
                Value = max
            };
        }


        
        private List<FlatnessItemViewModel> CalcFlatness()
        {
            var output = new List<FlatnessItemViewModel>();
            foreach (var map in FlatnessNameMaps)
            {
                var plane = Planes[map.Key];
                var planeData = GrabPointsThatStartWith(PointSettingViewModels, map.Key);
                output.Add(new FlatnessItemViewModel()
                {
                    Name = map.Value,
                    Value = plane.MeasureFlatness(planeData.Select(d => d.X).ToArray(),
                        planeData.Select(d => d.Y).ToArray(), planeData.Select(d => d.Value).ToArray())
                });
            }

            return output;
        }


        private void ConstructPlanes(List<PointSettingViewModel> pointSettings)
        {
            foreach (var planeName in PlaneNames)
            {
                IEnumerable<PointSettingViewModel> planeData = GrabPointsThatStartWith(pointSettings, planeName);
                Planes[planeName] = Plane3D.leastSquareAdaptFlatSurface(planeData.Select(d => d.X).ToArray(),
                    planeData.Select(d => d.Y).ToArray(), planeData.Select(d => d.Value).ToArray());
            }
        }
        

        private IEnumerable<PointSettingViewModel> GrabPointsThatStartWith(List<PointSettingViewModel> pointSettings, string planeName)
        {
            return pointSettings.Where(p => p.Name.StartsWith(planeName));
        }

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