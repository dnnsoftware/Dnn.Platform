// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.AspNetClientCapabilityProvider
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Providers.AspNetClientCapabilityProvider;
    using DotNetNuke.Tests.Utilities.Mocks;
    using NUnit.Framework;

    [TestFixture]
    public class AspNetClientCapabilityProviderTest
    {
        private const string iphoneUserAgent = "Mozilla/5.0 (iPod; U; CPU iPhone OS 4_0 like Mac OS X; en-us) AppleWebKit/532.9 (KHTML, like Gecko) Version/4.0.5 Mobile/8A293 Safari/6531.22.7";
        private Providers.AspNetClientCapabilityProvider.AspNetClientCapabilityProvider _clientCapabilityProvider;

        [SetUp]
        public void Setup()
        {
            this._clientCapabilityProvider = new Providers.AspNetClientCapabilityProvider.AspNetClientCapabilityProvider();

            ComponentFactory.Container = new SimpleContainer();
            var dataProvider = MockComponentProvider.CreateDataProvider();
            dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
            MockComponentProvider.CreateDataCacheProvider();
            MockComponentProvider.CreateEventLogController();

            // create the bin folder
            var folderPath = HostingEnvironment.ApplicationPhysicalPath + "bin";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Empty_ClientCapabilityId()
        {
            // Act
            string nullClientCapabilityId = string.Empty;
            var clientCapabilitiesByNullId = this._clientCapabilityProvider.GetClientCapabilityById(nullClientCapabilityId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Null_ClientCapabilityId()
        {
            // Act
            string nullClientCapabilityId = null;
            var clientCapabilitiesByEmptyId = this._clientCapabilityProvider.GetClientCapabilityById(nullClientCapabilityId);
        }
    }
}
