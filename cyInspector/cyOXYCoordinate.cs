using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cyOXYInspector;
namespace cyInspector
{
    public class cyOXYCoordinate: cyUCSMeasure
    {
        public cylineParam xline { private set; get; }
        public cylineParam yline { private set; get; }
        public cyPoint2d orig;

        public cyOXYCoordinate(cylineParam x, cylineParam y)
        {
            xline = x;
            yline = y;

            cyOXYLine.IntersectPoint(xline, yline, out orig);
        }

        //根据x方向和y方向的比例关系，换算出新的坐标系关系
        public cyOXYCoordinate OnRatioCoordinate(double xRatio, double yRation)
        {
            cylineParam newXline = new cylineParam() { A=0, B=0, C=0 };
            cylineParam newYline = new cylineParam() { A = 0, B = 0, C = 0 };
            newXline.A = xline.A * xRatio; newXline.B = xline.B * yRation; newXline.C = xline.C;
            newYline.A = yline.A * xRatio; newYline.B = yline.B * yRation; newYline.C = yline.C;
            double sqrt1 = 1/Math.Sqrt(newXline.A * newXline.A + newXline.B * newXline.B);
            newXline = newXline * sqrt1;
            sqrt1 = 1 / Math.Sqrt(newYline.A * newYline.A + newYline.B * newYline.B);
            newYline = newYline * sqrt1;
            return new cyOXYCoordinate(newXline, newYline);
        }

        public cylineParam OnXPosLine(double xBias)
        {
            //做一条x = A的直线
            cyPoint2d localPoint;
            OnCoordinatePoint2Image(new cyPoint2d() { x = xBias, y = 0 }, out localPoint);
            cylineParam line = yline;
            line.C = line.A * localPoint.x + line.B * localPoint.y;
            return line;
        }

        public cylineParam OnYPosLine(double yBias)
        {
            cyPoint2d localPoint;
            OnCoordinatePoint2Image(new cyPoint2d() { y = yBias, x = 0 }, out localPoint);
            cylineParam line = xline;
            line.C = line.A * localPoint.x + line.B * localPoint.y;
            return line;
        }
        /// <summary>
        /// 将坐标的原点，移动到相对应的点上，作为原点。
        /// 也就是将坐标系中的点（x,y） 移动到 （0，0）， 也就是 两条直线的交点处。
        /// </summary>
        /// <param name="coorlocalPoint"></param>
        public void SetCoordinateRelativateOrig(cyPoint2d  PointValue)
        {
            cyPoint2d imgPoint1;
            OnCoordinatePoint2Image(PointValue, out imgPoint1);
            List<double> matrix = new List<double>(4) { xline.A, xline.B, yline.A, yline.B };
            List<double> outResult;
            Matrix_Mult2X1(matrix, imgPoint1.ToList(), out outResult);
            xline = new cylineParam() { A = xline.A, B = xline.B, C = outResult[0] };
            yline = new cylineParam() { A = yline.A, B = yline.B, C = outResult[1] };
            cyOXYLine.IntersectPoint(xline, yline, out orig);
        }

        public void SetCoordinateImageOrig(cyPoint2d orgPoint)
        {
            cylineParam line1 = xline;
            line1.C = xline.A * orgPoint.x + xline.B * orgPoint.y;
            xline = line1;
            line1 = yline;
            line1.C = yline.A * orgPoint.x + yline.B * orgPoint.y;
            yline = line1;

            orig = orgPoint;
        }

        /// <summary>
        /// 根据指定的点的图像中的坐标， 修正坐标直线的方向
        /// </summary>
        /// <param name="checkPoint">测试点</param>
        /// <param name="partIndex">测试点所在象限</param>
        public void OnMdodifyCoordilateXy(cyPoint2d checkPoint, int partIndex)
        {
            cyPoint2d coorPoint;
            OnPoint2Coordinate(checkPoint, out coorPoint);

            switch (partIndex)
            {
                case 1://(x > 0, y > 0 )
                    {
                        if (coorPoint.x < 0)
                            yline = yline * (-1);
                        if (coorPoint.y < 0)
                            xline = xline * (-1);
                        break;
                    }
                case 2://(x < 0, y > 0)
                    {
                        //1,4
                        if (coorPoint.x > 0)
                            yline = yline * (-1);
                        if (coorPoint.y < 0)
                            xline = xline * (-1);
                        break;
                    }
                case 3://(x <0, y < 0)
                    {
                        if (coorPoint.x > 0)
                            yline = yline * (-1);
                        if (coorPoint.y > 0)
                            xline = xline * (-1);
                        break;
                    }
                case 4://(x > 0, y < 0)
                    {
                        if (coorPoint.x < 0)
                            yline = xline * (-1);
                        if (coorPoint.y > 0)
                            xline = yline * (-1);
                        break;
                    }
                default: break;
            }
            OnPoint2Coordinate(checkPoint, out coorPoint);
        }

        //将坐标中的点转回到图像中去 
        public void OnCoordinatePoint2Image(cyPoint2d coorPoint, out cyPoint2d imgPoint)
        {
            imgPoint = new cyPoint2d();

            List<double> matrix = new List<double>(4) { yline.A, xline.A, yline.B, xline.B };
            List<double> Tmatrix = new List<double>(2) { coorPoint.x, coorPoint.y };
            List<double> addBias = new List<double>(2) { orig.x, orig.y};

            List<double> result;
            //matrix*T + addBias => result
            Matrix_Mult2X1(matrix, Tmatrix, addBias, out result);
            imgPoint.x = result[0];
            imgPoint.y = result[1];
        }

        public cyPoint2d OnCoordinatePoint2Image(cyPoint2d coorPoint)
        {
            cyPoint2d imgPoint = new cyPoint2d();

            List<double> matrix = new List<double>(4) { yline.A, xline.A, yline.B, xline.B };
            List<double> Tmatrix = new List<double>(2) { coorPoint.x, coorPoint.y };
            List<double> addBias = new List<double>(2) { orig.x, orig.y };

            List<double> result;
            //matrix*T + addBias => result
            Matrix_Mult2X1(matrix, Tmatrix, addBias, out result);
            imgPoint.x = result[0];
            imgPoint.y = result[1];
            return imgPoint;
        }

        // 将图像中的点转到坐标系中对应的点中去
        public void OnPoint2Coordinate(cyPoint2d oldPoint, out cyPoint2d coordinatePoint)
        {
            List<double> matrix = new List<double>(4) { yline.A, xline.A, yline.B, xline.B };
            List<double> Inv_matrix;
            MatrixInv2X2(matrix, out Inv_matrix);
            cyPoint2d arrayValue = oldPoint - orig;
            List<double> dat = new List<double>() { arrayValue.x, arrayValue.y };
            List<double> result ;
            Matrix_Mult2X1(Inv_matrix, dat, out result);
            coordinatePoint = new cyPoint2d() { x = result[0], y = result[1] };
        }

        public cyPoint2d OnPoint2Coordinate(cyPoint2d oldPoint )
        {
            List<double> matrix = new List<double>(4) { yline.A, xline.A, yline.B, xline.B };
            List<double> Inv_matrix;
            MatrixInv2X2(matrix, out Inv_matrix);
            cyPoint2d arrayValue = oldPoint - orig;
            List<double> dat = new List<double>() { arrayValue.x, arrayValue.y };
            List<double> result;
            Matrix_Mult2X1(Inv_matrix, dat, out result);
            return new cyPoint2d() { x = result[0], y = result[1] };
        }
    }
}
