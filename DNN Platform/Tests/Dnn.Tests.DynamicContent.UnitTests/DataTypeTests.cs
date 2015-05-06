// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
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
        }
    }
}
