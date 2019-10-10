using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using Application = System.Windows.Forms.Application;

namespace UI.ImageProcessing
{
    public class FindLineConfigs
    {
        private Dictionary<string, FindLineParam> _findLineParamsDict = new Dictionary<string, FindLineParam>();
        private Dictionary<string, FindLineLocation> _findLineLocationsAbsDict = new Dictionary<string, FindLineLocation>();

        public List<FindLineLocation> FindLineLocationsRelative { get;  set; }
        private CoordinateSolver _solver;



        /// <summary>
        /// Convert relative locations to absolute locations
        /// </summary>
        public void GenerateLocationsAbs(CoordinateSolver solver)
        {
            _findLineLocationsAbsDict.Clear();
            foreach (var location in FindLineLocationsRelative)
            {
                var locationAbs = solver.FindLineLocationRelativeToAbsolute(location);
                _findLineLocationsAbsDict[location.Name] = locationAbs;
            }
        }

        public string SerializeDir { get; set; }

        public string ParamsSerializeName { get; set; }

        public string LocationsSerializeName { get; set; }

        public string ParamsPath
        {
            get { return Path.Combine(SerializeDir, ParamsSerializeName + ".xml"); }
        }

        public string LocationsPath
        {
            get { return Path.Combine(SerializeDir, LocationsSerializeName + ".xml"); }
        }


        /// <summary>
        /// Construct from code generated data
        /// </summary>
        /// <param name="findLineParams"></param>
        /// <param name="findLineLocations"></param>
        public FindLineConfigs(List<FindLineParam> findLineParams, List<FindLineLocation> findLineLocations)
        {
            foreach (var param in findLineParams)
            {
                _findLineParamsDict[param.Name] = param;
            }

            FindLineLocationsRelative = findLineLocations;
        }

        /// <summary>
        /// Construct from data from disk
        /// </summary>
        /// <param name="serializeDir"></param>
        /// <param name="paramsSerializeName"></param>
        /// <param name="locationsSerializeName"></param>
        public FindLineConfigs(string serializeDir, string paramsSerializeName, string locationsSerializeName = null)
        {
            SerializeDir = serializeDir;
            ParamsSerializeName = paramsSerializeName;
            LocationsSerializeName = locationsSerializeName;

            LoadFindLineParamsFromDisk();
            if(!string.IsNullOrEmpty(LocationsSerializeName)) LoadLocationsFromDisk();
        }


        /// <summary>
        /// Group find line location and params into something like:
        /// "2-left" + "2-right" = "2"
        /// </summary>
        public Dictionary<string, FindLineFeeding> GenerateFindLineFeedings()
        {
            Dictionary<string, FindLineFeeding> outputs = new Dictionary<string, FindLineFeeding>();
            foreach (var findLineName in _findLineLocationsAbsDict.Keys)
            {
                TryAddFindLineFeedings(findLineName, outputs);
            }

            return outputs;
        }

      

