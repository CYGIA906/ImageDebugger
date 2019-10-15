using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UI.Model;
using UI.ViewModels;

namespace UI.ImageProcessing
{
    public partial class I94BottomViewMeasurement
    {
        public ObservableCollection<FaiItem> GenFaiItemValues(string faiItemSerializationDir)
        {
            throw new NotImplementedException();
        }

        public List<FindLineLocation> GenFindLineLocationValues()
        {
            var outputs = new List<FindLineLocation>()
            {
                new FindLineLocation()
                {
                    //p
                    Name = "21left-top", X = 72, Y = 2679, Angle = 0, Len2 = 148, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    //pName = "21left-bottom",
                    X = 80, Y = 3866, Angle = 0, Len2 = 130, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    //p
                    Name = "21right-top", X = 2322, Y = 2671, Angle = 180, Len2 = 145, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    //p
                    Name = "21right-bottom", X = 2323, Y = 3817, Angle = 180, Len2 = 145, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    //p
                    Name = "23top", X = 1208, Y = 2069, Angle = 90, Len2 = 610, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    // n
                    Name = "23bottom-left", X = 660, Y = 4450, Angle = 90, Len2 = 114, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    // n
                    Name = "23bottom-center", X = 1189, Y = 4448, Angle = 90, Len2 = 215, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    // n
                    Name = "23bottom-right", X = 1750, Y = 4448, Angle = 90, Len2 = 114, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    // p
                    Name = "24.left", X = 414, Y = 1016, Angle = 0, Len2 = 145, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    // p
                    Name = "24.right", X = 1973, Y = 1016, Angle = 180, Len2 = 145, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    // p
                    Name = "27.top", X = 3140, Y = 1590, Angle = 90, Len2 = 150, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    //p
                    Name = "27.bottom", X = 3140, Y = 2679, Angle = -90, Len2 = 150, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    // p
                    Name = "29.left", X = 2602, Y = 2134, Angle = 0, Len2 = 150, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    //p
                    Name = "29.right", X = 3684, Y = 2134, Angle = 180, Len2 = 150, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    // p
                    Name = "31.topLeft", X = 283, Y = 2278, Angle = 45, Len2 = 260, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    //p
                    Name = "31.bottomLeft", X = 282, Y = 4238, Angle = -45, Len2 = 260, ImageIndex = 0
                },
                new FindLineLocation()
                {
                    Name = "31.topRight", X = 2109, Y = 2275, Angle = 135, Len2 = 260, ImageIndex = 0
                }
            };
            return outputs;
        }

        public ObservableCollection<FindLineParam> GenFindLineParamValues(string paramSerializationBaseDir)
        {
            throw new NotImplementedException();
        }
    }
}