// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Integration.Services.Installer
{
    using System;
    using DotNetNuke.Instrumentation;

    internal class TestLogSource : ILoggerSource
    {
        public ILog GetLogger(string name)
        {
            return new TestLogger();
        }

        public ILog GetLogger(Type type)
        {
            return new TestLogger();
        }
    }
}
