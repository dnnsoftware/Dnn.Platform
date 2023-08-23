// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Mail
{
    using Dnn.ExchangeOnlineAuthProvider.Components;
    using Dnn.GoogleMailAuthProvider.Components;
    using DotNetNuke.Framework.Internal.Reflection;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Services.Mail.OAuth;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SmtpOAuthControllerTest
    {
        [Test]
        public void Google_OAuth_Provider_Should_Exists()
        {
            var provider = CreateSmtpOAuthController().GetOAuthProvider("GoogleMail");

            Assert.NotNull(provider);
        }

        [Test]
        public void Exchange_OAuth_Provider_Should_Exists()
        {
            var provider = CreateSmtpOAuthController().GetOAuthProvider("ExchangeOnline");

            Assert.NotNull(provider);
        }

        private static SmtpOAuthController CreateSmtpOAuthController()
        {
            var assemblyLocator = new Mock<IAssemblyLocator>();
            assemblyLocator.SetupGet(al => al.Assemblies)
                .Returns(
                    new[]
                    {
                        new AssemblyWrapper(typeof(GoogleMailOAuthProvider).Assembly),
                        new AssemblyWrapper(typeof(ExchangeOnlineOAuthProvider).Assembly),
                    });

            var typeLocator = new TypeLocator { AssemblyLocator = assemblyLocator.Object, };
            return new SmtpOAuthController(typeLocator);
        }
    }
}
