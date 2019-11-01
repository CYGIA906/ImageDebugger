using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageDebugger.Core.Models;
using ImageDebugger.Core.ViewModels.LineScan;

namespace ImageDebugger.Core.ImageProcessing.Utilts
{
    public class CsvSerializer
    {
        public List<string> Header { get; set; }
        public string OutputDir { get; set; }

        public CsvSerializer(string outputDir)
        {
            OutputDir = outputDir;
        }

        public void Serialize(IEnumerable<ICsvColumnElement> items, string imageName, bool shouldSerialize = true)
        {
            if (!shouldSerialize) return;
            var itemsSorted = items.OrderBy(item => item.CsvName);
            if (Header == null) InitHeader(itemsSorted.Select(item => item.CsvName));

            var line = itemsSorted.Select(item => item.Value.ToString("f4")).ToList();
            line.Insert(0, imageName);
            var csvLine = string.Join(",", line);

            var fileExists = File.Exists(CsvPath);
            Directory.CreateDirectory(OutputDir);
            var lineToWrite = fileExists ? csvLine : HeaderLine + Environment.NewLine + csvLine;
            using (var fs = new StreamWriter(CsvPath, fileExists))
            {
                fs.WriteLine(lineToWrite);
            }
        }

        private void InitHeader(IEnumerable<string> names)
        {
            Header = new List<string>(){"Time"};
            foreach (var name in names)
            {
                Header.Add(name);
            }
        }

        public string HeaderLine
        {
            get { return string.Join(",", Header); }
        }

        public string CsvPath
        {
            get { return Path.Combine(OutputDir, DateTime.Now.ToString("MMdd")) + ".csv"; }
        }
    }
}