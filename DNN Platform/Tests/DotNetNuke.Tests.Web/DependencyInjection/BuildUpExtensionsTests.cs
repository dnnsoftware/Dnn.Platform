// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http.Filters;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.DependencyInjection.Extensions;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public partial class BuildUpExtensionsTests
    {
        // The event logger is used in the test Filters
        private IEventLogger eventLogger;
        private IServiceProvider container;

        [SetUp]
        public void Setup()
        {
            this.eventLogger = Mock.Of<IEventLogger>();

            var mockContainer = new Mock<IServiceProvider>();
            mockContainer.Setup(provider => provider.GetService(typeof(IEventLogger))).Returns(this.eventLogger);

            this.container = mockContainer.Object;
        }

        [TearDown]
        public void TearDown()
        {
            this.eventLogger = null;
            this.container = null;
            CBO.ClearInstance();
        }

        [Test]
        public void CheckParameter_ContainerIsNullTest()
        {
            var container = default(IServiceProvider);

            // This should invoke without exceptions
            container.BuildUp(Mock.Of<IFilter>());
        }

        [Test]
        public void CheckParameter_Filter_IsNullTest()
        {
            var container = Mock.Of<IServiceProvider>();

            // This should invoke without exceptions
            container.BuildUp(null);
        }

        [Test]
        public void BuildUp_PrivateProperty_Test()
        {
            this.MockCBO<PrivateFilterAttribute>();
            var filter = new PrivateFilterAttribute();

            this.container.BuildUp(filter);
            filter.OnActionExecuted(null);

            this.VerifyEventLoggerInvoked();
        }

        [Test]
        public void BuildUp_ProtectedProperty_Test()
        {
            this.MockCBO<ProtectedFilterAttribute>();
            var filter = new ProtectedFilterAttribute();

            this.container.BuildUp(filter);
            filter.OnActionExecuted(null);

            this.VerifyEventLoggerInvoked();

        }

        [Test]
        public void BuildUp_ProtectedInternalProperty_Test()
        {
            this.MockCBO<ProtectedInternalFilterAttribute>();
            var filter = new ProtectedInternalFilterAttribute();

            this.container.BuildUp(filter);
            filter.OnActionExecuted(null);

            this.VerifyEventLoggerInvoked();
        }

        [Test]
        public void BuildUp_InternalProperty_Test()
        {
            this.MockCBO<InternalFilterAttribute>();
            var filter = new InternalFilterAttribute();

            this.container.BuildUp(filter);
            filter.OnActionExecuted(null);

            this.VerifyEventLoggerInvoked();
        }

        [Test]
        public void BuildUp_NoDependencyAttribute_Test()
        {
            this.MockCBO<PublicNoDependencyAttributeFilterAttribute>();
            var filter = new PublicNoDependencyAttributeFilterAttribute();

            this.container.BuildUp(filter);
            Assert.Throws<NullReferenceException>(() => filter.OnActionExecuted(null));

            this.VerifyEventLoggerInvoked(Times.Never());
        }

        [Test]
        public void BuildUp_NoSetter_Test()
        {
            this.MockCBO<NoSetFilterAttribute>();
            var filter = new NoSetFilterAttribute();

            this.container.BuildUp(filter);
            Assert.Throws<NullReferenceException>(() => filter.OnActionExecuted(null));

            this.VerifyEventLoggerInvoked(Times.Never());
        }

        void MockCBO<T>()
            where T : IFilter
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(propertyInfo => propertyInfo.GetSetMethod(true) != null && propertyInfo.GetCustomAttribute<DependencyAttribute>() != null)
                .ToList();

            var mockCbo = new Mock<ICBO>();
            mockCbo
                .Setup(x => x.GetCachedObject<IEnumerable<PropertyInfo>>(It.IsAny<CacheItemArgs>(), It.IsAny<CacheItemExpiredCallback>(), It.IsAny<bool>()))
                .Returns(properties);

            CBO.SetTestableInstance(mockCbo.Object);
        }

        private void VerifyEventLoggerInvoked() => this.VerifyEventLoggerInvoked(Times.Once());

        private void VerifyEventLoggerInvoked(Times times) =>
            Mock.Get(this.eventLogger)
                .Verify(logger => logger.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogType>()), times);

    }
}
