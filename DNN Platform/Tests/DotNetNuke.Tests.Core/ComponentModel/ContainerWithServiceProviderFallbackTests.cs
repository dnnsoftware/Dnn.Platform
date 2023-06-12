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

        CollectionAssert.AreEqual(new List<string> { "payload" }, retrieved);
    }

    [Test]
    public void RegisterComponentInstance_Must_Register_In_ComponentsList()
    {
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), FakeServiceProvider.Create());
        ComponentFactory.Container = container;

        container.RegisterComponentInstance("test", typeof(IList<string>), new List<string>());

        Assert.Contains("test", ComponentFactory.GetComponents<IList<string>>().Keys);
    }

    [Test]
    public void GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        container.RegisterComponent<IDictionary, Dictionary<bool, bool>>();

        Assert.IsAssignableFrom<Dictionary<bool, bool>>(container.GetComponent<IDictionary>());
    }

    [Test]
    public void GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        Assert.IsAssignableFrom<Dictionary<int, int>>(container.GetComponent<IDictionary>());
    }

    [Test]
    public void NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        Assert.IsNull(container.GetComponent<IDictionary>("Dictionary"));
    }

    [Test]
    public void NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        container.RegisterComponent(typeof(IDictionary), typeof(Dictionary<bool, bool>));

        Assert.IsAssignableFrom<Dictionary<bool, bool>>(container.GetComponent(typeof(IDictionary)));
    }

    [Test]
    public void NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        Assert.IsAssignableFrom<Dictionary<int, int>>(container.GetComponent(typeof(IDictionary)));
    }

    [Test]
    public void NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()
    {
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);

        Assert.IsNull(container.GetComponent("Dictionary", typeof(IDictionary)));
    }
}
