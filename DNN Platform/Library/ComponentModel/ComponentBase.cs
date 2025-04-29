// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel;

using System;

/// <summary>A base class for a Dnn component.</summary>
/// <typeparam name="TContract">The contract type.</typeparam>
/// <typeparam name="TType">The component type.</typeparam>
public abstract class ComponentBase<TContract, TType>
    where TType : class, TContract
{
    private static TContract testableInstance;
    private static bool useTestable = false;

    /// <summary>Gets an instance of the Component.</summary>
    public static TContract Instance
    {
        get
        {
            if (useTestable && testableInstance != null)
            {
                return testableInstance;
            }

            var component = ComponentFactory.GetComponent<TContract>();

            if (component == null)
            {
                component = (TContract)Activator.CreateInstance(typeof(TType), true);
                ComponentFactory.RegisterComponentInstance<TContract>(component);
            }

            return component;
        }
    }

    /// <summary>Registers an instance of a component.</summary>
    /// <param name="instance">The instance to register.</param>
    public static void RegisterInstance(TContract instance)
    {
        if (ComponentFactory.GetComponent<TContract>() == null)
        {
            ComponentFactory.RegisterComponentInstance<TContract>(instance);
        }
    }

    /// <summary>Registers an instance to use for the Singleton.</summary>
    /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
    /// <param name="instance">The instance to set.</param>
    internal static void SetTestableInstance(TContract instance)
    {
        testableInstance = instance;
        useTestable = true;
    }

    /// <summary>Clears the current instance, a new instance will be initialized when next requested.</summary>
    /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
    internal static void ClearInstance()
    {
        useTestable = false;
        testableInstance = default(TContract);
    }
}
