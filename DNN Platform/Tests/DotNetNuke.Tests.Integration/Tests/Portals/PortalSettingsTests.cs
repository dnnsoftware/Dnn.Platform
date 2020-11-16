// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Portals
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PortalSettingsTests : DnnWebTest
    {
        private string _settingName;
        private string _settingValue;

        public PortalSettingsTests()
            : base(Constants.PORTAL_Zero)
        {
        }

        [SetUp]
        public void Setup()
        {
            this._settingName = "NameToCheckFor";

            // we need different value so when we save we force going to database
            this._settingValue = "ValueToCheckFor_" + new Random().Next(1, 100);
        }

        [Test]
        public void Saving_Non_Secure_Value_Doesnt_Encrypt_It()
        {
            // Act
            PortalController.UpdatePortalSetting(this.PortalId, this._settingName, this._settingValue, true, null, false);
            var result = PortalController.GetPortalSetting(this._settingName, this.PortalId, string.Empty);

            // Assert
            Assert.AreNotEqual(result, string.Empty);
            Assert.AreEqual(this._settingValue, result);
        }

        [Test]
        public void Saving_Secure_Value_Encrypts_It()
        {
            // Act
            PortalController.UpdatePortalSetting(this.PortalId, this._settingName, this._settingValue, true, null, true);

            var result = PortalController.GetPortalSetting(this._settingName, this.PortalId, string.Empty);
            var decrypted = DotNetNuke.Security.FIPSCompliant.DecryptAES(result, Config.GetDecryptionkey(), Host.GUID);

            // Assert
            Assert.AreNotEqual(result, string.Empty);
            Assert.AreNotEqual(this._settingValue, result);
            Assert.AreEqual(decrypted, this._settingValue);
        }
    }
}
