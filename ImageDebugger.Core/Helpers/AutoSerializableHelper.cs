
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using ImageDebugger.Core.Models;

namespace ImageDebugger.Core.Helpers
{
    public static class AutoSerializableHelper
    {
        /// <summary>
        /// Load auto-serializables from disk
        /// If anyone not exist, a new one will be created
        /// </summary>
        /// <param name="objectNames">Names of the <see cref="AutoSerializableBase&lt;T&gt;"/></param>
        /// <param name="serializationDir">The directory that the auto-serializables exists</param>
        /// <typeparam name="T">Type of the auto-serializaables to load</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> LoadAutoSerializables<T>(IEnumerable<string> objectNames, string serializationDir) where T: AutoSerializableBase<T>, new()
        {
            Directory.CreateDirectory(serializationDir);


            var outputs = new List<T>();
            
            foreach (var name in objectNames)
            {
                var filePath = Path.Combine(serializationDir, name+".xml");
                try // Load it from disk
                {
                    using (var fs = new FileStream(filePath, FileMode.Open))
                    {
                        var serializer = new XmlSerializer(typeof(T));
                        T item = (T) serializer.Deserialize(fs);
                        item.SerializationDirectory = serializationDir;
                        outputs.Add(item);
                    }
                }
                catch (FileNotFoundException e) // If not in disk, create a new one
                {
                    outputs.Add(new T()
                    {
                        Name = name,
                        SerializationDirectory = serializationDir
                    });
                }
            }

            foreach (var item in outputs)
            {
                item.ShouldAutoSerialize = true;
            }

            return outputs;
        }
    }
} 