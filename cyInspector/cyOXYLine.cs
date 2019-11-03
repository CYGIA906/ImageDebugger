using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using cyInspector;


namespace cyOXYInspector
{

    /// <summary>
    ///   在XOY坐标系中： 直线 A*x + B*y = C; 且满足 A^2 + B^2 = 1 或者 A^2 + B^2 = 0
    ///   所有的计算的直线，都是float型数据
    /// </summary>
    public class cyOXYLine : cyUCSMeasure
    {
        #region Param
        public float A { private set; get; }
        public float B { private set; get; }
        public float C { private set; get; }
        public float angle
        {
            get
            {
                return (Math.Abs(A) <= 1e-6 && Math.Abs(B) <= 1e-6) ? 0 : (float)(Math.Atan2(B, A));
            }
        }
        #endregion

        #region Create
        public cyOXYLine()
        {
            A = 0.0f;
            B = 1.0f;
            C = 0.0f;
        }

        public cyOXYLine(cylineParam lineParam)
        {
            A = (float)lineParam.A;
            B = (float)lineParam.B;
            C = (float)lineParam.C;
        }

        public cyOXYLine(float a, float b, float c)
        {
            A = a;
            B = b;
            C = c;
            SetNormalize();
        }

        public cyOXYLine(double a, double b, double c)
        {
            SetNormalize(a, b, c);
        }

        private void SetNormalize(double a, double b, double c)
        {
            A = 0; B = 0; C = 0;
            if (Math.Abs(a) <= 1e-6 && Math.Abs(b) <= 1e-6)
                return;

            double dValue = a * a + b * b;
            dValue = Math.Sqrt(dValue);
            A = (float)(a / dValue);
            B = (float)(b / dValue);
            C = (float)(c / dValue);
        }

        public cylineParam OnGetLineParam()
        {
            cylineParam line = new cylineParam();
            line.A = A;
            line.B = B;
            line.C = C;

            return line;
        }

        #endregion

        #region Feature
        /// <summary>
        /// 判断是否与a 垂直
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public bool OnVertical(cyOXYLine a)
        {
            float tValue = a.A * this.A + this.B * this.B;
            return Math.Abs(tValue) <= 1e-6 ? true : false;
        }

        public static bool IsVertical(cylineParam mainLine, cylineParam measureLine)
        {
            double tValue = mainLine.A * measureLine.A + mainLine.B * measureLine.B;
            return Math.Abs(tValue) <= Double.Epsilon ? true : false;
        }

        /// <summary>
        /// 根据所给的点， 过这个点，得到一条垂线
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <returns></returns>
        public cyOXYLine GetVecticalline(double px, double py)
        {
            float c2 = (float)(A * py - B * px);
            return new cyOXYLine(B, -A, c2);
        }

        public cyOXYLine GetVecticalline(cyPoint2d point)
        {
            float c2 = (float)(A * point.y - B * point.x);
            return new cyOXYLine(B, -A, c2);
        }


        public static cylineParam GetVecticalLine(cylineParam mainLine, double xpoint, double ypoint)
        {
            double c2 = (mainLine.A * ypoint - mainLine.B * xpoint);

            return new cylineParam() { A = -mainLine.B, B = mainLine.A, C = c2 };
        }

        public static cylineParam GetVecticalLine(cylineParam mainLine, cyPoint2d point)
        {
            double c2 = (mainLine.A * point.y - mainLine.B * point.x);

            return new cylineParam() { A = -mainLine.B, B = mainLine.A, C = c2 };
        }



        public static bool IntersectPoint(cylineParam line1, cylineParam interline, out cyPoint2d crossPoint)
        {
            crossPoint = new cyPoint2d() { x = 0, y = 0 };
            List<double> matrix = new List<double>() { line1.A, line1.B, interline.A, interline.B };
            List<double> mult = new List<double>() { line1.C, interline.C };
            List<double> inv1, result;
            cyUCSMeasure.MatrixInv2X2(matrix, out inv1);
            cyUCSMeasure.Matrix_Mult2X1(inv1, mult, out result);

            crossPoint.x = result[0];
            crossPoint.y = result[1];
            return true;
        }

