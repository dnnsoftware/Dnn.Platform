// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework;

using System;

/// <summary>Provides a readily testable way to manage a Singleton.</summary>
/// <typeparam name="TContract">The interface that the controller provides.</typeparam>
/// <typeparam name="TSelf">The type of the controller itself, used to call the GetFactory override.</typeparam>
public abstract class ServiceLocator<TContract, TSelf>
    where TSelf : ServiceLocator<TContract, TSelf>, new()
{
    private static Lazy<TContract> instance = new Lazy<TContract>(InitInstance, true);
    private static TContract testableInstance;
    private static bool useTestable;

    /// <summary>Gets a singleton of T.</summary>
    public static TContract Instance
    {
        get
        {
            if (useTestable)
            {
                return testableInstance;
            }

            return instance.Value;
        }
    }

    /// <summary>Gets or sets the service locator factory.</summary>
    protected static Func<TContract> Factory { get; set; }

    /// <summary>Registers an instance to use for the Singleton.</summary>
    /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
    /// <param name="instance">The instance to set.</param>
    public static void SetTestableInstance(TContract instance)
    {
        testableInstance = instance;
        useTestable = true;
    }

    /// <summary>Clears the current instance, a new instance will be initialized when next requested.</summary>
    /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
    public static void ClearInstance()
    {
        useTestable = false;
        testableInstance = default(TContract);
        instance = new Lazy<TContract>(InitInstance, true);
    }

    /// <summary>Gets the service locator factory.</summary>
    /// <returns>A factory function.</returns>
    protected abstract Func<TContract> GetFactory();

    private static TContract InitInstance()
    {
        if (Factory == null)
        {
            var controllerInstance = new TSelf();
            Factory = controllerInstance.GetFactory();
        }

        return Factory();
    }
}
