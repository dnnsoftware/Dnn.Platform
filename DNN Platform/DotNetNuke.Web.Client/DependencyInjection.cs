// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client;

using System;
using System.Reflection;

using DotNetNuke.Instrumentation;

using Microsoft.Extensions.DependencyInjection;

internal static class DependencyInjection
{
    private static readonly Type CommonGlobalsType;

    static DependencyInjection()
    {
        try
        {
            // all these types are part of the same library, so we don't need a separate catch for each one
            CommonGlobalsType = Type.GetType("DotNetNuke.Common.Globals, DotNetNuke");
        }
        catch (Exception exception)
        {
            LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to get get types for reflection", exception);
        }
    }

    public static IServiceScope GetOrCreateServiceScope()
    {
        var getOrCreateServiceScopeMethod = CommonGlobalsType.GetMethod("GetOrCreateServiceScope", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException("Unable to retrieve Globals.GetOrCreateServiceScope method via reflection");
        var serviceScope = getOrCreateServiceScopeMethod.Invoke(null, []);
        return (IServiceScope)serviceScope;
    }

    public static IServiceProvider GetCurrentServiceProvider()
    {
        var getCurrentServiceProviderMethod = CommonGlobalsType.GetMethod("GetCurrentServiceProvider", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException("Unable to retrieve Globals.GetCurrentServiceProvider method via reflection");
        var serviceProvider = getCurrentServiceProviderMethod.Invoke(null, []);
        return (IServiceProvider)serviceProvider;
    }
}
