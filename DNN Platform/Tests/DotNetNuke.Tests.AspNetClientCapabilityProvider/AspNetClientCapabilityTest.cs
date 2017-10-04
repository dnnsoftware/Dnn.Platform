#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Web;
using System.Web.Configuration;
using DotNetNuke.ComponentModel;
using DotNetNuke.Providers.AspNetClientCapabilityProvider;
using DotNetNuke.Tests.Utilities.Mocks;

using NUnit.Framework;

namespace DotNetNuke.Tests.AspNetClientCapabilityProviderTest
{
    [TestFixture]
    public class AspNetClientCapabilityTest
    {
        #region Private Variables
        private Providers.AspNetClientCapabilityProvider.AspNetClientCapabilityProvider _clientCapabilityProvider;

        #endregion

        #region User Agents

        public const String iphoneUserAgent = "Mozilla/5.0 (iPod; U; CPU iPhone OS 4_0 like Mac OS X; en-us) AppleWebKit/532.9 (KHTML, like Gecko) Version/4.0.5 Mobile/8A293 Safari/6531.22.7";

        public const String wp7UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows Phone OS 7.0; Trident/3.1; IEMobile/7.0) Asus;Galaxy6";

        public const String msIE8UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; InfoPath.3; Creative AutoUpdate v1.40.02)";
        public const String msIE9UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
        public const String msIE10UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";

        public const String fireFox5NT61UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:5.0) Gecko/20110619 Firefox/5.0";

        public const String iPadTabletUserAgent = "Mozilla/5.0 (iPad; U; CPU OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B334b Safari/531.21.10";
        public const String samsungGalaxyTablet = "Mozilla/5.0 (Linux; U; Android 2.2; en-gb; SAMSUNG GT-P1000 Tablet Build/MASTER) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1";

        public const String winTabletPC = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705; Tablet PC 2.0)";


        public const String htcDesireVer1Sub22UserAgent = "Mozilla/5.0 (Linux; U; Android 2.2; sv-se; Desire_A8181 Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1";
        public const String blackBerry9105V1 = "BlackBerry9105/5.0.0.696 Profile/MIDP-2.1 Configuration/CLDC-1.1 VendorID/133";

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
        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion

        #region Testing IsMobile for common smart phones and Tablets

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_True_For_BlackBerry9105V1()
        {
            //Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(blackBerry9105V1);

            // Act
            var bIsMobile = clientCapability.IsMobile;

            //Assert
            Assert.IsTrue(bIsMobile);
        }


        [Test]
        public void AspNetClientCapability_IsMobile_Returns_True_For_IPhone()
        {
            // Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(iphoneUserAgent);             

            // Act
            var bIsMobile = clientCapability.IsMobile;

            //Assert
            Assert.IsTrue(bIsMobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_True_For_WP7()
        {
            //Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(wp7UserAgent);

            // Act
            var bIsMobile = clientCapability.IsMobile;

            //Assert
            Assert.IsTrue(bIsMobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_True_For_IPad()
        {
            //Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(iPadTabletUserAgent);

            // Act            
            var bIsMobile = clientCapability.IsMobile;

            //Assert
            Assert.IsTrue(bIsMobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_True_For_SamsungGalaxyGTP1000()
        {
            //var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", string.Empty);
            //simulator.SetHeader("user-agent", samsungGalaxyTablet);
            //simulator.SimulateRequest();
            //var capabilities = new HttpCapabilitiesDefaultProvider().GetBrowserCapabilities(HttpContext.Current.Request);
            
            //Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(samsungGalaxyTablet);

            // Act
            var isMobile = clientCapability.IsMobile;

            //Assert
            Assert.IsTrue(isMobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_False_For_EmptyUserAgent()
        {
            //Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(String.Empty);

            // Act
            var bIsMmobile = clientCapability.IsMobile;

            //Assert
            Assert.IsFalse(bIsMmobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_False_For_NullUserAgent()
        {
            //Arrange
            String agent = null;
            var AspNetClientCapability = _clientCapabilityProvider.GetClientCapability(agent);

            // Act
            var bIsMobile = AspNetClientCapability.IsMobile;

            //Assert
            Assert.IsFalse(bIsMobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_True_For_HTCDesireVer1Sub22()
        {
            // Arrange
            var AspNetClientCapability = _clientCapabilityProvider.GetClientCapability(htcDesireVer1Sub22UserAgent);

            // Act
            var bIsMobile = AspNetClientCapability.IsMobile;

            //Assert
            Assert.IsTrue(bIsMobile);
        }

        #endregion

        #region Testing IsMobible for common desktop browsers

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_False_For_InternetExplorer8()
        {
            //Arrange
            var AspNetClientCapability = _clientCapabilityProvider.GetClientCapability(msIE8UserAgent);

            // Act
            var bIsMobile = AspNetClientCapability.IsMobile;

            //Assert
            Assert.IsFalse(bIsMobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_False_For_InternetExplorer9()
        {
            //Arrange
            var AspNetClientCapability = _clientCapabilityProvider.GetClientCapability(msIE9UserAgent);

            // Act
            var bIsMobile = AspNetClientCapability.IsMobile;

            //Assert
            Assert.IsFalse(bIsMobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_False_For_InternetExplorer10()
        {
            //Arrange
            var AspNetClientCapability = _clientCapabilityProvider.GetClientCapability(msIE10UserAgent);

            // Act
            var bIsMobile = AspNetClientCapability.IsMobile;

            //Assert
            Assert.IsFalse(bIsMobile);
        }

        [Test]
        public void AspNetClientCapability_IsMobile_Returns_False_For_FireFox()
        {
            //Arrange
            var AspNetClientCapability = _clientCapabilityProvider.GetClientCapability(fireFox5NT61UserAgent);

            // Act
            var bIsMobile = AspNetClientCapability.IsMobile;

            //Assert
            Assert.IsFalse(bIsMobile);
        }

        #endregion

        #region Testing IsTables False for non Tablet devices and User Agents
        [Test]
        public void AspNetClientCapability_IsTablet_Returns_False_For_IPhone()
        {
            // Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(iphoneUserAgent);

            // Act
            var bIsTablet = clientCapability.IsTablet;

            //Assert
            Assert.IsFalse(bIsTablet);
        }

        [Test]
        public void AspNetClientCapability_IsTablet_Returns_True_For_PCTablet()
        {
            // Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(winTabletPC);

            // Act
            var bIsTablet = clientCapability.IsTablet;

            //Assert
            Assert.IsTrue(bIsTablet);
        }

        #endregion

        #region Testing SupportsFlash True User Agents of known Devices

        [Test]
        public void AspNetClientCapability_SupportsFlash_Returns_True_For_HTCDesireVer1Sub22()
        {
            // Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(htcDesireVer1Sub22UserAgent);

            // Act
            var bSupportsFlash = clientCapability.SupportsFlash;

            //Assert
            Assert.IsFalse(bSupportsFlash);
        }

        #endregion

        #region Testing SupportsFlash False User Agents of known Devices

        [Test]
        public void AspNetClientCapability_SupportsFlash_Returns_False_For_WindowsPhone7()
        {
            // Arrange
            var clientCapability = _clientCapabilityProvider.GetClientCapability(wp7UserAgent);

            // Act
            var bIsTablet = clientCapability.IsTablet;

            //Assert
            Assert.IsFalse(bIsTablet);
        }

        #endregion

    }

}
