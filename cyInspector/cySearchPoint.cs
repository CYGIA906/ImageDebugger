using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace cyInspector
{    

    /// <summary>
    /// 针对单一一条线上的点
    /// </summary>
    public class cySearchPoint
    {

        private double[] _gauss { get; set; } = null;

        private int OnSearchFirstPoint(List<double> pointValue, double LimitValue)
        {
            for (int i = 0; i < pointValue.Count(); i++)
                if (pointValue[i] >= LimitValue)
                    return i;

            return -1;
        }

        private int OnSearchLastPoint(List<double> pointValue, double LimitValue)
        {
            for (int i = pointValue.Count() -1; i >= 0 ; i--)
                if (pointValue[i] >= LimitValue)
                    return i;

            return -1;
        }
        
        //当有中间值时找中间的index
        private int OnSearchMidPoint(List<double> pointValue, double LimitValue)
        {
            double maxValue = 0;double secondValue = 0;
            int maxIndex = -1; int secondIndex = -1;
            for (int i = pointValue.Count() - 1; i >= 0; i--)
            {
                if (pointValue[i] >= LimitValue)
                {
                    if (pointValue[i] > maxValue)
                    {
                        secondValue = maxValue; secondIndex = maxIndex;
                        maxValue = pointValue[i]; maxIndex = i;
                    }
                    else if (pointValue[i] > secondValue)
                    {
                        secondValue = pointValue[i]; secondIndex = i;
                    }
                }
            }
            if(secondIndex >= 0 && maxIndex >= 0)
            {
                return (int)(secondIndex + maxIndex) / 2;
            }
            return maxIndex;
        }


        /// <summary>
        /// 当相关的值最大的时候，表示为最优点
        /// </summary>
        /// <param name="pointValue"></param>
        /// <param name="LimitValue"></param>
        /// <returns></returns>
        private int OnSearchBestPoint(List<double> pointValue, double LimitValue)
        {
            int maxIndex = 0;
            double maxValue = pointValue[0];
            for(int i = 1; i < pointValue.Count(); i++)
            {
                if(maxValue < pointValue[i])
                {
                    maxValue = pointValue[i];
                    maxIndex = i;
                }
            }
            if (maxValue > LimitValue)
                return maxIndex;
            else
                return -1;
        }

        private int OnSearchAllPoint(List<double>pointValue, double LimitValue, out List<int> searchIndex)
        {
            searchIndex = new List<int>();
            int startIndex = 0;
            int lenth = 0;
            bool bBreak = false;
            for (int i = 0; i < pointValue.Count(); i++)
            {
                if (LimitValue < pointValue[i])
                {
                    if(bBreak == true)
                    {
                        lenth++;
                    }
                    else
                    {
                        startIndex = i;
                        lenth = 0;
                        bBreak = true;
                    }
                }
                else
                {
                    if(bBreak == true)
                    {
                        searchIndex.Add((int)(startIndex + lenth / 2));
                    }
                    bBreak = false;
                }
            }
            return searchIndex.Count();
        }

        /// <summary>
        /// 搜索绝对的最大的梯度下降区域
        /// </summary>
        /// <param name="pointValue"></param>
        /// <param name="searchIndex"></param>
        private void OnSearchAbsGrandient(List<double> pointValue, List<double> xlocal, List<double> ylocal,double LimitValue, cyEnumPointSel pointSel,int lenth1,out List<double> findX, out List<double> findY)
        {
            findX = new List<double>();
            findY = new List<double>();
            // 用于计算拥有多少个数据
            int rowCount = pointValue.Count() / lenth1;

            int startIndex = 0;
            for (int i = 0; i < rowCount; i++)
            {
                List<double> tmpLocal = pointValue.GetRange(startIndex, lenth1);
                switch (pointSel)
                {
                    case cyEnumPointSel.First:
                    {
                            int tmpIndex = OnSearchFirstPoint(tmpLocal, LimitValue);
                            if (tmpIndex >= 0)
                            {
                                tmpIndex += startIndex;
                                findX.Add(xlocal[tmpIndex]);
                                findY.Add(ylocal[tmpIndex]);
                            }
                            break;
                        }
                    case cyEnumPointSel.Last:
                        {
                            int tmpIndex = OnSearchLastPoint(tmpLocal, LimitValue);
                            if (tmpIndex >= 0)
                            {
                                tmpIndex += startIndex;
                                findX.Add(xlocal[tmpIndex]);
                                findY.Add(ylocal[tmpIndex]);
                            }
                            break;
                        }
                    case cyEnumPointSel.Best:
                        {
                            int tmpIndex = OnSearchBestPoint(tmpLocal, LimitValue);
                            if (tmpIndex >= 0)
                            {
                                tmpIndex += startIndex;
                                findX.Add(xlocal[tmpIndex]);
                                findY.Add(ylocal[tmpIndex]);
                            }
                            break;
                        }
                    case cyEnumPointSel.Mid:
                        {
                            int tmpIndex = OnSearchMidPoint(tmpLocal, LimitValue);
                            if (tmpIndex >= 0)
                            {
                                tmpIndex += startIndex;
                                findX.Add(xlocal[tmpIndex]);
                                findY.Add(ylocal[tmpIndex]);
                            }
                            break;
                         }
                    case cyEnumPointSel.All:
                        {
                            List<int> searchIndex;
                            int tmpIndex = OnSearchAllPoint(tmpLocal, LimitValue, out searchIndex);
                            if (tmpIndex > 0)
                            {
                                for (int t = 0; t < tmpIndex; t++)
                                {
                                    int index = searchIndex[t] +  startIndex;
                                    findX.Add(xlocal[index]);
                                    findY.Add(ylocal[index]);
                                }

                            }
                            break;
                        }
                    default:
                        break;
                }
                startIndex += lenth1;
            }
        }

        public void OnGetGaussKernel(int winSize, float sigma)
        {
            int wincenter, x;
            double sum = 0.0f;
            //计算中心点大小
            wincenter = winSize / 2;
            //kern用来存储高斯模糊前的数据
            //ikern用来存储高斯模糊后与像素值256的乘积值
            _gauss = new double[winSize];
            //计算高斯分布公式的系数
            float SQRT_2PI = 2.506628274631f;
            float sigmaMul2PI = 1.0f / (sigma * SQRT_2PI);
            float divSigmaPow2 = 1.0f / (2.0f * sigma * sigma);

            for (x = 0; x < wincenter + 1; x++)
            {
                _gauss[wincenter - x] = _gauss[wincenter + x] = Math.Exp(-(x * x) * divSigmaPow2) * sigmaMul2PI;
                sum += _gauss[wincenter - x] + ((x != 0) ? _gauss[wincenter + x] : 0.0);
            }
            sum = 1.0f / sum;

            for (x = 0; x < winSize; x++)
            {
                _gauss[x] *= sum;
            }
        }

        private void ConvolGaussKernel(ref List<double> pointValue, double[] gauss)
        {
            Trace.Assert(pointValue.Count > gauss.Length);
            int pointLenth = pointValue.Count;

            List<double> algPointValue = new List<double>();
            for (int w = 0; w < pointValue.Count; w++)
            {
                //当前位置,采用镜像的方式
                double sumValue = 0;
                int halfSize = gauss.Length / 2;
                for (int tIndex = 0; tIndex < gauss.Length; tIndex++)
                {
                    int tmpIndex = (w + tIndex - halfSize);
                    if (tmpIndex < 0)
                        sumValue += gauss[tIndex] * pointValue[-tmpIndex];
                    else if (tmpIndex >= pointValue.Count)
                        sumValue += gauss[tIndex] * pointValue[pointLenth-1 + pointLenth - tmpIndex];
                    else
                        sumValue += gauss[tIndex] * pointValue[tmpIndex];
                }
                algPointValue.Add(sumValue);
            }
            //计算结果
            pointValue = algPointValue;
        }

        private void OnAbsGrandit(ref List<double> pointValue)
        {
            for (int i = pointValue.Count-1; i >= 1; i--)
                pointValue[i] = Math.Abs(pointValue[i] - pointValue[i - 1]);
            pointValue[0] = 0;
        }

        private void OnValueTranslateSingle(ref List<double> pointValue, cyEnumPointSmooth method)
        {
            switch (method)
            {
                case cyEnumPointSmooth.GaussSmooth:
                    {
                        ConvolGaussKernel(ref pointValue, _gauss);
                        OnAbsGrandit(ref pointValue);
                        break;
                    }
                case cyEnumPointSmooth.AbsGradiant:
                    {
                        OnAbsGrandit(ref pointValue);
                        break;
                    }
                default:
                    {
                        OnAbsGrandit(ref pointValue);
                        break;
                    }
            }

        }

        public void OnSearchOneAxisPoint(ref List<double> pointValue, double threValue,cyEnumPointSmooth smooth, cyEnumPointSel method, out List<int> searchIndex)
        {
            Trace.Assert(_gauss != null);
            searchIndex = new List<int>();
            if (pointValue.Count == 0)
                return;
            OnValueTranslateSingle(ref pointValue, smooth);

            switch (method)
            {
                case cyEnumPointSel.First:
                    {
                        int tIndex = OnSearchFirstPoint(pointValue, threValue);
                        searchIndex.Add(tIndex);
                        break;
                    }
                case cyEnumPointSel.Last:
                    {
                        int tIndex = OnSearchLastPoint(pointValue, threValue);
                        searchIndex.Add(tIndex);
                        break;
                    }
                case cyEnumPointSel.Best:
                    {
                        int tIndex = OnSearchBestPoint(pointValue, threValue);
                        searchIndex.Add(tIndex);
                        break;
                    }
                case cyEnumPointSel.All:
                    {
                        int tIndex = OnSearchAllPoint(pointValue, threValue, out searchIndex);
                        break;
                    }
                default:
                    {
                        int tIndex = OnSearchBestPoint(pointValue, threValue);
                        searchIndex.Add(tIndex);
                        break;
                    }
            }
        }
    }
}