        public static cyPoint2d IntersectPoint(cylineParam line1, cylineParam interline)
        {
            cyPoint2d crossPoint = new cyPoint2d() { x = 0, y = 0 };
            List<double> matrix = new List<double>() { line1.A, line1.B, interline.A, interline.B };
            List<double> mult = new List<double>() { line1.C, interline.C };
            List<double> inv1, result;
            cyUCSMeasure.MatrixInv2X2(matrix, out inv1);
            cyUCSMeasure.Matrix_Mult2X1(inv1, mult, out result);

            crossPoint.x = result[0];
            crossPoint.y = result[1];
            return crossPoint;
        }



        public bool IntersectPoint(cylineParam line1, out cyPoint2d crossPoint)
        {
            crossPoint = new cyPoint2d() { x = 0, y = 0 };
            double condition = line1.A * B - A * line1.B;
            if (Math.Abs(condition) < 1e-6)
            {
                return false;
            }

            crossPoint.x = ((B * line1.C - line1.B * C) / condition);
            crossPoint.y = ((-A * line1.C + line1.A * C) / condition);
            return true;
        }


        /// <summary>
        /// 将直线方程转换成标准的 A^2 + B^2 =1
        /// </summary>
        /// <returns></returns>
        public bool SetNormalize()
        {
            if (Math.Abs(A) <= 1e-6 && Math.Abs(B) <= 1e-6)
                return false;

            double dValue = A * A + B * B;
            dValue = Math.Sqrt(dValue);
            A = (float)(A / dValue);
            B = (float)(B / dValue);
            C = (float)(C / dValue);
            return true;
        }


        public cyOXYLine GetParallel(double x, double y)
        {
            return new cyOXYLine(A, B, (float)(A * x + B * y));
        }

        public cylineParam GetLineParam()
        {
            return new cylineParam() { A = this.A, B = this.B, C = this.C };
        }

        /// <summary>
        /// 判断两条直线是否平行
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public bool GetParallel(cyOXYLine a)
        {
            double tValue = A * a.B - B * a.A;
            return Math.Abs(tValue) <= 1e-16;
        }

        /// <summary>
        /// 计算两个直线的夹角是多少
        /// </summary>
        /// <param name="line1"></param>
        /// <returns></returns>
        public double GetAngle(cyOXYLine line1)
        {
            double xArr = line1.A - A;
            double yArr = line1.B - B;

            double angle = Math.Atan2(yArr, xArr);
            return angle;
        }

        /// <summary>
        /// 根据旋转矩阵，对直线进行旋转, 
        /// 旋转中心点：(0,0)点
        /// 旋转矩阵为：Tr = [cosθ, -sinθ, Xoffset];  
        ///                  [sinθ, cosθ, Yoffset];   
        ///                  [    0,     0,      1];  
        ///    θ为：O-XY，中的夹角       
        /// 直线系数[A,B,C], 原始的直线系数为：[a,b,c]
        /// [A,B,-C] = [a,b,-c]*Tr;(因为还是在原始的坐标系中， x', y'都是过渡量，还是得转换成x,y的方程)
        /// </summary>
        /// <param name="transArray">旋转矩阵，</param>
        /// <param name="pointX">旋转点坐标X</param>
        /// <param name="pointY">旋转点坐标Y</param>
        public cyOXYLine OnTranslateCoorPoint(double angle, double pointX, double pointY)
        {
            double cosV = Math.Cos(angle), sinV = Math.Sin(angle);
            double A1 = A * cosV - B * sinV;
            double B1 = A * sinV + B * cosV;
            double C1 = C - ((A * pointX) + B * (pointY));

            //计算某个特定的值

            cyOXYLine newline = new cyOXYLine(A1, B1, C1);

            newline.OnMove(-pointX, -pointY);
            return newline;
        }


