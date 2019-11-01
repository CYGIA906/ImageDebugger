using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using ImageDebugger.Core.Enums;
using ImageDebugger.Core.WPFBase;
using UI._3D.Views.Drawer;
using UI._3D.Views.Drawer.DrawerContent;
using UI._3D.Views.Drawer.DrawerContent.Flatness;
using UI._3D.Views.Drawer.DrawerContent.Thickness;

namespace UI._3D.Converters
{
    public class EnumToDrawContentViewConverter : ValueConverterBase<EnumToDrawContentViewConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var viewType = (DrawerContentType3D) value;
            var outputType = viewType == DrawerContentType3D.PointSettings ? typeof(PointSettingView) :
                viewType == DrawerContentType3D.Flatness ? typeof(FlatnessListView) :
                viewType == DrawerContentType3D.Parallelism ? typeof(ParallelismView) : typeof(ThicknessListView);
            UserControl output;
            try
            {
                output = DrawerContentViews.First(ele => ele.GetType() == outputType);
            }
            catch (InvalidOperationException e)
            {
                output = (UserControl) Activator.CreateInstance(outputType);
                DrawerContentViews.Add(output);
            }

            return output;

        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static List<UserControl> DrawerContentViews { get; } = new List<UserControl>();
    }
}