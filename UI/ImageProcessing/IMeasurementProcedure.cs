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
        ObservableCollection<FaiItem> FaiItems { get; }

        /// <summary>
        /// Fire when the measurement result output is ready but have not been fed back
        /// </summary>
        event Action MeasurementResultReady;
        
        string Name { get; }

        /// <summary>
        /// Fire after the feed back of measurement result
        /// </summary>
        event Action MeasurementResultPulled;

        Dictionary<string, double> Process(List<HImage> images, FindLineConfigs findLineConfigs, HWindow windowHandle9,
            ObservableCollection<FaiItem> faiItems, out HalconGraphics graphics);

    }
}