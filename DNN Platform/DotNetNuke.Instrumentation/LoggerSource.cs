// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation;

/// <summary>Provides access to an <see cref="ILoggerSource"/> instance.</summary>
public static class LoggerSource
{
    /// <summary>Gets the instance.</summary>
    public static ILoggerSource Instance { get; private set; } = new LoggerSourceImpl();

    /// <summary>Overrides the <see cref="Instance"/> for testing.</summary>
    /// <param name="loggerSource">A test <see cref="ILoggerSource"/> instance.</param>
    public static void SetTestableInstance(ILoggerSource loggerSource)
    {
        Instance = loggerSource;
    }
}
