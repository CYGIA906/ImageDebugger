﻿using System.Collections.Generic;
using UI.ImageProcessing;

namespace UI.Model
{
    public class ImageProcessingResult
    {
        public Dictionary<string, double> FaiDictionary { get; set; }

        public HalconGraphics HalconGraphics { get; set; }

        public DataRecorder DataRecorder { get; set; }
    }
}