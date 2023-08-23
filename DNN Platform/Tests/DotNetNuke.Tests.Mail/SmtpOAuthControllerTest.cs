// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Mail
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Configuration;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Services.Mail.OAuth;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Dnn.GoogleMailAuthProvider.Components;
    using Dnn.ExchangeOnlineAuthProvider.Components;
    using NUnit.Framework;
    using DotNetNuke.Prompt;

    [TestFixture]
    public class SmtpOAuthControllerTest
    {
        [SetUp]
        public void Setup()
        {
            // Load the provider instances.
            var googleProvider = typeof(GoogleMailOAuthProvider);
            var exchangeProvider = typeof(ExchangeOnlineOAuthProvider);
        }

        [Test]
        public void Google_OAuth_Provider_Should_Exists()
        {
            var provider = SmtpOAuthController.Instance.GetOAuthProvider("GoogleMail");

            Assert.NotNull(provider);
        }

        [Test]
        public void Exchange_OAuth_Provider_Should_Exists()
        {
            var provider = SmtpOAuthController.Instance.GetOAuthProvider("ExchangeOnline");

            Assert.NotNull(provider);
        }
    }
}
