// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.CryptographyProviders
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Services.Cryptography;
    using NUnit.Framework;

    [TestFixture]
    public class FipsCompilanceCryptographyProviderTests
    {
        private static CryptographyProvider _provider;

        [SetUp]
        public void Setup()
        {
            ComponentFactory.InstallComponents(new ProviderInstaller("cryptography", typeof(CryptographyProvider), typeof(FipsCompilanceCryptographyProvider)));

            _provider = ComponentFactory.GetComponent<CryptographyProvider>("FipsCompilanceCryptographyProvider");
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
            Assert.AreNotEqual(message, encryptedValue);
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
            Assert.AreEqual(string.Empty, decryptedValue);
        }
    }
}
