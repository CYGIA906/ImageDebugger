using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HalconDotNet;
using UI.Model;
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

        Dictionary<string, double> Process(List<HImage> images, FindLineConfigs findLineConfigs,
            ObservableCollection<FaiItem> faiItems, int indexToShow, out HalconGraphics graphics,
            out DataRecorder recorder);

         ObservableCollection<FaiItem> GenFaiItemValues(string faiItemSerializationDir);


         List<FindLineLocation> GenFindLineLocationValues();

         ObservableCollection<FindLineParam> GenFindLineParamValues(string paramSerializationBaseDir);
        
    }
}