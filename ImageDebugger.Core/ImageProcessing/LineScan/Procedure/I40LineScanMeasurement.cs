using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using cyXYZInspector;
using HalconDotNet;
using ImageDebugger.Core.Helpers;
using ImageDebugger.Core.Models;
using ImageDebugger.Core.ViewModels.Application;
using ImageDebugger.Core.ViewModels.LineScan.Flatness;
using ImageDebugger.Core.ViewModels.LineScan.Parallelism;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;
using ImageDebugger.Core.ViewModels.LineScan.Thickness;

namespace ImageDebugger.Core.ImageProcessing.LineScan.Procedure
{
    public partial class I40LineScanMeasurement : ILineScanMeasurementProcedure
    {
        /// <summary>
        /// The name of the points to be found
        /// </summary>
        public IEnumerable<string> PointNames { get; }

        public string Name { get; set; } = "I40";
        public int NumImageRequireInSingleRun { get; set; } = 3;

        private HDevelopExport _halconScripts = new HDevelopExport();


        private HTuple _shapeModelHandleRight;

        public string ShapeModelPath
        {
            get { return ApplicationViewModel.SolutionDirectory + "/Configs/3D/I40/ShapeModels/model-2-4-14"; }
        }

        public I40LineScanMeasurement()
        {
            PointNames = GenPointNames();

            HOperatorSet.ReadShapeModel(ShapeModelPath, out _shapeModelHandleRight);
        }

        private IEnumerable<string> GenPointNames()
        {
            var output = new List<string>()
            {
                "16.2-1", "16.2-2", "16.2-3", "16.2-4", "16.2-5", "16.2-6", "16.2-7", "16.2-8",
                "16.3-1", "16.3-2", "16.3-3", "16.3-4", "16.3-5", "16.3-6", "16.3-7", "16.3-8",
                "16.4-1", "16.4-2", "16.4-3", "16.4-4", "16.4-5", "16.4-6", "16.4-7", "16.4-8",
                "16.5-1", "16.5-2", "16.5-3", "16.5-4", "16.5-5", "16.5-6", "16.5-7", "16.5-8",
                "17.1-1", "17.1-2",
                "17.2-1", "17.2-2",
                "17.3-1", "17.3-2",
                "17.4-1", "17.4-2",
                "18-E1", "18-E2", "18-E3", "18-E4", "18-E5", "18-E6", "18-E7", "18-E8", "18-E9", "18-E10", "18-E11",
                "18-E12", "18-E13", "18-E14", "18-E15", "18-E16",
                "19-F1", "19-F2", "19-F3", "19-F4", "19-F5", "19-F6", "19-F7", "19-F8", "19-F9", "19-F10", "19-F11",
                "19-F12", "19-F13", "19-F14",
                "19-A1", "19-A2", "19-A3", "19-A4", "19-A5", "19-A6", "19-A7", "19-A8",
                "20.1-F", "20.2-F", "20.3-F", "20.4-F",
                "20.1-P", "20.2-P", "20.3-P", "20.4-P",
                "22-P1", "22-P2", "22-P3", "22-P4"
            };

            return output;
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
            ["16.2"]="16.2", ["16.3"] = "16.3",   ["16.4"] = "16.4",["16.5"] = "16.5", ["19-F"] = "21"
        };

        private List<string> PlaneNames { get; set; } = new List<string>()
        {
            "16.2", "16.3", "16.4", "16.5", 
            "19-F",
            "19-A"
        };


        private FindLineParam _findLineParamH = new FindLineParam()
        {
            CannyHigh = 200, CannyLow = 100, Threshold = 100, ErrorThreshold = 0.5
        };


        private FindLineParam _findLineParamV = new FindLineParam()
        {
            CannyHigh = 200, CannyLow = 100, Threshold = 500, ErrorThreshold = 0.2, NewWidth = 2, NumSubRects = 30,
            Sigma1 = 4
        };

        private Dictionary<string, Plane3D> Planes { get; set; } = new Dictionary<string, Plane3D>();


        private HImage VisualizeAlignment(HImage image1, HImage image2)
        {
            HTuple width, height;
            image1.GetImageSize(out width, out height);
            var emptyImage = new HImage();
            emptyImage.GenImageConst("byte", width, height);
            return emptyImage.Compose3(image1.ScaleImageMax().ScaleImage(0.5, 0),
                image2.ScaleImageMax().ScaleImage(0.5, 0));
        }


        private readonly double _yCoeff = 0.0123;

        private readonly double _xCoeff = 0.013988;


        private IEnumerable<PointSettingViewModel> GrabPointsThatStartWith(List<PointSettingViewModel> pointSettings,
            string planeName)
        {
            return pointSettings.Where(p => p.Name.StartsWith(planeName));
        }


