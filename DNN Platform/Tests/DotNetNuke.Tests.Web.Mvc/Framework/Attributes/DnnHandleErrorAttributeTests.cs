﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web.Mvc;
using DotNetNuke.Tests.Web.Mvc.Fakes;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
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
                Exception = testException
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
