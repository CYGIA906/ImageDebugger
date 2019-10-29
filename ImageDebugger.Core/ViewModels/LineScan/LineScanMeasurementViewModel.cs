using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ImageDebugger.Core.Commands;
using ImageDebugger.Core.Enums;
using ImageDebugger.Core.Helpers;
using ImageDebugger.Core.ImageProcessing.LineScan;
using ImageDebugger.Core.ViewModels.Base;
using ImageDebugger.Core.ViewModels.LineScan.PointSetting;

namespace ImageDebugger.Core.ViewModels.LineScan
{
    public class LineScanMeasurementViewModel : MeasurementPlayerViewModelBase
    {
        private ILineScanMeasurementProcedure LineScanMeasurementProcedure { get; set; } = new I40LineScanMeasurement();

        /// <summary>
        /// Point settings
        /// </summary>
        public List<PointSettingViewModel> PointSettingViewModels { get; set; }

        /// <summary>
        /// Root directory for serialization
        /// </summary>
        private string SerializationBaseDir =>
            Path.Combine(IoC.IoC.SerializationDirectory, LineScanMeasurementProcedure.Name);

        public DrawerContentType3D DrawerContent { get; set; } = DrawerContentType3D.PointSettings;

        private string PointSettingSerializationDir => Path.Combine(SerializationBaseDir, "Points");
            

        public LineScanMeasurementViewModel()
        {
            PointSettingViewModels =
                AutoSerializableHelper.LoadAutoSerializables<PointSettingViewModel>(
                    LineScanMeasurementProcedure.PointNames, PointSettingSerializationDir).ToList(); 
            
            
            // Commands
            ShowPointSettingViewCommand = new RelayCommand(ShowPointSettingView);
            ShowFlatnessViewCommand = new RelayCommand(ShowFlatnessView);
            ShowParallelismViewCommand = new RelayCommand(ShowParallelismView);
            ShowThicknessViewCommand = new RelayCommand(ShowThicknessView);
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
        
        
        
        
    }
}