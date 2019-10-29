using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using PropertyChanged;

namespace ImageDebugger.Core.ViewModels.Base
{
    public class AutoSerializableBase<T> : ViewModelBase
    {
        /// <summary>
        /// Fai name
        /// </summary>
        [XmlAttribute]
        [DoNotNotify]
        public string Name { get; set; }

        /// <summary>
        /// Whether the object should be auto-serialize when changed
        /// </summary>
        [XmlIgnore][DoNotNotify]
        public bool ShouldAutoSerialize { get; set; }

        public AutoSerializableBase()
        {
            PropertyChanged += Serialize;
        }

        private void Serialize(object sender, PropertyChangedEventArgs e)
        {
            if (!ShouldAutoSerialize) return;
            if (string.IsNullOrEmpty(Name)) return;

            using (var fs = new FileStream(Path.Combine(SerializationDirectory, Name + ".xml")
                , FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(fs, this);
            }
        }

        [DoNotNotify] public string SerializationDirectory { get; set; }
    }
}