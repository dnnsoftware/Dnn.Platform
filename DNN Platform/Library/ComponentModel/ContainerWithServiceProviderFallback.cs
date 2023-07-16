// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel;

using System;
using System.Collections;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A container which gets components from an <see cref="IServiceProvider"/> for components not registered.</summary>
public class ContainerWithServiceProviderFallback : IContainer
{
    private readonly IContainer container;
    private readonly IServiceProvider serviceProvider;

    /// <summary>Initializes a new instance of the <see cref="ContainerWithServiceProviderFallback"/> class.</summary>
    /// <param name="container">The container to wrap.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public ContainerWithServiceProviderFallback(IContainer container, IServiceProvider serviceProvider)
    {
        this.container = container;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public string Name
        => nameof(ContainerWithServiceProviderFallback);

    /// <inheritdoc />
    public void RegisterComponent<TComponent>()
        where TComponent : class
        => this.container.RegisterComponent<TComponent>();

    /// <inheritdoc />
    public void RegisterComponent<TComponent>(string name)
        where TComponent : class
        => this.container.RegisterComponent<TComponent>(name);

    /// <inheritdoc />
    public void RegisterComponent<TComponent>(string name, ComponentLifeStyleType lifestyle)
        where TComponent : class
        => this.container.RegisterComponent<TComponent>(name, lifestyle);

    /// <inheritdoc />
    public void RegisterComponent<TContract, TComponent>()
        where TComponent : class
        => this.container.RegisterComponent<TContract, TComponent>();

    /// <inheritdoc />
    public void RegisterComponent<TContract, TComponent>(string name)
        where TComponent : class
        => this.container.RegisterComponent<TContract, TComponent>(name);

    /// <inheritdoc />
    public void RegisterComponent<TContract, TComponent>(string name, ComponentLifeStyleType lifestyle)
        where TComponent : class
        => this.container.RegisterComponent<TContract, TComponent>(name, lifestyle);

    /// <inheritdoc />
    public void RegisterComponent(Type componentType)
        => this.container.RegisterComponent(componentType);

    /// <inheritdoc />
    public void RegisterComponent(Type contractType, Type componentType)
        => this.container.RegisterComponent(contractType, componentType);

    /// <inheritdoc />
    public void RegisterComponent(Type contractType, Type componentType, ComponentLifeStyleType lifestyle)
        => this.container.RegisterComponent(contractType, componentType, lifestyle);

    /// <inheritdoc />
    public void RegisterComponent(string name, Type componentType)
        => this.container.RegisterComponent(name, componentType);

    /// <inheritdoc />
    public void RegisterComponent(string name, Type contractType, Type componentType)
        => this.container.RegisterComponent(name, contractType, componentType);

    /// <inheritdoc />
    public void RegisterComponentInstance<TContract>(string name, object instance)
        => this.container.RegisterComponentInstance<TContract>(name, instance);

    /// <inheritdoc />
    public void RegisterComponentSettings(string name, IDictionary dependencies)
        => this.container.RegisterComponentSettings(name, dependencies);

    /// <inheritdoc />
    public void RegisterComponentSettings(Type component, IDictionary dependencies)
        => this.container.RegisterComponentSettings(component, dependencies);

    /// <inheritdoc />
    public void RegisterComponentSettings<TComponent>(IDictionary dependencies)
        => this.container.RegisterComponentSettings<TComponent>(dependencies);

    /// <inheritdoc />
    public void RegisterComponent(string name, Type contractType, Type componentType, ComponentLifeStyleType lifestyle)
        => this.container.RegisterComponent(name, contractType, componentType, lifestyle);

    /// <inheritdoc />
    public void RegisterComponentInstance(string name, object instance)
        => this.container.RegisterComponentInstance(name, instance);

    /// <inheritdoc />
    public void RegisterComponentInstance(string name, Type contractType, object instance)
        => this.container.RegisterComponentInstance(name, contractType, instance);

    /// <inheritdoc />
    public void RegisterComponentInstance<TContract>(object instance)
        => this.container.RegisterComponentInstance<TContract>(instance);

    /// <inheritdoc />
    public object GetComponent(string name)
        => this.container.GetComponent(name);

    /// <inheritdoc />
    public TContract GetComponent<TContract>()
        => this.container.GetComponent<TContract>() ?? this.serviceProvider.GetService<TContract>();

    /// <inheritdoc />
    public TContract GetComponent<TContract>(string name)
        => this.container.GetComponent<TContract>(name);

    /// <inheritdoc />
    public object GetComponent(string name, Type contractType)
        => this.container.GetComponent(name, contractType);

    /// <inheritdoc />
    public string[] GetComponentList<TContract>()
        => this.container.GetComponentList<TContract>();

    /// <inheritdoc />
    public string[] GetComponentList(Type contractType)
        => this.container.GetComponentList(contractType);

    /// <inheritdoc />
    public IDictionary GetComponentSettings(string name)
        => this.container.GetComponentSettings(name);

    /// <inheritdoc />
    public IDictionary GetComponentSettings(Type component)
        => this.container.GetComponentSettings(component);

    /// <inheritdoc />
    public IDictionary GetComponentSettings<TComponent>()
        => this.container.GetComponentSettings<TComponent>();

    /// <inheritdoc />
    public object GetComponent(Type contractType)
        => this.container.GetComponent(contractType) ?? this.serviceProvider.GetService(contractType);
}
