using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using HalconDotNet;
using MaterialDesignThemes.Wpf;
using UI.Models;
using UI.ViewModels;

namespace UI.ImageProcessing
{
    public interface IMeasurementProcedure
    {
        /// <summary>
        /// Fire when the measurement result output is ready but have not been fed back
        /// </summary>
        event Action MeasurementResultReady;
        
        string Name { get; }

        /// <summary>
        /// Fire after the feed back of measurement result
        /// </summary>
        event Action MeasurementResultPulled;

        Task<ImageProcessingResult> ProcessAsync(List<HImage> images, FindLineConfigs findLineConfigs,
            ObservableCollection<FaiItem> faiItems, int indexToShow, SnackbarMessageQueue messageQueue
          );

         ObservableCollection<FaiItem> GenFaiItemValues(string faiItemSerializationDir);


         List<FindLineLocation> GenFindLineLocationValues();

         ObservableCollection<FindLineParam> GenFindLineParamValues(string paramSerializationBaseDir);
        
    }
}