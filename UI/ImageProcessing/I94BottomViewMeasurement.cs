using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HalconDotNet;
using MaterialDesignThemes.Wpf;
using UI.Model;
using UI.ViewModels;

namespace UI.ImageProcessing
{
    public partial class I94BottomViewMeasurement : IMeasurementProcedure
    {
        public event Action MeasurementResultReady;
        public string Name { get; }
        public event Action MeasurementResultPulled;

        public Task<ImageProcessingResult> ProcessAsync(List<HImage> images, FindLineConfigs findLineConfigs, ObservableCollection<FaiItem> faiItems, int indexToShow,
            SnackbarMessageQueue messageQueue)
        {
            throw new NotImplementedException();
        }

    
    }
}