// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Tests.Portals
{
    [TestFixture]
    public class PortalSettingsTests : DnnWebTest
    {
        private string _settingName;
        private string _settingValue;

        public PortalSettingsTests() : base(Constants.PORTAL_Zero)
        {
        }

        [SetUp]
        public void Setup()
        {
            _settingName = "NameToCheckFor";
            // we need different value so when we save we force going to database
            _settingValue = "ValueToCheckFor_" + new Random().Next(1, 100);
        }

        [Test]
        public void Saving_Non_Secure_Value_Doesnt_Encrypt_It()
        {
            //Act
            PortalController.UpdatePortalSetting(PortalId, _settingName, _settingValue, true, null, false);
            var result = PortalController.GetPortalSetting(_settingName, PortalId, "");

            //Assert
            Assert.AreNotEqual(result, "");
            Assert.AreEqual(_settingValue, result);
        }

        [Test]
        public void Saving_Secure_Value_Encrypts_It()
        {
            //Act
            PortalController.UpdatePortalSetting(PortalId, _settingName, _settingValue, true, null, true);

            var result = PortalController.GetPortalSetting(_settingName, PortalId, "");
            var decrypted = DotNetNuke.Security.FIPSCompliant.DecryptAES(result, Config.GetDecryptionkey(), Host.GUID);

            //Assert
            Assert.AreNotEqual(result, "");
            Assert.AreNotEqual(_settingValue, result);
            Assert.AreEqual(decrypted, _settingValue);
        }
    }
}
