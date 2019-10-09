using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UI.ViewModels;

namespace UI.ImageProcessing.Utilts
{
    public class FaiItemCsvSerializer
    {
        public List<string> Header { get; set; }
        public string OutputDir { get; set; }

        public FaiItemCsvSerializer(string outputDir)
        {
            OutputDir = outputDir;
        }

        public void Serialize(IEnumerable<FaiItem> items)
        {
            var itemsSorted = items.OrderBy(item => item.Name);
            if (Header == null) InitHeader(itemsSorted.Select(item => item.Name));

            var line = itemsSorted.Select(item => item.ValueBiased.ToString("f4")).ToList();
            line.Insert(0, DateTime.Now.ToString("HH:mm:ss:ff") );
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