        /// <summary>
        /// 以（pointX， pointY）为圆心，旋转 angle（rad）后，得到的新直线方程
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="pointX"></param>
        /// <param name="pointY"></param>
        /// <returns></returns>
        public cyOXYLine OnTranslateCoorPoint2(double angle, double pointX, double pointY)
        {
            double cosV = Math.Cos(angle), sinV = Math.Sin(angle);
            double t13 = -pointY * sinV - pointX * cosV;
            double t23 = -pointY * cosV + pointX * sinV;
            double off1 = A * pointX + B * pointY;

            double A1 = A * cosV - B * sinV;
            double B1 = A * sinV + B * cosV;
            double C1 = -(A * t13 + B * t23 - C + off1);

            return new cyOXYLine(A1, B1, C1);
        }

        public void OnMove(double Xoffset, double Yoffset)
        {
            C -= (float)(A * Xoffset + B * Yoffset);
        }

        public cyOXYLine ToGetVectorLine(double[] xValue, double[] yValue)
        {
            Trace.Assert(xValue.Length == yValue.Length);

            double[] avg = new double[2] { 0, 0 };
            for (int i = 0; i < xValue.Length; i++)
            {
                double c2 = -B * xValue[i] + A * yValue[i];
                //计算映射点， 然后求平均
                avg[0] += A * C - B * c2;
                avg[1] += B * C + A * c2;
            }

            avg[0] /= xValue.Length;
            avg[1] /= xValue.Length;

            //计算Y值
            double C1 = A * avg[1] - B * avg[0];
            return new cyOXYLine(-B, A, C1);
        }

        public static cylineParam ToGetVectorLine(double[] xValue, double[] yValue, cylineParam line1)
        {
            Trace.Assert(xValue.Length == yValue.Length);

            cylineParam result =  new cylineParam() { A = -line1.B, B = line1.A, C = 0};
            if (xValue.Length == 0)
                return result;
            for (int i = 0; i < xValue.Length; i++)
            {
                result.C += result.A * xValue[i] + result.B * yValue[i];
            }
    
            result.C /= xValue.Length;
            return result;
        }



        public static double GetPointDistance(cylineParam lineParam, double pointX, double pointY)
        {
            //保证 lineParam  的系数为 平方为 1
            //double subValue = Math.Sqrt(lineParam.A * lineParam.A + lineParam.B * lineParam.B);
            double pValue = lineParam.A * pointX + lineParam.B * pointY - lineParam.C;
            if (Math.Abs(lineParam.A) <= 1e-6 && Math.Abs(lineParam.B) <= 1e-6)
                return 0;
            else
                return Math.Abs(pValue);


        }

        #endregion

        #region operator
        public static cyOXYLine operator *(cyOXYLine line, double a)
        {
            cyOXYLine obj = new cyOXYLine();
            obj.A = (float)(line.A * a);
            obj.B = (float)(line.B * a);
            obj.C = (float)(line.C * a);
            return obj;
        }

        /// <summary>
        /// 将像素坐标系中的直线转换到real中， 注意， xRes = Image/ real 的比例
        /// </summary>
        /// <param name="line"></param>
        /// <param name="xRes"></param>
        /// <param name="yRes"></param>
        /// <returns></returns>
        public static cylineParam OnRatioLine(cylineParam line, double xRes, double yRes)
        {
            cylineParam line1 = new cylineParam { A = line.A * xRes, B = line.B * yRes, C = line.C };
            line1 = line1 * (1 / Math.Sqrt(line1.A * line1.A + line1.B * line1.B));
            return line1;
        }

        public static  cylineParam OnGetLine(cyPoint2d point1, cyPoint2d point2)
        {
            cylineParam line1 = new cylineParam();
            line1.A = point1.y - point2.y;
            line1.B = -(point1.x - point2.x);
            line1.C = line1.A * point1.x + line1.B * point1.y;
            line1.OnNormalize();
            return line1;
        }

