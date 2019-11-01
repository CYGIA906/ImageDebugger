using System.Xml.Serialization;
using ImageDebugger.Core.ViewModels.Base;

namespace ImageDebugger.Core.ViewModels.LineScan.PointSetting
{
    public class PointSettingViewModel : AutoSerializableBase<PointSettingViewModel>, ICsvColumnElement
    {
        [XmlAttribute] public double X { get; set; }
        [XmlAttribute] public double Y { get; set; }
        [XmlAttribute] public int KernelSize { get; set; } = 3;
        public string CsvName
        {
            get { return "Point " + Name; }
        }

        [XmlIgnore] public double Value { get; set; }
        [XmlAttribute] public double ZWeight { get; set; } = 1;
        [XmlAttribute] public double ZBias { get; set; }
        [XmlAttribute] public int ImageIndex { get; set; }
    }
}