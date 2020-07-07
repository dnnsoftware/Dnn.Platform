// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation
{
    using System;

    public interface ILoggerSource
    {
        ILog GetLogger(Type type);

        ILog GetLogger(string name);
    }
}
