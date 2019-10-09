using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using PropertyChanged;
using UI.Model;

namespace UI.ViewModels
{
    public class FaiItem : AutoSerializableBase<FaiItem>
    {
        /// <summary>
        /// Fai name
        /// </summary>
        [XmlAttribute][DoNotNotify]
        public string Name { get;  set; } 

        /// <summary>
        /// Max boundary of the fai item
        /// </summary>
        [XmlAttribute]public double MaxBoundary { get; set; }

        /// <summary>
        /// Min boundary of the fai item
        /// </summary>
        [XmlAttribute]public double MinBoundary { get; set; }

        /// <summary>
        /// Measured value
        /// </summary>
        public double Value;

        /// <summary>
        /// Measured value plus bias
        /// </summary>
         public double ValueBiased
        {
            get { return Value + Bias; }
        }

        /// <summary>
        /// Bias 
        /// </summary>
        [XmlAttribute]public double Bias { get; set; }


        /// <summary>
        /// Measure result
        /// </summary>
        public bool Passed
        {
            get { return ValueBiased > MinBoundary && ValueBiased < MaxBoundary; }
        }


        public string SerializationDir;


        protected override string GetSerializationPath()
        {
            return Path.Combine(SerializationDir, Name + ".xml");
        }

        public FaiItem(string name)
        {
            Name = name;
        }

        public FaiItem()
        {
        }

//        private void Serialize(object sender, PropertyChangedEventArgs e)
//        {
//            using (var fs = new FileStream(GetSerializationPath(), FileMode.Create))
//            {
//                var serializer = new XmlSerializer(typeof(FaiItem));
//                serializer.Serialize(fs, this);
//            }
//        }
//
//        public void ResumeAutoSerialization()
//        {
//            PropertyChanged += Serialize;
//        }
//        
//        public void StopAutoSerialization()
//        {
//            PropertyChanged -= Serialize;
//        }
    }
}