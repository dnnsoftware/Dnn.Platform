﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using DotNetNuke.ComponentModel;
using DotNetNuke.Providers.AspNetClientCapabilityProvider;
using DotNetNuke.Tests.Utilities.Mocks;

using NUnit.Framework;


namespace DotNetNuke.Tests.AspNetClientCapabilityProvider
{
    [TestFixture]
    public class AspNetClientCapabilityProviderTest
    {
        #region Private Variables
        private Providers.AspNetClientCapabilityProvider.AspNetClientCapabilityProvider _clientCapabilityProvider;

        #endregion

        #region UserAgent

        private const String iphoneUserAgent = "Mozilla/5.0 (iPod; U; CPU iPhone OS 4_0 like Mac OS X; en-us) AppleWebKit/532.9 (KHTML, like Gecko) Version/4.0.5 Mobile/8A293 Safari/6531.22.7";

        #endregion

        #region Setup & TearDown

        [SetUp]
        public void Setup()
        {
            _clientCapabilityProvider = new Providers.AspNetClientCapabilityProvider.AspNetClientCapabilityProvider();

			ComponentFactory.Container = new SimpleContainer();
			var dataProvider = MockComponentProvider.CreateDataProvider();
			dataProvider.Setup(d => d.GetProviderPath()).Returns("");
			MockComponentProvider.CreateDataCacheProvider();
			MockComponentProvider.CreateEventLogController();

            //create the bin folder
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

        #endregion

        #region Testing Getting ClientCapabilities based on clientID

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Empty_ClientCapabilityId()
        {
            //Act
            string nullClientCapabilityId = String.Empty;         
            var clientCapabilitiesByNullId = _clientCapabilityProvider.GetClientCapabilityById(nullClientCapabilityId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Null_ClientCapabilityId()
        {
            //Act
            string nullClientCapabilityId = null;
            var clientCapabilitiesByEmptyId = _clientCapabilityProvider.GetClientCapabilityById(nullClientCapabilityId);
        }

        #endregion
    }
}