            /// <summary>
            /// If key not exist, add one find line feeding
            /// </summary>
            /// <param name="name"></param>
            /// <param name="findLineFeedings"></param>
            private void TryAddFindLineFeedings(string name, Dictionary<string, FindLineFeeding> findLineFeedings)
        {
            // No "-" means one find line rect for one line result
            if (!name.Contains("-"))
            {
                var param = _findLineParamsDict[name];
                var location = _findLineLocationsAbsDict[name];

                var feeding = new FindLineFeeding()
                {
                    Row = location.Y,
                    Col = location.X,
                    Radian = MathUtils.ToRadian(location.Angle),
                    Len1 = location.Len1,
                    Len2 = location.Len2,
                    Transition = param.Polarity == FindLinePolarity.Positive ? "positive" : "negative",
                    NumSubRects = param.NumSubRects,
                    IgnoreFraction = param.IgnoreFraction,
                    Threshold = param.Threshold,
                    Sigma1 = param.Sigma1,
                    Sigma2 = param.Sigma2,
                    WhichEdge = param.WhichEdge == EdgeSelection.First ? "first" : "last",
                    WhichPair = param.WhichPair == PairSelection.First ? "first" : "last",
                    NewWidth = param.NewWidth,
                    MinWidth = param.MinWidth,
                    MaxWidth = param.MaxWidth,
                    FirstAttemptOnly = param.FirstAttemptOnly,
                    UsingPair = param.UsingPair,
                    ImageIndex = location.ImageIndex,
                    IsVertical = location.IsVertical,
                    CannyHigh = param.CannyHigh,
                    CannyLow = param.CannyLow
                };
                findLineFeedings[name] = feeding;
            }
            else
            {

                var key = name.Substring(0, name.IndexOf("-"));
                // If the key has already added... for example 2 for 2-left and 2-right
                if(findLineFeedings.ContainsKey(key)) return;

                var locations = _findLineLocationsAbsDict.Where(pair => pair.Key.Contains(key)).Select(pair => pair.Value).ToList();
                var parameters = _findLineParamsDict.Where(pair => pair.Key.Contains(key)).Select(pair => pair.Value).ToList();

                if(locations.Count != parameters.Count) throw new InvalidOperationException($"Location count {locations.Count} != parameter count {parameters.Count}");

                var feeding = new FindLineFeeding()
                {
                    Row = locations.Select(l=> l.Y).ToArray(),
                    Col = locations.Select(l => l.X).ToArray(),
                    Radian = locations.Select(l => MathUtils.ToRadian(l.Angle)).ToArray(),
                    Len1 = locations.Select(l=>l.Len1).ToArray(),
                    Len2 = locations.Select(l=>l.Len2).ToArray(),
                    Transition = parameters[0].Polarity == FindLinePolarity.Positive ? "positive" : "negative",
                    NumSubRects = parameters[0].NumSubRects,
                    IgnoreFraction = parameters[0].IgnoreFraction,
                    Threshold = parameters.Select(p=>p.Threshold).ToArray(),
                    Sigma1 = parameters[0].Sigma1,
                    Sigma2 = parameters[0].Sigma2,
                    WhichEdge = parameters[0].WhichEdge == EdgeSelection.First ? "first" : "last",
                    WhichPair = parameters[0].WhichPair == PairSelection.First ? "first" : "last",
                    NewWidth = parameters[0].NewWidth,
                    MinWidth = parameters[0].MinWidth,
                    MaxWidth = parameters[0].MaxWidth,
                    FirstAttemptOnly = parameters[0].FirstAttemptOnly,
                    UsingPair = parameters[0].UsingPair,
                    ImageIndex = locations[0].ImageIndex,
                    IsVertical = locations[0].IsVertical,
                    CannyHigh = parameters[0].CannyHigh,
                    CannyLow = parameters[0].CannyLow,
                };
                findLineFeedings[key] = feeding;
            }
        }


        private void LoadLocationsFromDisk()
        {
            using (var fs = new FileStream(LocationsPath, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(FindLineLocation[]),
                    new XmlRootAttribute(LocationsSerializeName));
                FindLineLocationsRelative = ((FindLineLocation[]) serializer.Deserialize(fs)).ToList();
                
            }
        }

        private void LoadFindLineParamsFromDisk()
        {
            using (var fs = new FileStream(ParamsPath, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(FindLineParam[]), new XmlRootAttribute(ParamsSerializeName));
                var paramsLoaded = (FindLineParam[]) serializer.Deserialize(fs);
                _findLineParamsDict = paramsLoaded.ToDictionary(p => p.Name, p => p);
            }
        }

        public void Serialize()
        {
    

            // Serialize params
            using (var fs = new FileStream(ParamsPath, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(FindLineParam[]), new XmlRootAttribute(ParamsSerializeName));
                serializer.Serialize(fs, _findLineParamsDict.Values.ToArray());
            }

            if (string.IsNullOrEmpty(LocationsSerializeName)) return;
            // Serialize locations
            using (var fs = new FileStream(LocationsPath, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(FindLineLocation[]),
                    new XmlRootAttribute(LocationsSerializeName));
                serializer.Serialize(fs, FindLineLocationsRelative.ToArray());
            }
        }
    }
}