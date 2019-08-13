using Microsoft.QualityTools.Testing.Fakes;
using DotNetNuke.Framework;
using Moq;
using NUnit.Framework;
using System;
using System.Web.UI; // This line is manually added in order not to break the build

namespace DotNetNuke.Tests.Core.Framework
{
    [TestFixture]
    public class jQueryTests
    {
        private MockRepository mockRepository;
        private IDisposable shimsContext;


        [SetUp]
        public void SetUp()
        {
            this.shimsContext = ShimsContext.Create();
            this.mockRepository = new MockRepository(MockBehavior.Strict);

        }

        [TearDown]
        public void TearDown()
        {
            this.shimsContext.Dispose();
            this.mockRepository.VerifyAll();
        }

        private jQuery CreatejQuery()
        {
            return new jQuery();
        }

        [Test]
        public void JQueryFileMapPath_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            bool getMinFile = false;

            // Act
            var result = jQuery.JQueryFileMapPath(
                getMinFile);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void JQueryUIFileMapPath_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            bool getMinFile = false;

            // Act
            var result = jQuery.JQueryUIFileMapPath(
                getMinFile);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void JQueryFile_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            bool getMinFile = false;

            // Act
            var result = jQuery.JQueryFile(
                getMinFile);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void JQueryMigrateFile_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            bool getMinFile = false;

            // Act
            var result = jQuery.JQueryMigrateFile(
                getMinFile);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void JQueryUIFile_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            bool getMinFile = false;

            // Act
            var result = jQuery.JQueryUIFile(
                getMinFile);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetJQueryScriptReference_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();

            // Act
            var result = jQuery.GetJQueryScriptReference();

            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetJQueryMigrateScriptReference_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();

            // Act
            var result = jQuery.GetJQueryMigrateScriptReference();

            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetJQueryUIScriptReference_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();

            // Act
            var result = jQuery.GetJQueryUIScriptReference();

            // Assert
            Assert.Fail();
        }

        [Test]
        public void KeepAlive_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            Page page = null;

            // Act
            jQuery.KeepAlive(
                page);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RegisterJQuery_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            Page page = null;

            // Act
            jQuery.RegisterJQuery(
                page);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RegisterJQueryUI_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            Page page = null;

            // Act
            jQuery.RegisterJQueryUI(
                page);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RegisterDnnJQueryPlugins_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            Page page = null;

            // Act
            jQuery.RegisterDnnJQueryPlugins(
                page);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RegisterHoverIntent_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            Page page = null;

            // Act
            jQuery.RegisterHoverIntent(
                page);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RegisterFileUpload_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();
            Page page = null;

            // Act
            jQuery.RegisterFileUpload(
                page);

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RequestRegistration_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();

            // Act
            jQuery.RequestRegistration();

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RequestUIRegistration_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();

            // Act
            jQuery.RequestUIRegistration();

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RequestDnnPluginsRegistration_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();

            // Act
            jQuery.RequestDnnPluginsRegistration();

            // Assert
            Assert.Fail();
        }

        [Test]
        public void RequestHoverIntentRegistration_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var jQuery = this.CreatejQuery();

            // Act
            jQuery.RequestHoverIntentRegistration();

            // Assert
            Assert.Fail();
        }
    }
}
