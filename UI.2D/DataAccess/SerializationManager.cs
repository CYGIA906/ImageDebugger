using System;
using ImageDebugger.Core.IoC.Interface;

namespace UI._2D.DataAccess
{
    public class SerializationManager : ISerializationManager
    {
        public string SerializationBaseDir => Environment.CurrentDirectory;
    }
}