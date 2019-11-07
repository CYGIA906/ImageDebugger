using System.Collections.Generic;
using HalconDotNet;
using ImageDebugger.Core.ViewModels.LineScan.Flatness;
using ImageDebugger.Core.ViewModels.LineScan.Parallelism;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;
using ImageDebugger.Core.ViewModels.LineScan.Thickness;
using MaterialDesignThemes.Wpf;

namespace ImageDebugger.Core.ImageProcessing.LineScan
{
    public interface ILineScanMeasurementProcedure
    {
        IEnumerable<string> PointNames { get;  }
        
        string Name { get; set; }
        
        int NumImageRequireInSingleRun { get; set; }

        ImageProcessingResults3D Process(List<HImage> images, List<PointSettingViewModel> pointSettings,
            ISnackbarMessageQueue messageQueue);

        List<FlatnessItemViewModel> CalcFlatness(List<PointSettingViewModel> pointSettingViewModels);

        List<ThicknessItemViewModel> CalcThickness(List<PointSettingViewModel> pointSettingViewModels);

        void ConstructPlanes(List<PointSettingViewModel> pointSettings);

        List<ParallelismItemViewModel> CalParallelism(List<PointSettingViewModel> pointSettingViewModels);
    }
}