        /// <summary>
        /// 根据直线方程和contour ，计算交点；
        /// </summary>
        /// <param name="contourX"></param>
        /// <param name="contourY"></param>
        /// <param name="lineParam"></param>
        /// <param name="interSectionX"></param>
        /// <param name="interSectionY"></param>
        public static void OnInterSectionContours(List<double> contourX, List<double> contourY, cylineParam lineParam, out List<double> interSectionX, out List<double> interSectionY)
        {
            interSectionX = new List<double>();
            interSectionY = new List<double>();
            if (contourX.Count == 0)
                return;
            double value1 = lineParam.C - contourX[0] * lineParam.A - contourY[0] * lineParam.B;
            int direct = 0;//零为正， 1 为负
            if(Math.Abs(value1) < 1e-8)
            {
                interSectionX.Add(contourX[0]); interSectionY.Add(contourY[0]);
            }
            for(int i = 1; i < contourX.Count; i++)
            {
                double value2 = lineParam.C - contourX[0] * lineParam.A - contourY[0] * lineParam.B;
                if ((value2 < 0 && value1 > 0) || (value2 > 0 && value1 < 0))
                {
                    cylineParam line1 = OnGetLine(new cyPoint2d() { x = contourX[i], y = contourY[i] }, new cyPoint2d() { x = contourX[i - 1], y = contourY[i - 1] });
                    cyPoint2d cross = IntersectPoint(line1, lineParam);
                    interSectionX.Add(cross.x);
                    interSectionY.Add(cross.y);
                }
                else if(value2 == 0)
                {
                    interSectionX.Add(contourX[i]);
                    interSectionY.Add(contourY[i]);
                }
                value1 = value2;
            }
        }

        public static void OnInterSectionContours(double[] contourX, double[] contourY, cylineParam lineParam, out List<double> interSectionX, out List<double> interSectionY)
        {
            interSectionX = new List<double>();
            interSectionY = new List<double>();
            if (contourX.Length == 0)
                return;
            double value1 = lineParam.C - contourX[0] * lineParam.A - contourY[0] * lineParam.B;
            int direct = 0;//零为正， 1 为负
            if (Math.Abs(value1) < 1e-8)
            {
                interSectionX.Add(contourX[0]); interSectionY.Add(contourY[0]);
            }
            for (int i = 1; i < contourX.Length; i++)
            {
                double value2 = lineParam.C - contourX[i] * lineParam.A - contourY[i] * lineParam.B;
                if ((value2 < 0 && value1 > 0) || (value2 > 0 && value1 < 0))
                {
                    //if (Math.Abs(value2) < Math.Abs(value1))
                    //{ interSectionX.Add(contourX[i]); interSectionY.Add(contourY[i]); }
                    //else
                    //{ interSectionX.Add(contourX[i - 1]); interSectionY.Add(contourY[i - 1]); }
                    cylineParam line1 = OnGetLine(new cyPoint2d() { x = contourX[i], y = contourY[i] }, new cyPoint2d() { x = contourX[i - 1], y = contourY[i - 1] });
                    cyPoint2d cross = IntersectPoint(line1, lineParam);
                    interSectionX.Add(cross.x);
                    interSectionY.Add(cross.y);
                }
                value1 = value2;
            }
        }


