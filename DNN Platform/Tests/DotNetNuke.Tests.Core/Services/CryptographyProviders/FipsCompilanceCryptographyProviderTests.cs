// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.CryptographyProviders
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    [TestFixture]
    public class FipsCompilanceCryptographyProviderTests
    {
        private static CryptographyProvider provider;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            ComponentFactory.InstallComponents(new ProviderInstaller("cryptography", typeof(CryptographyProvider), typeof(FipsCompilanceCryptographyProvider)));

            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton<CryptographyProvider, FipsCompilanceCryptographyProvider>());

            provider = ComponentFactory.GetComponent<CryptographyProvider>("FipsCompilanceCryptographyProvider");
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        public void EncryptData_Should_Return_Encrypted_String()
        {
            var message = "Hello world!";
            var encryptionKey = Config.GetDecryptionkey();

            // Arrange

            // Act
            var encryptedValue = provider.EncryptParameter(message, encryptionKey);

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
            var decryptedValue = provider.DecryptParameter(message, encryptionKey);

            // Assert
            Assert.That(decryptedValue, Is.EqualTo(string.Empty));
        }
    }
}
