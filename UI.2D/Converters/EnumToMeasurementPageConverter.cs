using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using ImageDebugger.Core.Enums;
using ImageDebugger.Core.ImageProcessing;
using ImageDebugger.Core.ViewModels.CameraMeasurement;
using ImageDebugger.Core.WPFBase;
using UI._2D.Views;
using I94BottomViewMeasure = ImageDebugger.Core.ImageProcessing.BottomView.I94BottomViewMeasure;
using I94TopViewMeasure = ImageDebugger.Core.ImageProcessing.TopView.I94TopViewMeasure;

namespace UI._2D.Converters
{
    public class EnumToMeasurementPageConverter : ValueConverterBase<EnumToMeasurementPageConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var pageEnum = (MeasurementPage) value;
            return RetrievePage(pageEnum);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static UserControl RetrievePage(MeasurementPage pageEnum)
        {

            CameraMeasurementView output;
            // Try get the first halcon window page with the requested measurement procedure
            try
            {
                output = pageEnum == MeasurementPage.I94Top? MeasurementPages.First(page => ((CameraMeasurementViewModel)page.DataContext).MeasurementUnit is I94TopViewMeasure)
                    : MeasurementPages.First(page => ((CameraMeasurementViewModel)page.DataContext).MeasurementUnit is I94BottomViewMeasure);
            }
            // If the list not contain a halcon page with the specific measurement procedure
            // Add one and return it
            catch (InvalidOperationException e)
            {
                IMeasurementProcedure procedure;
                if (pageEnum == MeasurementPage.I94Top)
                {
                    procedure = new I94TopViewMeasure();
                }
                else
                {
                    procedure = new I94BottomViewMeasure();
                }

                var page = new CameraMeasurementView()
                {
                    DataContext = new CameraMeasurementViewModel()
                    {
                        MeasurementUnit = procedure
                    }
                };

                MeasurementPages.Add(page);
                output = page;
            }


            return output;
        }
        
        private static List<CameraMeasurementView> MeasurementPages { get; } = new List<CameraMeasurementView>();

    }
}