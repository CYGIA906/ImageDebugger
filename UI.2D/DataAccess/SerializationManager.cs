using System;
using System.Windows;
using ImageDebugger.Core.IoC;
using ImageDebugger.Core.IoC.Interface;

namespace UI
{
    public class SerializationManager : ISerializationManager
    {
        public string SerializationBaseDir => Environment.CurrentDirectory;
    }
}