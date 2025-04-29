// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.CryptographyProviders;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Services.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CoreCryptographyProviderTests
{
    private static CryptographyProvider _provider;

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddSingleton(Mock.Of<INavigationManager>());
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
        ComponentFactory.InstallComponents(new ProviderInstaller("cryptography", typeof(CryptographyProvider), typeof(CoreCryptographyProvider)));

        _provider = ComponentFactory.GetComponent<CryptographyProvider>("CoreCryptographyProvider");
    }

    [Test]
    public void EncryptData_Should_Return_Encrypted_String()
    {
        var message = "Hello world!";
        var encryptionKey = Config.GetDecryptionkey();

        // Arrange

        // Act
        var encryptedValue = _provider.EncryptParameter(message, encryptionKey);

        // Assert
        Assert.That(encryptedValue, Is.Not.EqualTo(message));
    }

    [Test]
    public void DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encypted()
    {
        var message = "Hello world!";
        var encryptionKey = Config.GetDecryptionkey();

        // Arrange

        // Act
        var decryptedValue = _provider.DecryptParameter(message, encryptionKey);

        // Assert
        Assert.That(decryptedValue, Is.EqualTo(string.Empty));
    }
}
