using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HalconDotNet;
using UI.ViewModels;

namespace UI.ImageProcessing
{
    public interface IMeasurementProcedure
    {
        ObservableCollection<FaiItem> FaiItems { get; }

        void Process(List<HImage> images, FindLineConfigs findLineConfigs, HWindow windowHandle9);

    }
}