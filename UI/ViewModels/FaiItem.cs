using System.Xml.Serialization;

namespace UI.ViewModels
{
    public class FaiItem : ViewModelBase
    {
        /// <summary>
        /// Fai name
        /// </summary>
        [XmlAttribute]
        public string Name { get; }

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
         public double ValueBiased => Value + Bias;

        /// <summary>
        /// Bias 
        /// </summary>
        [XmlAttribute]public double Bias { get; set; }


        /// <summary>
        /// Measure result
        /// </summary>
        public bool Passed => ValueBiased > MinBoundary && ValueBiased < MaxBoundary;

        public FaiItem(string name)
        {
            Name = name;
        }
    }
}