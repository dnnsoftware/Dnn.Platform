using System.Collections.Generic;

using DotNetNuke.Services.FileSystem.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    [TestFixture]
    public class DefaultFolderProvidersTests
    {
        #region GetDefaultProviders Tests

        [Test]
        public void GetDefaultProviders_Should_Return_3_Valid_Providers()
        {
            var expectedValues = new List<string> { "StandardFolderProvider", "SecureFolderProvider", "DatabaseFolderProvider" };

            var defaultProviders = DefaultFolderProviders.GetDefaultProviders();

            CollectionAssert.AreEqual(expectedValues, defaultProviders);
        }

        #endregion
    }
}
