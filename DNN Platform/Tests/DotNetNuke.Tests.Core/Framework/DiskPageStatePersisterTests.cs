//using Microsoft.QualityTools.Testing.Fakes; // Commented out from the template
using DotNetNuke.Framework;
using Moq;
using NUnit.Framework;
using System.Web.UI;

namespace DotNetNuke.Tests.Core.Framework
{
    [TestFixture]
    public class DiskPageStatePersisterTests
    {
        private MockRepository mockRepository;
        //private IDisposable shimsContext; // Commented out from the template
        private Mock<Page> mockPage;

        [SetUp]
        public void SetUp()
        {
            //this.shimsContext = ShimsContext.Create(); // Commented out from the template
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockPage = this.mockRepository.Create<Page>();
        }

        [TearDown]
        public void TearDown()
        {
            //this.shimsContext.Dispose(); // Commented out from the template
            this.mockRepository.VerifyAll();
        }

        private DiskPageStatePersister CreateDiskPageStatePersister()
        {
            return new DiskPageStatePersister(
                this.mockPage.Object);
        }

        [Test]
        public void Load_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var diskPageStatePersister = this.CreateDiskPageStatePersister();

            // Act
            diskPageStatePersister.Load();

            // Assert
            Assert.Fail();
        }

        [Test]
        public void Save_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var diskPageStatePersister = this.CreateDiskPageStatePersister();

            // Act
            diskPageStatePersister.Save();

            // Assert
            Assert.Fail();
        }
    }
}
