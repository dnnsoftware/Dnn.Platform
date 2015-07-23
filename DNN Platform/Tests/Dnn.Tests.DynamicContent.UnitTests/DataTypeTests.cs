// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    class DataTypeTests
    {
        [Test]
        public void Constructor_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var dataType = new DataType();

            //Assert
            Assert.AreEqual(UnderlyingDataType.String, dataType.UnderlyingDataType);
            Assert.AreEqual(-1, dataType.PortalId);
            Assert.AreEqual(-1, dataType.CreatedByUserId);
            Assert.AreEqual(-1, dataType.LastModifiedByUserId);
        }

        [Test]
        public void Constructor_Sets_PortalId_Property()
        {
            //Arrange

            //Act
            var dataType = new DataType(Constants.CONTENT_ValidPortalId);

            //Assert
            Assert.AreEqual(Constants.CONTENT_ValidPortalId, dataType.PortalId);
        }

        [TestCase(-1, true)]
        [TestCase(0, false)]
        public void IsSystem_Returns_Correct_Value(int portalId, bool isSystem)
        {
            //Arrange

            //Act
            var dataType = new DataType(portalId);

            //Assert
            Assert.AreEqual(isSystem, dataType.IsSystem);
        }
    }
}
