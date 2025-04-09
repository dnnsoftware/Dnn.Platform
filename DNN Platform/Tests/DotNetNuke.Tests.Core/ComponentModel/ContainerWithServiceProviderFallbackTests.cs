// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.ComponentModel;

using System.Collections;
using System.Collections.Generic;

using DotNetNuke.ComponentModel;
using DotNetNuke.Tests.Utilities.Fakes;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

[TestFixture]
public class ContainerWithServiceProviderFallbackTests
{
    [Test]
    public void GetComponentListSupportsInterfaces()
    {
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), FakeServiceProvider.Create());
        container.RegisterComponent<IList>("payload", ComponentLifeStyleType.Singleton);

        var retrieved = container.GetComponentList(typeof(IList));

        Assert.That(retrieved, Is.EqualTo(new List<string> { "payload" }).AsCollection);
    }

    [Test]
    public void RegisterComponentInstance_Must_Register_In_ComponentsList()
    {
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), FakeServiceProvider.Create());
        ComponentFactory.Container = container;

        container.RegisterComponentInstance("test", typeof(IList<string>), new List<string>());

        Assert.That(ComponentFactory.GetComponents<IList<string>>().Keys, Contains.Item("test"));
    }

    [Test]
    public void GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        container.RegisterComponent<IDictionary, Dictionary<bool, bool>>();

        Assert.That(container.GetComponent<IDictionary>(), Is.AssignableFrom<Dictionary<bool, bool>>());
    }

    [Test]
    public void GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        Assert.That(container.GetComponent<IDictionary>(), Is.AssignableFrom<Dictionary<int, int>>());
    }

    [Test]
    public void NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        Assert.That(container.GetComponent<IDictionary>("Dictionary"), Is.Null);
    }

    [Test]
    public void NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        container.RegisterComponent(typeof(IDictionary), typeof(Dictionary<bool, bool>));

        Assert.That(container.GetComponent(typeof(IDictionary)), Is.AssignableFrom<Dictionary<bool, bool>>());
    }

    [Test]
    public void NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        Assert.That(container.GetComponent(typeof(IDictionary)), Is.AssignableFrom<Dictionary<int, int>>());
    }

    [Test]
    public void NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        Assert.That(container.GetComponent("Dictionary", typeof(IDictionary)), Is.Null);
    }
}