        public List<ParallelismItemViewModel> CalParallelism(List<PointSettingViewModel> pointSettingViewModels)
        {
            // Fai 16.1
            var output = new List<ParallelismItemViewModel>();

            var pointsA = GrabPointsThatStartWith(pointSettingViewModels, "19-A");
            var planeA = Planes["19-A"];
            var planeF = Planes["19-F"];
            var settingViewModels = pointsA as PointSettingViewModel[] ?? pointsA.ToArray();

            var f16Dot1 = planeF.MeasureParallelism(planeA, settingViewModels.Select(ele => ele.X).ToArray(),
                settingViewModels.Select(ele => ele.Y).ToArray(), settingViewModels.Select(ele => ele.Value).ToArray());
            output.Add(new ParallelismItemViewModel()
            {
                Name = "16.1",
                Value = f16Dot1
            });

            // Fai 22
            var points22p = GrabPointsThatStartWith(pointSettingViewModels, "22-P");
            var pointArray = points22p as PointSettingViewModel[] ?? points22p.ToArray();
            var xs = pointArray.Select(ele => ele.X).ToArray();
            var ys =
                pointArray.Select(ele => ele.Y).ToArray();
            var zs = pointArray.Select(ele => ele.Value).ToArray();
            var planeP = Plane3D.leastSquareAdaptFlatSurface(xs, ys, zs);
            
                var f22 = planeF.MeasureParallelism(planeP, xs, ys, zs);
            output.Add(new ParallelismItemViewModel()
            {
                Name = "22",
                Value = f22
            });

            return output;
        }

        public List<ThicknessItemViewModel> CalcThickness(List<PointSettingViewModel> pointSettingViewModels)
        {
            var output = new List<ThicknessItemViewModel>();

            // Point-point thickness
            foreach (var name in ThicknessPointPointMatches)
            {
                var pointPair = pointSettingViewModels.Where(p => p.Name.StartsWith(name)).ToList();
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
                var point = pointSettingViewModels.ByName(match.Key);
                var plane = Planes[match.Value];
                output.Add(new ThicknessItemViewModel()
                {
                    Name = point.Name,
                    Value = plane.GetDistance(point.X, point.Y, point.Value)
                });
            }

            // Cal fai18
            ThicknessItemViewModel f18c = CalFai18C(pointSettingViewModels);
            IEnumerable<ThicknessItemViewModel> f18m = CalFai18M(pointSettingViewModels);

            output.Add(f18c);
            output.AddRange(f18m);

            return output;
        }

        private IEnumerable<ThicknessItemViewModel> CalFai18M(List<PointSettingViewModel> pointSettingViewModels)
        {
            List<List<string>> names2D = new List<List<string>>
            {
                new List<string>() {"18-E16", "18-E1", "18-E3"},
                new List<string>() {"18-E4", "18-E6", "18-E7",},
                new List<string>() {"18-E8", "18-E9", "18-E11",},
                new List<string>() {"18-E12", "18-E14", "18-E15",},
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
                    var point = pointSettingViewModels.ByName(name);
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
                var point = pointSettingViewModels.ByName(name);
                var height = Math.Abs(plane.GetDistance(point.X, point.Y, point.Value) - 2.612);
                if (height > max) max = height;
            }


            return new ThicknessItemViewModel()
            {
                Name = "18-C",
                Value = max
            };
        }


        public List<FlatnessItemViewModel> CalcFlatness(List<PointSettingViewModel> pointSettingViewModels)
        {
            var output = new List<FlatnessItemViewModel>();
            foreach (var map in FlatnessNameMaps)
            {
                var plane = Planes[map.Key];
                var planeData = GrabPointsThatStartWith(pointSettingViewModels, map.Key);
                output.Add(new FlatnessItemViewModel()
                {
                    Name = map.Value,
                    Value = plane.MeasureFlatness(planeData.Select(d => d.X).ToArray(),
                        planeData.Select(d => d.Y).ToArray(), planeData.Select(d => d.Value).ToArray())
                });
            }

            return output;
        }


        public void ConstructPlanes(List<PointSettingViewModel> pointSettings)
        {
            foreach (var planeName in PlaneNames)
            {
                IEnumerable<PointSettingViewModel> planeData = GrabPointsThatStartWith(pointSettings, planeName);
                Planes[planeName] = Plane3D.leastSquareAdaptFlatSurface(planeData.Select(d => d.X).ToArray(),
                    planeData.Select(d => d.Y).ToArray(), planeData.Select(d => d.Value).ToArray());
            }
        }
    }
}