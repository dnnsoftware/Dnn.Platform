// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Security.Permissions
{
    using System;
    using System.IO;
    using System.Text;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;
    using NUnit.Framework;

    [TestFixture]
    public class PermissionInfoTests
    {
        [Test]
        public void SerializesJson_IncluddingObsoleteProperties()
        {
            // Arrange
            var permissionInfo = new PermissionInfo
            {
                ModuleDefID = 123,
                PermissionCode = "test",
                PermissionID = 456,
                PermissionKey = "testKey",
                PermissionName = "testName",
            };

            // Act
            var json = Json.Serialize(permissionInfo);

            // Assert
            Assert.NotNull(json);
            Assert.False(json.Contains(nameof(permissionInfo.ModuleDefId)));
            Assert.True(json.Contains(nameof(permissionInfo.PermissionCode)));
            Assert.True(json.Contains(nameof(permissionInfo.PermissionID))); // old obsolete casing.
            Assert.True(json.Contains(nameof(permissionInfo.PermissionId))); // new casing.
            Assert.True(json.Contains(nameof(permissionInfo.PermissionKey)));
            Assert.False(json.Contains(nameof(permissionInfo.PermissionName)));
        }

        [Test]
        public void DeserializesJson_Properly()
        {
            // Arrange
            var json = "{\"PermissionCode\":\"test\",\"PermissionID\":456,\"PermissionKey\":\"testKey\"}";

            // Act
            var permissionInfo = Json.Deserialize<PermissionInfo>(json);

            // Assert
            Assert.NotNull(permissionInfo);
            Assert.AreEqual("test", permissionInfo.PermissionCode);
            Assert.AreEqual(456, permissionInfo.PermissionID);
            Assert.AreEqual(456, ((IPermissionDefinitionInfo)permissionInfo).PermissionId);
            Assert.AreEqual("testKey", permissionInfo.PermissionKey);
        }

        [Test]
        public void SerializesXml_IncludesObsoleteProperties()
        {
            // Arrange
            var permissionInfo = new PermissionInfo
            {
                ModuleDefID = 123,
                PermissionCode = "test",
                PermissionID = 456,
                PermissionKey = "testKey",
                PermissionName = "testName",
            };
            // Act
            var xml = XmlUtils.Serialize(permissionInfo);

            // Assert
            Assert.NotNull(xml);
            Assert.False(xml.IndexOf("moduledefid", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.True(xml.Contains("permissioncode"));
            Assert.True(xml.Contains("permissionid"));
            Assert.True(xml.Contains("permissionkey"));
            Assert.False(xml.IndexOf("permissionname", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Test]
        public void Deserializes_Properly()
        {
            // Arrange
            var xml = "<PermissionInfo><permissioncode>test</permissioncode><permissionid>456</permissionid><permissionkey>testKey</permissionkey></PermissionInfo>";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(xml);
            writer.Flush();
            stream.Position = 0;

            // Act
            var permissionInfo = (PermissionInfo)XmlUtils.Deserialize(stream, typeof(PermissionInfo));

            // Assert
            Assert.NotNull(permissionInfo);
            Assert.AreEqual("test", permissionInfo.PermissionCode);
            Assert.AreEqual(456, permissionInfo.PermissionID);
            Assert.AreEqual(456, ((IPermissionDefinitionInfo)permissionInfo).PermissionId);
            Assert.AreEqual("testKey", permissionInfo.PermissionKey);
        }
    }
}
