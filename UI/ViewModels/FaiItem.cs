namespace UI.ViewModels
{
    public class FaiItem : ViewModelBase
    {
        /// <summary>
        /// Fai name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Max boundary of the fai item
        /// </summary>
        public double MaxBoundary { get; set; }

        /// <summary>
        /// Min boundary of the fai item
        /// </summary>
        public double MinBoundary { get; set; }

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
        public double Bias { get; set; }


        /// <summary>
        /// Measure result
        /// </summary>
        public bool Passed => ValueBiased > MinBoundary && ValueBiased < MaxBoundary;


    }
}