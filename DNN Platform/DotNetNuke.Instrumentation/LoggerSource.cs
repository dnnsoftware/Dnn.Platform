// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Instrumentation
{
    public static class LoggerSource 
    {
        static ILoggerSource _instance = new LoggerSourceImpl();

        public static ILoggerSource Instance
        {
            get { return _instance; }
        }

        public static void SetTestableInstance(ILoggerSource loggerSource)
        {
            _instance = loggerSource;
        }
    }
}