        public static void OnInterSectionContours2(double[] contourX, double[] contourY, cylineParam lineParam, out List<double> interSectionX, out List<double> interSectionY)
        {
            interSectionX = new List<double>();
            interSectionY = new List<double>();
            if (contourX.Length == 0)
                return;
            double value1 = lineParam.C - contourX[0] * lineParam.A - contourY[0] * lineParam.B;
            int direct = 0;//零为正， 1 为负
            if (Math.Abs(value1) < 1e-8)
            {
                interSectionX.Add(contourX[0]); interSectionY.Add(contourY[0]);
            }
            for (int i = 1; i < contourX.Length; i+=2)
            {
                double value2 = lineParam.C - contourX[i] * lineParam.A - contourY[i] * lineParam.B;
                if ((value2 < 0 && value1 > 0) || (value2 > 0 && value1 < 0))
                {
                    List<double> xpos = new List<double>(); List< double > ypos = new List<double>();
                    for(int t = -15; t< 15; t+=2)
                    {
                        int index = i + t;
                        if (index < 0)
                            index += contourX.Count();
                        else if (index >= contourX.Count())
                            index -= contourX.Count();
                        xpos.Add(contourX[index]); ypos.Add(contourY[index]);
                    }
                    cylineParam line1;
                    leastSquareAdaptLineFast(xpos, ypos, out line1);
                    cyPoint2d cross = IntersectPoint(line1, lineParam);
                    interSectionX.Add(cross.x);
                    interSectionY.Add(cross.y);
                }
                else if(Math.Abs(value2) <= Double.MinValue*10)
                {
                    interSectionX.Add(contourX[i]); interSectionY.Add(contourY[i]);
                }
                value1 = value2;
            }
        }


        public static void GetInterSectionContoursIndex(double[] contourX, double[] contourY, cylineParam lineParam, out List<int> crossIndex)
        {
            crossIndex = new List<int>();
            if (contourX.Length == 0)
                return;
            double value1 = lineParam.C - contourX[0] * lineParam.A - contourY[0] * lineParam.B;
            int direct = 0;//零为正， 1 为负
            if (Math.Abs(value1) < 1e-8)
            {
                crossIndex.Add(0);
            }
            for (int i = 1; i < contourX.Length; i++)
            {
                double value2 = lineParam.C - contourX[i] * lineParam.A - contourY[i] * lineParam.B;
                if ((value2 < 0 && value1 > 0) || (value2 > 0 && value1 < 0))
                {
                    crossIndex.Add(i);
                }
                value1 = value2;
            }
        }

        /// <summary>
        /// 遍历一条直线距离这个区域最小距离的点
        /// </summary>
        /// <param name="line"></param>
        /// <param name="xArray"></param>
        /// <param name="yArray"></param>
        public static int SearchClosingLinePoint(cylineParam line, double[] xArray, double[] yArray)
        {
            //List<double> dist = new List<double>();
            double minValue = Double.MaxValue;
            int index = 0;
            for(int i = 0; i < xArray.Length; i++)
            {
                double distValue = Math.Abs(line.A * xArray[i] + line.B * yArray[i] - line.C);
                if(minValue > distValue)
                {
                    minValue = distValue; index = i;
                }
            }
            return index;
        }
        #endregion

