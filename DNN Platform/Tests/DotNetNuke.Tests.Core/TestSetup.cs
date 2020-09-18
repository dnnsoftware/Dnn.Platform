// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Tests.Utilities.Mocks;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core
{
    [SetUpFixture]
    internal class TestSetup
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }
    }
}
