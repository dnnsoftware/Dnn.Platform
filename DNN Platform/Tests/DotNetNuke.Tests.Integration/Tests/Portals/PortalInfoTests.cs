// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Portals
{
    using System;
    using System.Linq;

    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PortalInfoTests : DnnWebTest
    {
        public PortalInfoTests()
            : base(Constants.PORTAL_Zero)
        {
        }

        [Test]
        public void Processor_Password_In_DB_Must_Be_Encrypted()
        {
            var firstPortal = PortalController.Instance.GetPortals().OfType<PortalInfo>().FirstOrDefault();
            Assert.NotNull(firstPortal);

            var newPassword = "StringToEncrypt_" + new Random().Next(1, 100);
            firstPortal.ProcessorPassword = newPassword;
            PortalController.Instance.UpdatePortalInfo(firstPortal);

            var result = DatabaseHelper.ExecuteScalar<string>(
                @"SELECT TOP(1) COALESCE(ProcessorPassword, '') FROM {objectQualifier}Portals WHERE PortalID=" + this.PortalId);

            var decrypted = DotNetNuke.Security.FIPSCompliant.DecryptAES(result, Config.GetDecryptionkey(), Host.GUID);

            Assert.AreNotEqual(result, string.Empty);
            Assert.AreNotEqual(result, decrypted);
            Assert.AreEqual(decrypted, newPassword);
        }
    }
}
