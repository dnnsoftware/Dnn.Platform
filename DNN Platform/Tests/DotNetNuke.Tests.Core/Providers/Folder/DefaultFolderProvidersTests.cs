// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
