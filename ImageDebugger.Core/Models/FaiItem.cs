using System.IO;
using System.Xml.Serialization;
using ImageDebugger.Core.ViewModels.Base;
using ImageDebugger.Core.ViewModels.LineScan;
using PropertyChanged;

namespace ImageDebugger.Core.Models
{
    public class FaiItem : AutoSerializableBase<FaiItem>, ICsvColumnElement
    {

        public string CsvName => Name;

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
        public double ValueUnbiased;

        /// <summary>
        /// Measured value plus bias
        /// </summary>
         public double Value    
        {
            get { return ValueUnbiased + Bias; }
            set => throw new System.NotImplementedException();
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
            get { return Value > MinBoundary && Value < MaxBoundary; }
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