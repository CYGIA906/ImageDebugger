using System.IO;
using System.Xml.Serialization;
using PropertyChanged;

namespace ImageDebugger.Core.Models
{
    public class FaiItem : AutoSerializableBase<FaiItem>
    {
       

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
        


    

        public FaiItem(string name)
        {
            Name = name;
        }

        public FaiItem()
        {

        }

    }
}