using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using cyInspector;

namespace cyXYZInspector
{
    /// <summary>
    /// 构建的O-XYZ平面坐标，
    /// </summary>
    public class Plane3D
    {
        public float A { get; private set; }
        public float B { get; private set; }
        public float C { get; private set; }
        public float D { get; private set; }
        private bool bNormalize { get; set; }

        #region Create Surface
        public Plane3D()
        {
            A = 0.0f;
            B = 0.0f;
            C = 1.0f;
            D = 0.0f;
        }

        public Plane3D(float a, float b, float c, float d)
        {
            A = a; B = b; C = c; D = d;
            SetNormalize();
        }

        public Plane3D(double a, double b, double c, double d)
        {
            A = 0; B = 0;C = 0; D = 0;
            SetNormalize(a,b,c,d);
        }

        private void SetNormalize(double a, double b, double c, double d)
        {
            double dValue = a * a + b * b + c * c;
            if (dValue < 1e-6)
                return;
            dValue = Math.Sqrt(dValue);
            A = (float)(a / dValue);
            B = (float)(b / dValue);
            C = (float)(c / dValue);
            D = (float)(d / dValue);
        }


        public static Plane3D operator* (Plane3D surf,double a)
        {
            return new Plane3D((float)(a* surf.A), (float)(a * surf.B), (float)(a* surf.C), (float)(a * surf.D));
        }
        #endregion

        /// <summary>
        /// 计算直线与该平面的交点
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="xCross"></param>
        /// <param name="yCross"></param>
        /// <param name="zCross"></param>
        /// <returns></returns>
        public void CrossLine(Plane3D line1, Plane3D line2, ref double xCross, ref double yCross,
            ref double zCross)
        {
            List<double> matrix = new List<double>() { line1.A, line1.B, line1.C, line2.A, line2.B, line2.C, A, B, C };
            List<double> InvMatrix = null;
            List<double> result = null;
            cyUCSMeasure.MatrixInv3X3(matrix, out InvMatrix);
            List<double> mutArray = new List<double>() { line1.D, line2.D, D };

            cyUCSMeasure.Matrix_Mult3X1(InvMatrix, mutArray, out result);
            xCross = result[0]; yCross = result[1]; zCross = result[2];
        }

        public void SetNormalize()
        {
            double dValue = A * A + B * B + C * C;
            if (dValue < 1e-6)
                return;
            dValue = Math.Sqrt(dValue);
            A = (float)(A / dValue);
            B = (float)(B / dValue);
            C = (float)(C / dValue);
            D = (float)(D / dValue);
        }


        /// <summary>
        /// 计算点到面的距离， 已Z方向为上下 
        /// </summary>
        /// <param name="xValue"></param>
        /// <param name="yValue"></param>
        /// <param name="zValue"></param>
        /// <param name="dist"></param>
        public void GetDistance(double[] xValue, double[] yValue, double[] zValue, ref double[] dist)
        {
            if (xValue.Length <= 0)
                return;
            if (Math.Abs(A) < 1e-6 && Math.Abs(B) < 1e-6 && Math.Abs(C) < 1e-6)
                return;

            Trace.Assert(xValue.Length == yValue.Length && xValue.Length == zValue.Length);

            SetNormalize();

            if (dist.Length != xValue.Length)
                dist = new double[xValue.Length];


            for(int i = 0; i < xValue.Length; i++)
            {
                dist[i] = (A * xValue[0] + B * yValue[0] + C * zValue[0] - D) * (C < 0 ? -1 : 1);
            }
        }

        public double GetDistance(double xValue, double yValue, double zValue)
        {
            return (A * xValue + B * yValue + C * zValue - D) * (C < 0 ? -1 : 1);
        }

        /// <summary>
        /// 根据一个点求取一个垂线:
        /// 垂线方程：plane1 plane2 相交组成
        /// </summary>
        /// <param name="xValue"></param>
        /// <param name="yValue"></param>
        /// <param name="zValue"></param>
        /// <param name="plane1"></param>
        /// <param name="plane2"></param>
        public bool GetNormaline(double xValue, double yValue, double zValue, ref Plane3D plane1, ref Plane3D plane2)
        {
            if (Math.Abs(A) < 1e-6 && Math.Abs(B) < 1e-6 && Math.Abs(C) < 1e-6)
                return false;

            //垂线方程满足： A/(x-x0) = B/(y-y0) = C/(z-z0);
            plane1 = new Plane3D(B, -A, 0, (float)(B * xValue - A * yValue));
            plane2 = new Plane3D(C, 0, -A, (float)(C * xValue - A * zValue));
            return true;
        }

