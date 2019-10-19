using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using UI.Enums;
using UI.ImageProcessing.BottomView;
using UI.ImageProcessing.TopView;

namespace UI.Converters
{
    public class EnumToMeasurementPageConverters : ValueConverterBase<EnumToMeasurementPageConverters>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static Page RetrievePage(MeasurementPage pageEnum)
        {
            var requestedMeasurementViewModelType = pageEnum == MeasurementPage.I94Top
                ? typeof(I94TopViewMeasure)
                : typeof(I94BottomViewMeasure);
            foreach (var page in MeasurementPages)
            {
                
            }
            
            throw new NotImplementedException();
        }
        
        private static List<Page> MeasurementPages { get; } = new List<Page>();

    }
}