        public static bool leastSquareAdaptLineFast(List<double> xArray, List<double> yArray, out cylineParam lineValue)
        {
            Trace.Assert(xArray.Count == yArray.Count);
            cyPointBaseClass class1 = new cyPointBaseClass();
            lineValue = new cylineParam() { A = 0, B = 0, C = 0 };

            int nums = xArray.Count;
            List<double> matrix = new List<double>() { 0, 0, 0, 0 };
            List<double> bias = new List<double>() { 0, 0 };
            double xx = 0, xy = 0, yy = 0, x = 0, y = 0;
            for (int i = 0; i < xArray.Count; i++)
            {
                xx += xArray[i] * xArray[i];
                yy += yArray[i] * yArray[i];
                x += xArray[i];
                xy += xArray[i] * yArray[i];
                y += yArray[i];
            }
            double dVAlue1 = xx - (x * x) / nums;
            double dValue2 = yy - (y * y) / nums;
            bool bFunX = true;
            // y方向的方差更小， 那证明， 应该采用 x= a*y +b;
            if (dVAlue1 < dValue2)
                bFunX = false;

            if (bFunX == true)
            {
                //y = ax+b;
                matrix[0] = xx; matrix[1] = x;
                matrix[2] = matrix[1]; matrix[3] = nums;
                bias[0] = xy; bias[1] = y;
            }
            else
            {
                //ay +b = x
                matrix[0] = yy; matrix[1] = y;
                matrix[2] = matrix[1]; matrix[3] = nums;
                bias[0] = xy; bias[1] = x;
            }

            List<double> Invmatrix = null;
            MatrixInv2X2(matrix, out Invmatrix);

            // AAd*y = a*x +c
            List<double> reusltMatrix = new List<double>();
            Matrix_Mult2X1(Invmatrix, bias, out reusltMatrix);
            double a = reusltMatrix[0];
            double c = reusltMatrix[1];

            //三次去掉最大的偏差点
            List<double> listDist = new List<double>();
            List<double> moveIndex = new List<double>();
            List<double> moveXArray = new List<double>();
            List<double> moveYArray = new List<double>();


            for (int t = 0; t < 0; t++)
            {
                // 求平均距离， 和最大的偏置项，在置信度[-99.6, 99.6]内的值保留，其他的去掉
                double sValue = 0;
                double uValue = 0;
                for (int j = 0; j < xArray.Count; j++)
                {
                    double pValue = 0;
                    pValue = (bFunX == true) ? (xArray[j] * a + c - yArray[j]) / Math.Sqrt(a * a + 1) :
                                                (yArray[j] * a + c - xArray[j]) / Math.Sqrt(a * a + 1);
                    listDist.Add(pValue);
                    uValue += pValue;
                    sValue += pValue * pValue;
                }
                uValue /= xArray.Count;
                sValue = Math.Sqrt((sValue - (uValue * uValue) * xArray.Count) / (xArray.Count - 1));

                double distMin = uValue - 1.96 * sValue;
                double distMax = uValue + 1.96 * sValue;
                nums = xArray.Count;
                for (int j = 0; j < xArray.Count; j++)
                {
                    if (listDist[j] > distMin && listDist[j] < distMax)
                        continue;

                    matrix[0] -= ((bFunX == true) ? (xArray[j] * xArray[j]) : (yArray[j] * yArray[j]));
                    matrix[1] -= ((bFunX == true) ? xArray[j] : yArray[j]);
                    bias[0] -= xArray[j] * yArray[j];
                    bias[1] -= ((bFunX == true) ? yArray[j] : xArray[j]);

                    xArray.RemoveAt(j);
                    yArray.RemoveAt(j);
                    listDist.RemoveAt(j);

                    //double xValue = xArray[j];
                    //double yValue = yArray[j];
                    //moveXArray.Add(xValue);
                    //moveYArray.Add(yValue);
                    j--;
                }
                matrix[3] = xArray.Count;
                matrix[2] = matrix[1];
                MatrixInv2X2(matrix, out Invmatrix);
                Matrix_Mult2X1(Invmatrix, bias, out reusltMatrix);
                a = reusltMatrix[0];
                c = reusltMatrix[1];
                listDist.Clear();
                if (xArray.Count == nums)
                    break;
            }

            double div = Math.Sqrt(a * a + 1);
            if (bFunX == true)
            {
                // y=ax+c
                lineValue.A = a / div;
                lineValue.B = -1 / div;
                lineValue.C = -c / div;
            }
            else
            {
                //x = ay+c
                lineValue.A = 1 / div;
                lineValue.B = -a / div;
                lineValue.C = c / div;
            }


            return true;
        }

