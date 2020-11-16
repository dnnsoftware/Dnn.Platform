// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Web.Mvc.Framework.ActionFilters;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;

    [TestFixture]
    public class DnnHandleErrorAttributeTests
    {
        [Test]
        public void OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController()
        {
            // Arrange
            const string expectedMessage = "This attribute can only be applied to Controllers that implement IDnnController";
            var sut = new DnnHandleErrorAttribute();

            var exceptionContext = new ExceptionContext();

            // Act / Assert
            var ex = Assert.Throws<InvalidOperationException>(() => sut.OnException(exceptionContext));
            Assert.AreEqual(expectedMessage, ex.Message);
        }

        [Test]
        public void OnException_Logs_TheException()
        {
            // Arrange
            var testException = new Exception();
            var exceptionContext = new ExceptionContext
            {
                Controller = new FakeDnnController(),
                Exception = testException,
            };

            var mockDnnHandleErrorAttribute = new Mock<DnnHandleErrorAttribute> { CallBase = true };
            mockDnnHandleErrorAttribute.Protected().Setup("LogException", testException);

            var sut = mockDnnHandleErrorAttribute.Object;

            // Act
            sut.OnException(exceptionContext);

            // Assert
            mockDnnHandleErrorAttribute.VerifyAll();
        }
    }
}
