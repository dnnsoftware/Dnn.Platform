using System;

namespace DotNetNuke.Instrumentation
{
    public interface ILoggerSource
    {
        ILog GetLogger(Type type);
        ILog GetLogger(string name);
    }
}