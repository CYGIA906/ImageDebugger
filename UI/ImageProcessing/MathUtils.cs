using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ImageProcessing
{
    public static class MathUtils
    {
        public static double ToRadian(double degree)
        {
            return degree / 180.0 * Math.PI;
        }
    }
}
