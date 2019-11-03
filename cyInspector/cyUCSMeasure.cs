using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyInspector
{
    public class cyUCSMeasure
    {
        /// <summary>
        /// 图像坐标，转换成世界坐标；
        /// </summary>
        /// <param name="Ix">图像坐标点</param>
        /// <param name="Iy"></param>
        /// <param name="Iz"></param>
        /// <param name="Wp">世界坐标点， lenth = 3</param>
        /// <param name="Rmatrix">转换矩阵</param>
        /// <param name="offsetT">偏移矩阵</param>
        public static void Ix2Wx(double Ix, double Iy, double Iz, out double[] Wp, List<double> Rmatrix, List<double> offsetT)
        {
            ///图像坐标可以转换到世界坐标，但世界坐标不能转换到图像坐标去，只能转换得有一定得偏差
            ///W = R·I + T;
            Wp = new double[3] { 0, 0, 0 };
            Wp[0] = Rmatrix[0] * Ix + Rmatrix[1] * Iy + Rmatrix[2] * Iz + offsetT[0];
            Wp[1] = Rmatrix[3] * Ix + Rmatrix[4] * Iy + Rmatrix[5] * Iz + offsetT[1];
            Wp[2] = Rmatrix[6] * Ix + Rmatrix[7] * Iy + Rmatrix[8] * Iz + offsetT[2];
        }

        //将存在一定的偏差
        public static void Wx2Ix(out double Ix, out double Iy, out double Iz, double[] Wp, List<double> RInvmatrix, List<double> offsetT)
        {
            ///图像坐标可以转换到世界坐标，但世界坐标不能转换到图像坐标去，只能转换得有一定得偏差
            ///W = R·I + T;
            Wp[0] -= offsetT[0]; Wp[1] -= offsetT[1]; Wp[2] -= offsetT[2];

            Ix = RInvmatrix[0] * Wp[0] + RInvmatrix[1] * Wp[1] + RInvmatrix[2] * Wp[2];
            Iy = RInvmatrix[3] * Wp[0] + RInvmatrix[4] * Wp[1] + RInvmatrix[5] * Wp[2];
            Iz = RInvmatrix[6] * Wp[0] + RInvmatrix[7] * Wp[1] + RInvmatrix[8] * Wp[2];
        }

        public static void IxLine2WxLine(cylineParam IxLine, out cylineParam Wxline, List<double> Rmatrix, List<double> offsetT)
        {
            /// Iz 用于补偿 图像中z的方向值
            Wxline = new cylineParam() { A = 0, B = 0, C = 0};
            List<double> RInvmatrix = new List<double>();
            MatrixInv3X3(Rmatrix, out RInvmatrix);
            if (RInvmatrix.Count != 9)
                return;

            Wxline.A = IxLine.A * RInvmatrix[0] + RInvmatrix[3] * IxLine.B;
            Wxline.B = IxLine.A * RInvmatrix[1] + RInvmatrix[4] * IxLine.B;
            double cAdd = Wxline.A * offsetT[0] + Wxline.B * offsetT[1];
            Wxline.C = cAdd + IxLine.C;
            double div = Math.Sqrt(Wxline.A * Wxline.A + Wxline.B * Wxline.B);
            Wxline.A /= div; Wxline.B /= div; Wxline.C /= div;

        }

        public static void MatrixInv3X3(List<double> Rmatrix, out List<double> RInvmatrix)
        {
            RInvmatrix = new List<double>();
            double t11 = Rmatrix[4] * Rmatrix[8] - Rmatrix[5] * Rmatrix[7]; 
            double t12 = -Rmatrix[1] * Rmatrix[8] + Rmatrix[2] * Rmatrix[7];
            double t13 = Rmatrix[1] * Rmatrix[5] - Rmatrix[2] * Rmatrix[4];

            double t21 = -Rmatrix[3] * Rmatrix[8] + Rmatrix[5] * Rmatrix[6];
            double t22 = Rmatrix[0] * Rmatrix[8] - Rmatrix[2] * Rmatrix[6];
            double t23 = Rmatrix[2] * Rmatrix[3] - Rmatrix[0] * Rmatrix[5];

            double t31 = Rmatrix[3] * Rmatrix[7] - Rmatrix[4] * Rmatrix[6];
            double t32 = -Rmatrix[0] * Rmatrix[7] + Rmatrix[1] * Rmatrix[6];
            double t33 = Rmatrix[0] * Rmatrix[4] - Rmatrix[1] * Rmatrix[3];

            double decade = Rmatrix[0] * t11 + Rmatrix[1] * t21 + Rmatrix[2] * t31;

            if (Math.Abs(decade) < 1e-12)
                return;

            RInvmatrix.Add(t11/decade); RInvmatrix.Add(t12/decade); RInvmatrix.Add(t13 / decade);
            RInvmatrix.Add(t21 / decade); RInvmatrix.Add(t22 / decade); RInvmatrix.Add(t23 / decade);
            RInvmatrix.Add(t31 / decade); RInvmatrix.Add(t32 / decade); RInvmatrix.Add(t33 / decade);
        }

        public static void MatrixInv2X2(List<double> Rmatrix, out List<double> RInvmatrix)
        {
            RInvmatrix = new List<double>() { 0,0,0,0};
            double div = Rmatrix[3] * Rmatrix[0] - Rmatrix[1] * Rmatrix[2];
            if (Math.Abs(div) <= 1e-24)
                return;
            RInvmatrix[0] = Rmatrix[3]/div; RInvmatrix[1] = -Rmatrix[1]/div;
            RInvmatrix[2] = -Rmatrix[2]/div; RInvmatrix[3] = Rmatrix[0]/div;
        }

        public static void Matrix_Mult3X1(List<double> Rmatrix, List<double> array1, out List<double> result)
        {
            result = new List<double>() { 0,0,0};
            if (Rmatrix.Count != 9)
                return;
            if (array1.Count != 3)
                return;

            result[0] = Rmatrix[0] * array1[0] + Rmatrix[1] * array1[1] + Rmatrix[2] * array1[2];
            result[1] = Rmatrix[3] * array1[0] + Rmatrix[4] * array1[1] + Rmatrix[5] * array1[2];
            result[2] = Rmatrix[6] * array1[0] + Rmatrix[7] * array1[1] + Rmatrix[8] * array1[2];
        }

        public static void Matrix_Mult3X3(List<double> Rmatrix, List<double> array1, out List<double> result)
        {
            result = new List<double>(9) { 0, 0, 0 ,0,0,0,0,0,0};
            if (Rmatrix.Count != 9)
                return;
            if (array1.Count != 9)
                return;

            for (int t = 0; t < 3; t++)
            {
                int i = t * 3;
                result[i] = Rmatrix[i] * array1[0] + Rmatrix[i+1] * array1[3] + Rmatrix[i+2] * array1[6];
                result[i+1] = Rmatrix[i] * array1[1] + Rmatrix[i+1] * array1[4] + Rmatrix[i+2] * array1[7];
                result[i+2] = Rmatrix[i] * array1[2] + Rmatrix[i+1] * array1[5] + Rmatrix[i+2] * array1[8];
            }
        }

        // result = Rmatrix*mul1 + bias;
        public static void Matrix_Mult3X1(List<double> Rmatrix, List<double> mul1, List<double> bias, out List<double> result)
        {
            result = new List<double>() { 0, 0, 0 };
            if (Rmatrix.Count != 9)
                return;
            if (mul1.Count != 3)
                return;

            result[0] = Rmatrix[0] * mul1[0] + Rmatrix[1] * mul1[1] + Rmatrix[2] * mul1[2] + bias[0]; 
            result[1] = Rmatrix[3] * mul1[0] + Rmatrix[4] * mul1[1] + Rmatrix[5] * mul1[2] + bias[1];
            result[2] = Rmatrix[6] * mul1[0] + Rmatrix[7] * mul1[1] + Rmatrix[8] * mul1[2] + bias[2];
        }

        public static void Matrix_Mult2X1(List<double> Rmatrix, List<double> input, List<double> bias, out List<double> result)
        {
            result = new List<double>() { 0, 0};
            if (Rmatrix.Count != 4)
                return;
            if (input.Count != 2)
                return;

            result[0] = Rmatrix[0] * input[0] + Rmatrix[1] * input[1] + bias[0];
            result[1] = Rmatrix[2] * input[0] + Rmatrix[3] * input[1] + bias[1];
        }

        public static void Matrix_Mult2X1(List<double> Rmatrix, List<double> input, out List<double> result)
        {
            result = new List<double>() { 0, 0 };
            if (Rmatrix.Count != 4)
                return;
            if (input.Count != 2)
                return;

            result[0] = Rmatrix[0] * input[0] + Rmatrix[1] * input[1];
            result[1] = Rmatrix[2] * input[0] + Rmatrix[3] * input[1];
        }

        public static void GetRMatrix3X3(double xAngle, double yAngle, double zAngle, out List<double> Rmatrix)
        {
            List<double> Rmatrix1 = new List<double>(9) { 0, 0, 0 , 0, 0, 0, 0,0,0};
            List<double> Rmatrix2 = new List<double>(9) { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            List<double> Rmatrix3 = new List<double>(9) { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Rmatrix1[0] = 1;
            Rmatrix1[4] =Rmatrix1[8]= Math.Cos(xAngle);
            Rmatrix1[5] = -Math.Sin(xAngle); Rmatrix1[7] = -Rmatrix1[5];


            Rmatrix2[4] = 1;
            Rmatrix2[0] = Rmatrix2[8] = Math.Cos(yAngle);
            Rmatrix2[2] = Math.Sin(yAngle); Rmatrix2[6] = -Rmatrix2[2];

            Rmatrix3[8] = 1;
            Rmatrix3[0] = Rmatrix3[4] = Math.Cos(zAngle);
            Rmatrix3[1] = -Math.Sin(zAngle); Rmatrix3[3] = -Rmatrix3[1];

            List<double> tmpValue;
            Matrix_Mult3X3(Rmatrix1, Rmatrix2, out tmpValue);
            Matrix_Mult3X3(tmpValue, Rmatrix3, out Rmatrix);


        }
    }
}
