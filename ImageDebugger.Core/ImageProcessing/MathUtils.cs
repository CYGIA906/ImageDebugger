using System;
using MathNet.Numerics.LinearAlgebra;

namespace ImageDebugger.Core.ImageProcessing
{
    public static class MathUtils
    {

        private static HDevelopExport _halconScripts  = new HDevelopExport();
        public static double ToRadian(double degree)
        {
            return degree / 180.0 * Math.PI;
        }

        public static Matrix<double> GetChangeOfBaseInv(Line xAxisNew, Line yAxisNew, double xUnit, double yUnit)
        {
            Point xUnitVec = xAxisNew.GetUnitVector();
            Point yUnitVec = yAxisNew.GetUnitVector();

            var intersection = xAxisNew.Intersect(yAxisNew);

            double[,] values = {
                {xUnitVec.ImageX * xUnit, yUnitVec.ImageX * yUnit, intersection.ImageX},
                {xUnitVec.ImageY * xUnit, yUnitVec.ImageY * yUnit, intersection.ImageY},
                {0.0, 0.0, 1.0}
            };

            var builder = Matrix<double>.Build;
           var changeOfBase = builder.DenseOfArray(values);
           
           return changeOfBase.Inverse();

        }

      
    }
}