        /// <summary>
        /// 采用最小二乘法拟合一个平面
        /// </summary>
        /// <param name="xValue"></param>
        /// <param name="yValue"></param>
        /// <param name="zValue"></param>
        /// <returns></returns>
        public static Plane3D leastSquareAdaptFlatSurface(double[] xValue, double[] yValue, double[] zValue)
        {
            if (xValue.Length <= 0)
                return new Plane3D(0, 0, 0, 0);

            Trace.Assert(xValue.Length == yValue.Length && xValue.Length == zValue.Length);

            int nums = xValue.Length;
            /*
             *     [xx, xy, x   ]        [xz]
             * T = [xy, yy, y   ] ,  TB= [yz]
             *     [ x,  y, nums]        [ z]
             * 并且    T*[A,B, -D]^ = TB; C = -1
            */
            List<double> matrix = new List<double>(9) { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            List<double> ResltMatr = new List<double>(3) { 0, 0, 0 };
            for (int i = 0; i < xValue.Length; i++)
            {
                matrix[0] += xValue[i] * xValue[i];
                matrix[1] += xValue[i] * yValue[i];
                matrix[2] += xValue[i];
                matrix[4] += yValue[i] * yValue[i];
                matrix[5] += yValue[i];

                ResltMatr[0] += xValue[i] * zValue[i];
                ResltMatr[1] += yValue[i] * zValue[i];
                ResltMatr[2] += zValue[i];
            }
            matrix[3] = matrix[1]; matrix[6] = matrix[2]; matrix[7] = matrix[5];
            matrix[8] = nums;
            List<double> Inv_matrix;
            cyUCSMeasure.MatrixInv3X3(matrix, out Inv_matrix);

            List<double> surfData;// data: z = a*x + b*y +c;
            cyInspector.cyUCSMeasure.Matrix_Mult3X1(Inv_matrix, ResltMatr, out surfData);

            //将方程换算成标准形式 ax + by +cz = d;
            return new Plane3D(surfData[0], surfData[1], -1, -surfData[2]);
        }

        /// <summary>
        /// 用于计算某个面的平面度
        /// </summary>
        /// <param name="xValue"></param>
        /// <param name="yValue"></param>
        /// <param name="zValue"></param>
        /// <param name="flatness"></param>
        public float MeasureFlatness(double[] xArrays, double[] yArrays, double[] zArrays)
        {
            Trace.Assert(xArrays.Length == yArrays.Length && xArrays.Length == zArrays.Length);
            int nums = xArrays.Length;
            if (nums == 0)
                return -1;

            double minValue = Double.MaxValue;
            double maxValue = Double.MinValue;
            for(int i =0; i < nums; i++)
            {
                double dist1 = (A * xArrays[i] + B * yArrays[i] + C * zArrays[i] - D);
                if (maxValue < dist1)
                    maxValue = dist1;
                if (minValue > dist1)
                    minValue = dist1;
            }
            return (float)(maxValue - minValue);
        }

        /// <summary>
        /// 计算 一个面和相关拟合面的平行度；
        /// </summary>
        /// <param name="coor3D"></param>
        /// <param name="xArray"></param>
        /// <param name="yArray"></param>
        /// <param name="zArray"></param>
        /// <returns></returns>
        public double MeasureParallelism(Plane3D coor3D, double[] xArray, double[] yArray, double[] zArray)
        {
            Trace.Assert(xArray.Length == yArray.Length && xArray.Length == zArray.Length);
            if (xArray.Length <= 0)
                return -1;

            Plane3D plane1 = new Plane3D(0, 0, 0, 0);
            Plane3D plane2 = new Plane3D(0, 0, 0, 0);
            double maxValue = Double.MinValue;
            double minValue = Double.MaxValue;
            for (int t = 0; t < xArray.Length; t++)
            {
                //求垂线
                GetNormaline(xArray[t], yArray[t], zArray[t], ref plane1, ref plane2);
                double xCross = 0; double yCross = 0; double zCross = 0;
                //垂线与coorSurface的交点
                coor3D.CrossLine(plane1, plane2, ref xCross, ref yCross, ref zCross);

                double dist = GetDistance(xCross, yCross, zCross);
                if (dist < minValue)
                    minValue = dist;
                if (dist > maxValue)
                    maxValue = dist;
            }
            return maxValue - minValue;
        }
    }
}
