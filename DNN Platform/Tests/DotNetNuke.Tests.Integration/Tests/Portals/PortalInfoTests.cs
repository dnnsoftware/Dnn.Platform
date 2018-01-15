#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Linq;
using DNN.Integration.Test.Framework.Helpers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Tests.Portals
{
    [TestFixture]
    public class PortalInfoTests : DnnWebTest
    {
        public PortalInfoTests() : base(Constants.PORTAL_Zero)
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
                @"SELECT TOP(1) COALESCE(ProcessorPassword, '') FROM {objectQualifier}Portals WHERE PortalID=" + PortalId);

            var decrypted = DotNetNuke.Security.FIPSCompliant.DecryptAES(result, Config.GetDecryptionkey(), Host.GUID);

            Assert.AreNotEqual(result, "");
            Assert.AreNotEqual(result, decrypted);
            Assert.AreEqual(decrypted, newPassword);
        }
    }
}