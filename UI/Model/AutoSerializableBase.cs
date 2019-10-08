using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using UI.ViewModels;

namespace UI.Model
{
    public class AutoSerializableBase<T> : ViewModelBase
    {
        private void Serialize(object sender, PropertyChangedEventArgs e)
        {
            using (var fs = new FileStream(GetSerializationPath(), FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(fs, this);
            }
        }

        protected virtual string GetSerializationPath()
        {
            throw new System.NotImplementedException();
        }

        public void ResumeAutoSerialization()
        {
            PropertyChanged += Serialize;
        }
        
        public void StopAutoSerialization()
        {
            PropertyChanged -= Serialize;
        }
    }
}