        /// <summary>
        /// 根据点坐标拟合一条直线
        /// </summary>
        public static bool leastSquareAdaptLine(List<double> xArray, List<double> yArray, out cylineParam lineValue)
        {
            Trace.Assert(xArray.Count == yArray.Count);
            cyPointBaseClass class1 = new cyPointBaseClass();
            lineValue = new cylineParam() { A = 0,B = 0,C = 0};

            int nums = xArray.Count;
            List<double> matrix = new List<double>() { 0, 0, 0, 0 };
            List<double> bias = new List<double>() { 0, 0 };
            double xx = 0, xy = 0, yy = 0, x = 0, y = 0;
            for (int i = 0; i<xArray.Count; i++)
            {
                xx += xArray[i] * xArray[i];
                yy += yArray[i] * yArray[i];
                x += xArray[i];
                xy += xArray[i] * yArray[i];
                y += yArray[i];
            }
            double dVAlue1 = xx - (x * x) / nums;
            double dValue2 = yy - (y * y) / nums;
            bool bFunX = true;
            // y方向的方差更小， 那证明， 应该采用 x= a*y +b;
            if (dVAlue1<dValue2)
                bFunX = false;

            if (bFunX == true)
            {
                //y = ax+b;
                matrix[0] = xx; matrix[1] = x;
                matrix[2] = matrix[1]; matrix[3] = nums;
                bias[0] = xy; bias[1] = y;
            }
            else
            {
                //ay +b = x
                matrix[0] = yy; matrix[1] = y;
                matrix[2] = matrix[1]; matrix[3] = nums;
                bias[0] = xy; bias[1] = x;
            }

            List<double> Invmatrix = null;
            MatrixInv2X2(matrix, out Invmatrix);

            // AAd*y = a*x +c
            List<double> reusltMatrix = new List<double>();
            Matrix_Mult2X1(Invmatrix, bias, out reusltMatrix);
            double a = reusltMatrix[0];
            double c = reusltMatrix[1];

            //三次去掉最大的偏差点
            List<double> listDist = new List<double>();
            List<double> moveIndex = new List<double>();
            List<double> moveXArray = new List<double>();
            List<double> moveYArray = new List<double>();


            int nums2 = xArray.Count();
            for (int t = 0; t< 3; t++)
            {
                // 求平均距离， 和最大的偏置项，在置信度[-99.6, 99.6]内的值保留，其他的去掉
                double sValue = 0;
                double uValue = 0;
                if (xArray.Count() < nums2*2 / 3 || xArray.Count() <= 2)
                    break;
                for (int j = 0; j<xArray.Count; j++)
                {
                    double pValue = 0;
                    pValue = (bFunX == true) ? (xArray[j] * a + c - yArray[j]) / Math.Sqrt(a* a + 1) :
                                                (yArray[j] * a + c - xArray[j]) / Math.Sqrt(a* a + 1);
                    listDist.Add(pValue);
                    uValue += pValue;
                    sValue += pValue* pValue;
                }
                uValue /= xArray.Count;
                sValue = Math.Sqrt((sValue - (uValue* uValue) * xArray.Count) / (xArray.Count - 1));

                double distMin = uValue - 1.96 * sValue;
                double distMax = uValue + 1.96 * sValue;
                nums = xArray.Count;
                for (int j = 0; j<xArray.Count; j++)
                {
                    if (listDist[j] > distMin && listDist[j] < distMax)
                        continue;

                    matrix[0] -= ((bFunX == true)? (xArray[j] * xArray[j]):(yArray[j]* yArray[j]));
                    matrix[1] -= ((bFunX == true) ? xArray[j]:yArray[j]);
                    bias[0] -= xArray[j] * yArray[j];
                    bias[1] -= ((bFunX == true) ? yArray[j]:xArray[j]);

                    xArray.RemoveAt(j);
                    yArray.RemoveAt(j);
                    listDist.RemoveAt(j);

                    //double xValue = xArray[j];
                    //double yValue = yArray[j];
                    //moveXArray.Add(xValue);
                    //moveYArray.Add(yValue);
                    j--;
                }
                matrix[3] = xArray.Count;
                matrix[2] = matrix[1];
                MatrixInv2X2(matrix, out Invmatrix);
                Matrix_Mult2X1(Invmatrix, bias, out reusltMatrix);
                a = reusltMatrix[0];
                c = reusltMatrix[1];
                listDist.Clear();
                if (xArray.Count == nums)
                    break;
            }

            double div = Math.Sqrt(a * a + 1);
            if (bFunX ==true)
            {
                // y=ax+c
                lineValue.A = a / div;
                lineValue.B = -1 / div;
                lineValue.C = -c / div;
            }
            else
            {
                //x = ay+c
                lineValue.A = 1/div;
                lineValue.B = -a / div;
                lineValue.C = c / div;
            }


            return true;
        }

