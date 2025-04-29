// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Extensions;

using System.Collections;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

/// <summary>Dependency injection extensions for HttpContext.</summary>
public static class HttpContextDependencyInjectionExtensions
{
    /// <summary>Sets the service scope for the http context.</summary>
    /// <param name="httpContext">The http context.</param>
    /// <param name="scope">The service scope.</param>
    public static void SetScope(this HttpContext httpContext, IServiceScope scope)
        => SetScope(new HttpContextWrapper(httpContext), scope);

    /// <summary>Sets the service scope for the http context base.</summary>
    /// <param name="httpContext">The http context base.</param>
    /// <param name="scope">The service scope.</param>
    public static void SetScope(this HttpContextBase httpContext, IServiceScope scope)
    {
        if (!httpContext.Items.Contains(typeof(IServiceScope)))
        {
            httpContext.Items[typeof(IServiceScope)] = scope;
        }
    }

    /// <summary>Clears the service scope for the http context.</summary>
    /// <param name="httpContext">The http context on which to clear the service scope.</param>
    public static void ClearScope(this HttpContext httpContext)
        => ClearScope(new HttpContextWrapper(httpContext));

    /// <summary>Clears the service scope for the http context.</summary>
    /// <param name="httpContext">The http context on which to clear the service scope.</param>
    public static void ClearScope(this HttpContextBase httpContext)
    {
        httpContext.Items.Remove(typeof(IServiceScope));
    }

    /// <summary>Gets the http context service scope.</summary>
    /// <param name="httpContext">The http context from which to get the scope from.</param>
    /// <returns>A service scope.</returns>
    public static IServiceScope GetScope(this HttpContext httpContext)
        => GetScope(new HttpContextWrapper(httpContext));

    /// <summary>Gets the http context base service scope.</summary>
    /// <param name="httpContext">The http context base from which to get the scope from.</param>
    /// <returns>A service scope.</returns>
    public static IServiceScope GetScope(this HttpContextBase httpContext)
    {
        var scope = httpContext.Items.GetScope();
        if (scope is not null || Globals.DependencyProvider is null)
        {
            return scope;
        }

        var scopeLock = new object();
        const string ScopeLockName = "GetScope_lock";
        if (httpContext.Items.Contains(ScopeLockName))
        {
            // another thread is adding the scope, try again
            return GetScope(httpContext);
        }

        httpContext.Items.Add(ScopeLockName, scopeLock);
        lock (httpContext.Items[ScopeLockName])
        {
            if (httpContext.Items[ScopeLockName] == scopeLock)
            {
                if (!httpContext.Items.Contains(typeof(IServiceScope)))
                {
                    scope = Globals.DependencyProvider.CreateScope();
                    httpContext.Items[typeof(IServiceScope)] = scope;
                }
            }
        }

        if (scope is not null)
        {
            httpContext.AddOnRequestCompleted(DisposeScope);
            return scope;
        }

        return GetScope(httpContext);
    }

    private static IServiceScope GetScope(this IDictionary httpContextItems)
    {
        return httpContextItems[typeof(IServiceScope)] as IServiceScope;
    }

    private static void DisposeScope(HttpContextBase httpContext)
    {
        httpContext.Items.GetScope()?.Dispose();
        httpContext.ClearScope();
    }
}
