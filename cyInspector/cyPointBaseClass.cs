using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyInspector
{

    public struct cyPoint2f
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    public struct cyPoint2d
    {
        public double x { get; set; }
        public double y { get; set; }


        public static cyPoint2d operator -(cyPoint2d a, cyPoint2d b)
        {
            return new cyPoint2d() { x = a.x - b.x, y = a.y - b.y };
        }

        public static cyPoint2d operator +(cyPoint2d a, cyPoint2d b)
        {
            return new cyPoint2d() { x = a.x + b.x, y = a.y + b.y };
        }

        public static cyPoint2d operator *(cyPoint2d a, double b)
        {
            return new cyPoint2d() { x = a.x*b, y = a.y*b };
        }

        public List<double> ToList()
        {
            return new List<double>() { x, y };
        }

        public double[] ToArray()
        { return new double[] { x, y }; }
    }

    public struct cyPoint2
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    class cyPointBaseClass
    {
        public cyPointBaseClass()
        {
            ;
        }
    }
}
