using System;
using System.Collections.ObjectModel;
using HalconDotNet;
using UI.ImageProcessing;

// TODO: Finish displaying part

namespace UI.ViewModels
{
    public class HalconWindowPageViewModel : ViewModelBase, IMeasurementProcedure
    {
        public ObservableCollection<FaiItem> FaiItems { get; }

        /// <summary>
        /// Path to the shape model in disk
        /// </summary>
        public string ModelPath
        {
            get => _modelPath;
            set
            {
                _modelPath = value;
                HOperatorSet.ReadShapeModel(_modelPath, out _shapeModelHandle);
            }
        }

        HDevelopExport _halconScript = new HDevelopExport();
        private HTuple _shapeModelHandle;
        private double[] _thresholds = { 128, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 };
        private string _modelPath;

        public void Process(HImage image)
        {
            HObject imageUndistorted, lineRegions, findLineRects;
            HTuple measuredValues, pixelValues, ptXsUsed, ptYsUsed, ptXsIgnored, ptYsIgnored, interXs, interYs;
            _halconScript.I94TopFaceMeasurement(image, out imageUndistorted, out lineRegions, out findLineRects,
                _shapeModelHandle, 0, 0, _thresholds, out measuredValues, out ptXsUsed, out ptYsUsed, out ptXsIgnored,
                out ptYsIgnored, out interXs, out interYs, out pixelValues);


            FillFaiItems(measuredValues);
        }


        /// <summary>
        /// Take measured values from halcon script and fill them into <see cref="FaiItems"/>
        /// </summary>
        /// <param name="measuredValues"></param>
        private void FillFaiItems(HTuple measuredValues)
        {
            var values = measuredValues.ToDArr();
            var numValues = values.Length;
            var numItems = FaiItems.Count;
            if(numItems != numValues)
                throw new InvalidOperationException($"Expect {numItems} but get {numValues} measured values");

            for (int i = 0; i < numValues; i++)
            {
                FaiItems[i].Value = values[i];
            }
        }


        public HalconWindowPageViewModel()
        {
            FaiItems = new ObservableCollection<FaiItem>();
            FaiItems.Add(new FaiItem("FAI2") { MaxBoundary = 15.49, MinBoundary = 15.390});

            FaiItems.Add(new FaiItem("FAI3") { MaxBoundary = 15.906, MinBoundary = 15.806 });

            FaiItems.Add(new FaiItem("FAI4.1") {  MaxBoundary = 34.92, MinBoundary = 34.82 });
            FaiItems.Add(new FaiItem("FAI4.2") {  MaxBoundary = 34.92, MinBoundary = 34.82 });
            FaiItems.Add(new FaiItem("FAI4.3") {  MaxBoundary = 34.92, MinBoundary = 34.82 });

            FaiItems.Add(new FaiItem("FAI5.1") {  MaxBoundary = 36.15, MinBoundary = 36.05 });
            FaiItems.Add(new FaiItem("FAI5.2") {  MaxBoundary = 36.15, MinBoundary = 36.05 });
            FaiItems.Add(new FaiItem("FAI5.3") {  MaxBoundary = 36.15, MinBoundary = 36.05 });

            FaiItems.Add(new FaiItem("FAI6.1") {MaxBoundary = 22.153, MinBoundary = 22.053 });
            FaiItems.Add(new FaiItem("FAI6.2") {  MaxBoundary = 22.153, MinBoundary = 22.053 });
            FaiItems.Add(new FaiItem("FAI6.3") {  MaxBoundary = 22.153, MinBoundary = 22.053 });

            FaiItems.Add(new FaiItem("FAI9.1") {  MaxBoundary = 11.231, MinBoundary = 11.131 });
            FaiItems.Add(new FaiItem("FAI9.2") {  MaxBoundary = 11.231, MinBoundary = 11.131 });
            FaiItems.Add(new FaiItem("FAI9.3") {  MaxBoundary = 11.231, MinBoundary = 11.131 });

            FaiItems.Add(new FaiItem("FAI12.1") {  MaxBoundary = 18.805, MinBoundary = 18.705 });
            FaiItems.Add(new FaiItem("FAI12.2") {  MaxBoundary = 18.805, MinBoundary = 18.705 });
            FaiItems.Add(new FaiItem("FAI12.3") {  MaxBoundary = 18.805, MinBoundary = 18.705 });

            FaiItems.Add(new FaiItem("FAI16.1") {  MaxBoundary = 29.599, MinBoundary = 29.499});
            FaiItems.Add(new FaiItem("FAI16.2") {  MaxBoundary = 29.599, MinBoundary = 29.499});
            FaiItems.Add(new FaiItem("FAI16.3") {  MaxBoundary = 29.599, MinBoundary = 29.499});

            FaiItems.Add(new FaiItem("FAI17.1") {  MaxBoundary = 17.154, MinBoundary = 17.054});
            FaiItems.Add(new FaiItem("FAI17.2") {  MaxBoundary = 17.154, MinBoundary = 17.054});
            FaiItems.Add(new FaiItem("FAI17.3") {  MaxBoundary = 17.154, MinBoundary = 17.054});

            FaiItems.Add(new FaiItem("FAI19.1") {  MaxBoundary = 1.658, MinBoundary = 1.558});
            FaiItems.Add(new FaiItem("FAI19.2") {  MaxBoundary = 1.658, MinBoundary = 1.558});
            FaiItems.Add(new FaiItem("FAI19.3") {  MaxBoundary = 1.658, MinBoundary = 1.558});

            FaiItems.Add(new FaiItem("FAI20.1") {  MaxBoundary = 19.353, MinBoundary = 19.253});
            FaiItems.Add(new FaiItem("FAI20.2") {  MaxBoundary = 19.353, MinBoundary = 19.253});

        }


    }
}