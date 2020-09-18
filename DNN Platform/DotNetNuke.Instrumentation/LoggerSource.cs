// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation
{
    public static class LoggerSource
    {
        private static ILoggerSource _instance = new LoggerSourceImpl();

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
