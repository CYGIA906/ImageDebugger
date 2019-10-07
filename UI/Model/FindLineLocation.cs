using System.Collections.Generic;
using System.Xml;

namespace UI.Model
{
    public class FindLineLocation
    {
        public string Name { get; set; }
        
        public double X { get; set; }

        public double Y { get; set; }

        public double Angle { get; set; }

        public double Len2 { get; set; }
    }

    public interface IFindLineLocations
    {
        List<FindLineLocation> Locations { get; set; }
    }

    public class TopViewFindLineLocations : IFindLineLocations
    {
        public List<FindLineLocation> Locations { get; set; }

        public TopViewFindLineLocations()
        {
            Locations = new List<FindLineLocation>()
            {
                new FindLineLocation
                {
                    Name = "2-left",
                    X = 690.4,
                    Y = 2015,
                    Angle = 90,
                    Len2 = 222
                }, new FindLineLocation()
                {
                    Name = "2-right",
                    X= 1699,
                    Y = 2016,
                    Angle = 90,
                    Len2 = 223
                }            };
        }
    }
    
}