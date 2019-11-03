using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using cyInspector;


namespace cyOXYInspector
{
    public class cyOXYCircle
    {
        public cyCircleParam cirValue;

        public cyOXYCircle()
        {
            cirValue = new cyCircleParam() { xCenter = 0.0, yCenter = 0.0, radius = 0.0 };
        }

        public cyOXYCircle(double xValue, double yValue, double radiusValue)
        {
            cirValue = new cyCircleParam() { xCenter = xValue, yCenter = yValue, radius = radiusValue };
        }
        public cyOXYCircle(cyCircleParam param)
        {
            cirValue = new cyCircleParam() { xCenter = param.xCenter, yCenter = param.yCenter, radius = param.radius };
        }

        /// <summary>
        /// 根据点坐标拟合一条直线
        /// </summary>
        public static bool leastSquareAdaptCirCle(double[] xArray, double[] yArray, out cyCircleParam circleValue)
        {
            Trace.Assert(xArray.Length == yArray.Length);
            cyPointBaseClass class1 = new cyPointBaseClass();
            circleValue = new cyCircleParam() { xCenter = 0, yCenter = 0, radius = 0};
            //最小二乘法矩阵：
            // [ xx, xy, x ] [a]   [-Ni_Xi]
            // [ xy, yy, y ]*[b] = [-Ni_Yi]; |A*AT| = |A|^n
            // [ x, y, nums] [c]   [ -Ni  ]
            int nums = xArray.Length;
            List<double> matrix = new List<double>(9) { 0,0,0,0,0,0,0,0,0 };
            List<double> ResltMatr = new List<double>(3) { 0,0,0 };
            for (int i = 0; i < xArray.Length; i++)
            {
                double tmpXX = xArray[i] * xArray[i];
                double tmpYY = yArray[i] * yArray[i];
                double tmpNi = tmpXX + tmpYY;

                ResltMatr[0] -= tmpNi * xArray[i];
                ResltMatr[1] -= tmpNi * yArray[i];
                ResltMatr[2] -= tmpNi;
                matrix[0] += tmpXX; matrix[1] += xArray[i] * yArray[i]; matrix[2] += xArray[i];
                matrix[4] += tmpYY;matrix[5] += yArray[i];
            }
            matrix[3] = matrix[1];matrix[6] = matrix[2]; matrix[7] = matrix[5]; matrix[8] = nums;
            //List<double> matrix = new List<double> { xx, xy, x, xy, yy, y, x, y, nums };
            List<double> INV_matrix;
            cyUCSMeasure.MatrixInv3X3(matrix, out INV_matrix);

            if (INV_matrix.Count != 9)
                return false;

            List<double> circleData;
            cyUCSMeasure.Matrix_Mult3X1(INV_matrix, ResltMatr, out circleData);

            circleValue.xCenter = -circleData[0] / 2; circleValue.yCenter = -circleData[1] / 2;
            double tValue = circleData[0] * circleData[0]/4 + circleData[1] * circleData[1]/4 - circleData[2];
            if (tValue < 1e-6)
                return false;

            circleValue.radius = Math.Sqrt(tValue);
            return true;
        }
    }
}
