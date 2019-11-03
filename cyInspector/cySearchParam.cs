using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyInspector
{
    /// <summary>
    /// 直线参数
    /// </summary>
    public struct cylineParam
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }

        public cylineParam(cyPoint2d point1, cyPoint2d point2)
        {
            A = (point1.y - point2.y);
            B = (point2.x - point1.x);
            C = ((point2.x - point1.x) * point1.y - (point2.y - point1.y) * point1.x);
            double meanValue = Math.Sqrt(A * A + B * B);
            if(meanValue <= Double.Epsilon)
            {
                A = 0; B = 0;C = 0;
            }
            else
            {
                A /= meanValue;B /= meanValue; C /= meanValue;
            }
        }

        public static cylineParam operator *(cylineParam a, double b)
        {
            return new cylineParam() { A = a.A * b, B = a.B * b, C = a.C * b };
        }

        public void OnNormalize()
        {
            double t = 1 / Math.Sqrt(A * A + B * B);
            A *= t; B *= t; C *= t;
        }
    }

    public struct cyCircleParam
    {
        public double xCenter { get; set; }
        public double yCenter { get; set; }
        public double radius { get; set; }
    }


    /// <summary>
    /// oXYZ 平面方程参数
    /// </summary>
    public struct cyPlanSurfaceParam
    {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
    }

    /// <summary>
    /// 表示搜索直线的时候，搜索的方向
    /// </summary>
    public enum cySearchDirect
    {
        //顺时针方向
        Left = 0, // 往左走
        Right = 1, // 往右走
        Up = 2,     //在图像中往上走
        Down = 3   //往下走
    }

    //搜索圆的方向
    public enum cySearchCirDirect
    {
        //从内往外
        Inner = 0,
        //往外往内
        Outer = 1
    }

    public enum cyEnumPointSel
    {
        First = 0,
        Last = 1,
        Best = 2,
        Mid = 3,
        All = 4,
        Other = -1
    }

    /// <summary>
    /// 对采取到的点做一定的处理后求梯度
    /// </summary>
    public enum cyEnumPointSmooth
    {
        GaussSmooth = 0, // 高斯平滑后再求梯度
        AbsGradiant = 1, // 绝对梯度值
    }


    public struct cyCenterSearch
    {
        public double x;
        public double y;
        public double angle;
        public void Init() { x = 0.0d; y = 0.0d; angle = 0.0d; }

        public cyCenterSearch(double _x, double _y, double _angle)
        {
            x = _x; y = _y; angle = _angle;
        }
    }

}
