using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using cyOXYInspector;
using cyXYZInspector;

namespace cyInspector
{
    /// <summary>
    /// 用于做测量的算法

    /// 在 O-XYZ 坐标系中， 平面方程： A*x + B*y + C*z = D; 且 A^2 + B^2 + C^2 = 1
    /// 在 O-XYZ 坐标系中
    /// </summary>
    public class MetorAlg
    {
        #region _2DMeasure
        /// 
        /// <summary>
        /// 根据 mainline coorline ,两条直线方程（坐标直线），mainline 和交点， 并修正 coorline， 两条直线求交点，
        /// </summary>
        /// <param name="mainline"></param>
        /// <param name="coorline"></param>
        /// <returns></returns>
        public void  JustifyCoordinateLine(ref cyOXYLine mainline, ref cyOXYLine coorline, out double xCross, out double yCross)
        {

            cyPoint2d point;
            mainline.IntersectPoint(coorline.OnGetLineParam(), out point);
            xCross = point.x;
            yCross = point.y;

            coorline = mainline.GetVecticalline(xCross, yCross);
        }

        /// <summary>
        /// 根据拟合的两条直线，mainline ，coorline， 并取拟合coorline上某些点，做mainline的垂线，与coorline相交、
        /// 交点到直线mainline的距离， 根据【rows ,cols】的比例关系，换算成实际的距离尺寸，预期后期要做更改
        /// Date：2019 08 16
        /// </summary>
        /// <param name="mainline">基准线</param>
        /// <param name="coorline">测量线</param>
        /// <param name="measureRatio">图像方向上的坐标矩阵， 目前只用了x方向和y方向的，后期需要增加一些补偿因子等</param>
        /// <param name="xCoorPoint">拟合测量线用的点坐标 X， Image中的</param>
        /// <param name="yCoorPoint">拟合测量线用的点坐标 Y， Image中的</param>
        /// <param name="dist">计算得到的距离，后期看是算最大值还是最小值，还是平均值</param>
        /// <returns>当直线合理的时候，将返回true</returns>
        public static bool CalculateDistanceOfLine(ref cylineParam mainline, ref cylineParam coorline, double[] measureRatio, double[] xCoorPoint, double[] yCoorPoint, out float[] dist)
        {
            dist = new float[yCoorPoint.Length];
            if (xCoorPoint.Length != yCoorPoint.Length || xCoorPoint.Length <= 0)
                return false;
            
            double div1 = coorline.B * mainline.B + coorline.A * mainline.A;
            for (int i = 0; i < xCoorPoint.Length; i++)
            {
                double c_justify = coorline.B * xCoorPoint[i] - coorline.A * yCoorPoint[i];
                double value1 = (mainline.B * (c_justify) + coorline.A * mainline.C) / div1;
                double value2 = (-mainline.A * c_justify + coorline.B * mainline.C) / div1;

                double r1 = coorline.A * measureRatio[0];
                double r2 = coorline.B * measureRatio[1];
                //double ratio = Math.Sqrt((r1 * r1 + r2 * r2) / 2);
                //这个比例系数需要修正，但哪种修正方式合适，目前还没有想出合理的方式， 
                //double ratio = r1;
                double ratio = measureRatio[0];
                double dist1 = coorline.A * value1 + coorline.B * value2 - coorline.C;
                dist[i] = (float)(Math.Abs(dist1) * ratio);
            }
            return true;
        }


        /// <summary>
        /// 根据在图像中的位置，计算建立的坐标的坐标值
        /// </summary>
        /// <param name="xlineArray"> 坐标系x的方程</param>
        /// <param name="ylineArray"> 坐标系y的方程</param>
        /// <param name="xPos"> 图像坐标系中x的值，也就是cols的值</param>
        /// <param name="yPos">图像坐标系中y的值， 也就是rows的值</param>
        /// <param name="measureRatio">测量的转换矩阵</param>
        /// <param name="corXpos">世界坐标系中，对应于我图像坐标系中 检测点的X坐标值</param>
        /// <param name="corYpos">世界坐标系中，对应于我图像坐标系中 检测点的y坐标值</param>
        public void CalculateCoorDist(ref cyOXYLine xlineArray, ref cyOXYLine ylineArray, double[] xPos, double[] yPos, double[] measureRatio,ref double[] corXpos, ref double[] corYpos)
        {
            // 待计算
            if (corXpos == null)
                corXpos = new double[xPos.Length];
            if (corYpos == null)
                corYpos = new double[xPos.Length];
        }
        #endregion

        #region _3DMeasure
        public bool leastSquareAdaptFlatSurface()
        {
            return true;
        }
        #endregion
    }
}
