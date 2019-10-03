using System.Collections.ObjectModel;
using System.Windows.Input;
using HalconDotNet;
using UI.ViewModels;

namespace UI.ImageProcessing
{
    public interface IMeasurementProcedure
    {
        ObservableCollection<FaiItem> FaiItems { get; }

        void Process(HImage image);

    }
}