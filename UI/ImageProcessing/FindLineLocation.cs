using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace UI.ImageProcessing
{
    public class FindLineLocation
    {
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public double X { get; set; }
        [XmlAttribute] public double Y { get; set; }
        [XmlAttribute] public double Angle { get; set; }
        [XmlAttribute] public double Len1 { get; set; } = 10;

        [XmlAttribute] public double Len2 { get; set; }

        /// <summary>
        /// Which image to find line
        /// </summary>
        [XmlAttribute] public int ImageIndex { get; set; }

        public string IsVertical => (180 - Math.Abs(Angle) < 10 || Math.Abs(Angle) < 10) ? "true" : "false";


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
                    },
                    new FindLineLocation()
                    {
                        Name = "2-right",
                        X = 1699,
                        Y = 2016,
                        Angle = 90,
                        Len2 = 223
                    },
                    new FindLineLocation()
                    {
                        Name = "17",
                        X = 2312,
                        Y = 1055,
                        Len2 = 294
                    }
                };
            }
        }
    }
}