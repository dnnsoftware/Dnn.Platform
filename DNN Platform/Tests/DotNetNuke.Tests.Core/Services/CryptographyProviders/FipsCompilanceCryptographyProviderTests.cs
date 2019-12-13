﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Services.Cryptography;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Services.CryptographyProviders
{
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
            //Arrange

            //Act
            var encryptedValue = _provider.EncryptParameter(message, encryptionKey);

            //Assert
            Assert.AreNotEqual(message, encryptedValue);
        }

        [Test]
        public void DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encypted()
        {
            var message = "Hello world!";
            var encryptionKey = Config.GetDecryptionkey();
            //Arrange

            //Act
            var decryptedValue = _provider.DecryptParameter(message, encryptionKey);

            //Assert
            Assert.AreEqual(string.Empty, decryptedValue);
        }
    }
}
