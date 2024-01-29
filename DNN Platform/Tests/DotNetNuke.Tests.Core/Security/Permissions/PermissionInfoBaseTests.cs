// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Security.Permissions
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;
    using NUnit.Framework;

    [TestFixture]
    public class PermissionInfoBaseTests
    {
        [Test]
        public void SerializesXMLProperly()
        {
            // Arrange
            var derived = new DerivedPermissionInfo
            {
                AllowAccess = true,
                DisplayName = "Test Name",
                RoleID = 123,
                RoleName = "Test Role",
                UserID = 234,
                Username = "Test User",
            };

            // Act
            var xml = XmlUtils.Serialize(derived);

            // Assert
            Assert.IsNotNull(xml);
            Assert.True(xml.Contains("allowaccess"));
            Assert.True(xml.Contains("displayname"));
            Assert.True(xml.Contains("roleid"));
            Assert.True(xml.Contains("rolename"));
            Assert.True(xml.Contains("userid"));
            Assert.True(xml.Contains("username"));
        }
    }

    public class DerivedPermissionInfo : PermissionInfoBase
    {
    }
}