        /// <summary>
        /// 采用另外一种方式进行数学计算， 最小二乘法做一定的约束条件，约束方程为 J = L2*(A*aParam + B*bParam)^2)
        /// 这种方式可以小范围的调整角度
        /// </summary>
        /// <param name="xArray"></param>
        /// <param name="yArray"></param>
        /// <param name="aParam"></param>
        /// <param name="bParam"></param>
        /// <param name="L2"></param>
        /// <returns></returns>
        public static bool leastSquareAdaptLine2(double[] xArray, double[] yArray, double aParam, double bParam, out cylineParam lineValue,  double L2 = 100)
        {
            if (L2 <= 0)
                return leastSquareAdaptLine(new List<double>(xArray), new List<double>(yArray), out lineValue);

            lineValue = new cylineParam();
            Trace.Assert(xArray.Length == yArray.Length);
            if (xArray.Length < 1)
                return false;

            Trace.Assert(xArray.Length == yArray.Length);
            cyPointBaseClass class1 = new cyPointBaseClass();

            int nums = xArray.Length;
            double xx = 0, yy = 0;
            double xy = 0;
            double x = 0, y = 0;

            for (int i = 0; i < xArray.Length; i++)
            {
                xx += xArray[i] * xArray[i];
                yy += yArray[i] * yArray[i];
                xy += xArray[i] * yArray[i];
                y += yArray[i];
                x += xArray[i];
            }

            
            /*     [ xx + n*L2*bParam, x ]       [xy - n*L2*aParam]      [n,                  -x]
             * T = [                     ] , B = [                ], T* =[                      ]
             *     [                x, n ]       [     y          ]      [-x    xx + n*L2*bParam] 
             */
            double t11 = nums, t12 = -x;
            double t21 = t12, t22 = (xx + nums*L2*bParam);
            double b1 = xy - nums * L2 * aParam;
            double b2 = y;
            double div = nums * (xx + nums * L2 * bParam) - x * x;

            double a = t11 * b1 + t12 * b2;
            double b = t21 * b1 + t22 * b2;

            if (Math.Abs(a) <= 1e-6 && Math.Abs(-div) <= 1e-6)
                return true;

            double dValue = Math.Sqrt(a * a + div * div);
            lineValue.A = a / dValue;
            lineValue.B = -div / dValue;
            lineValue.C = -b / dValue;

            return true;
        }


        public static bool leastSquareAdaptLineWeight(List<double> xArray, List<double> yArray, List<double> weight,out cylineParam lineValue)
        {
            Trace.Assert(xArray.Count == yArray.Count);
            cyPointBaseClass class1 = new cyPointBaseClass();
            lineValue = new cylineParam() { A = 0, B = 0, C = 0 };

            int nums = xArray.Count;
            List<double> matrix = new List<double>() { 0, 0, 0, 0 };
            List<double> bias = new List<double>() { 0, 0 };
            double xx = 0, xy = 0, yy = 0, x = 0, y = 0;
            for (int i = 0; i < xArray.Count; i++)
            {
                xx += xArray[i] * xArray[i]*weight[i];
                yy += yArray[i] * yArray[i]*weight[i];
                xy += xArray[i] * yArray[i]*weight[i];
                x += xArray[i] * weight[i];
                y += yArray[i] * weight[i];
            }

            matrix[0] = xx; matrix[1] = xy;
            matrix[2] = matrix[1]; matrix[3] = yy;
            bias[0] = x; bias[1] = y;
            List<double> result;
            List<double> invMatrix;
            cyUCSMeasure.MatrixInv2X2(matrix, out invMatrix);
            cyUCSMeasure.Matrix_Mult2X1(invMatrix, bias, out result);
            lineValue.A = result[0]; lineValue.B = result[1];
            //double c = (result[0]*x + result[1]*y) /weight.Sum() ;
            lineValue = new cylineParam() { A = result[0], B = result[1], C = 1 };
            lineValue.OnNormalize();
            return true;
        }

    }
}
