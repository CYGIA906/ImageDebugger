using System;
using System.Diagnostics;

namespace cyInspector
{
    /// <summary>
    /// 旋转矩阵：
    ///     [ cosV ,-sinV, 0]            [ X']     [ x-rotX ]    [rotX - offsetX]
    /// T = [ sinV, cosV,  0]  计算公式: [ Y'] = T*[ y-rotY ]  + [rotY -offsetY];
    ///     [  0,     0,   1]            [ 1 ]     [    1   ]    [ 0  ]
    /// 将新图像中的点转换回去的逆矩阵为：T^(-1) = 
    ///           [ cosV , sinV, 0]
    ///  T^(-1) = [ sinV , cosV, 0]
    ///           [  0   ,   0  ,1]
    /// </summary>
    public class MatImageTraslatePoint
    {
        private double _rotX { get; set; }
        private double _rotY { get; set; }

        private double[] _imgMatrix_trans = null;

        private double[] _imgMatrix_Restrans = null;
        /// <summary>
        /// 计算偏移要用到的方向
        /// </summary>
        private double[] _offsetVal = new double[2];

        private double[] _rotPoint = new double[2];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rAngle"></param>
        /// <param name="rotPos">(X,Y)</param>
        /// <param name="offset"></param>
        public MatImageTraslatePoint(double rAngle, double[] rotPos, double[] offset)
        {
            //旋转矩阵
            _imgMatrix_trans = new double[6];
            _imgMatrix_Restrans = new double[6];
            double cosV = Math.Cos(rAngle);
            double sinV = Math.Sin(rAngle);

            //正矩阵
            _imgMatrix_trans[0] = cosV;_imgMatrix_trans[1] = -sinV; _imgMatrix_trans[2] = 0;
            _imgMatrix_trans[3] = sinV; _imgMatrix_trans[4] = cosV; _imgMatrix_trans[5] = 0;

            //逆矩阵
            _imgMatrix_Restrans[0] = cosV; _imgMatrix_Restrans[1] = sinV; _imgMatrix_Restrans[2] = 0;
            _imgMatrix_Restrans[3] = -sinV; _imgMatrix_Restrans[4] = cosV; _imgMatrix_Restrans[5] = 0;


            _rotPoint[0] = rotPos[0];
            _rotPoint[1] = rotPos[1];

            _offsetVal[0] = offset[0];
            _offsetVal[1] = offset[1];
        }

        public MatImageTraslatePoint()
        {
            //旋转矩阵
            _imgMatrix_trans = new double[6];
            _imgMatrix_Restrans = new double[6];
            double cosV = Math.Cos(0);
            double sinV = Math.Sin(0);

            //正矩阵
            _imgMatrix_trans[0] = cosV; _imgMatrix_trans[1] = -sinV; _imgMatrix_trans[2] = 0;
            _imgMatrix_trans[3] = sinV; _imgMatrix_trans[4] = cosV; _imgMatrix_trans[5] = 0;
        }

        public void SetRotPos(double[] rotPos)
        {
            _rotPoint[0] = rotPos[0];
            _rotPoint[1] = rotPos[1];
        }

        public void SetAngel(double rAngle)
        {
            double cosV = Math.Cos(rAngle);
            double sinV = Math.Sin(rAngle);

            //正矩阵
            _imgMatrix_trans[0] = cosV; _imgMatrix_trans[1] = -sinV; _imgMatrix_trans[2] = 0;
            _imgMatrix_trans[3] = sinV; _imgMatrix_trans[4] = cosV; _imgMatrix_trans[5] = 0;

            //逆矩阵
            _imgMatrix_Restrans[0] = cosV; _imgMatrix_Restrans[1] = sinV; _imgMatrix_Restrans[2] = 0;
            _imgMatrix_Restrans[3] = -sinV; _imgMatrix_Restrans[4] = cosV; _imgMatrix_Restrans[5] = 0;
        }

        public void SetOffsetVal(double[] offset)
        {
            _offsetVal[0] = offset[0];
            _offsetVal[1] = offset[1];
        }

        /// <summary>
        /// 正变换 将图像中的点， 根据旋转后，将旋转后的坐标点转换回原始的图像坐标中去
        /// </summary>
        /// <param name="srcPointX"></param>
        /// <param name="srcPointY"></param>
        /// <param name="dstPointX"></param>
        /// <param name="dstPointY"></param>
        public void ImageTranslatePoint(double[] srcPointX, double[] srcPointY, out double[] dstPointX, out double[] dstPointY)
        {
            int lenth = srcPointX.Length;
            if (lenth <=0)
            {
                dstPointX = null;
                dstPointY = null;
                return;
            }
            dstPointX = new double[lenth];
            dstPointY = new double[lenth];
            for (int i = 0; i < lenth; i++)
            {
                dstPointX[i] = _imgMatrix_trans[0] * (srcPointX[i] - _rotPoint[0]) + 
                    _imgMatrix_trans[1] * (srcPointY[i] - _rotPoint[1]) + _rotPoint[0] - _offsetVal[0];

                dstPointY[i] = _imgMatrix_trans[3] * (srcPointX[i] - _rotPoint[0]) +
                    _imgMatrix_trans[4] * (srcPointY[i] - _rotPoint[1]) + _rotPoint[1] - _offsetVal[1];
            }
        }
        public void ImageTranslatePoint(double srcPointX, double srcPointY, out double dstPointX, out double dstPointY)
        {
            dstPointX = _imgMatrix_trans[0] * (srcPointX - _rotPoint[0]) +
                _imgMatrix_trans[1] * (srcPointY - _rotPoint[1]) + _rotPoint[0] - _offsetVal[0];

            dstPointY = _imgMatrix_trans[3] * (srcPointX - _rotPoint[0]) +
                _imgMatrix_trans[4] * (srcPointY - _rotPoint[1]) + _rotPoint[1] - _offsetVal[1];
        }

        /// <summary>
        /// 将旋转后的坐标点转换回原始图像坐标中去
        /// </summary>
        /// <param name="srcPointX"></param>
        /// <param name="srcPointY"></param>
        /// <param name="dstPointX"></param>
        /// <param name="dstPointY"></param>
        public void ImageRetransferPoint(double[] srcPointX, double[] srcPointY, out double[] dstPointX, out double[] dstPointY)
        {
            int lenth = srcPointX.Length;
            if (lenth <= 0)
            {
                dstPointX = null;
                dstPointY = null;
                return;
            }
            dstPointX = new double[lenth];
            dstPointY = new double[lenth];

            double rotX = _rotPoint[0] - _offsetVal[0];
            double rotY = _rotPoint[1] - _offsetVal[1];

            for (int i = 0; i < lenth; i++)
            {
                dstPointX[i] = _imgMatrix_Restrans[0] * (srcPointX[i] - rotX) +
                    _imgMatrix_Restrans[1] * (srcPointY[i] - rotY) + _rotPoint[0];

                dstPointY[i] = _imgMatrix_Restrans[3] * (srcPointX[i] - rotX) +
                    _imgMatrix_Restrans[4] * (srcPointY[i] - rotY) + _rotPoint[1];
            }
        }
    }
}
