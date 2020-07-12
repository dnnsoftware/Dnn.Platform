// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Security.PortalSecurity
{
    using DotNetNuke.Security;
    using NUnit.Framework;

    [TestFixture]
    public class PortalSecurityTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [TestCase("<source></source>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<source>Hi this is personal Test for source tag</source>   ", "    ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<SOURCE>{Upper case}</SOURCE>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<source src=\"https://google.com\">Source with attribute</source>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]

        [TestCase("<source></source>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<source>Hi this is personal Test for source tag</source>   ", "    ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<SOURCE>Upper case</SOURCE>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<source src=\"https://google.com\">Source with attribute</source>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)]

        [TestCase("<source></source>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<source>Hi this is personal Test for source tag</source>   ", "    ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<SOURCE>{Upper case}</SOURCE>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<source src=\"https://google.com\">Source with attribute</source>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]

        [TestCase("<source></source>", "&lt;source&gt;&lt;/source&gt;",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase(
            "<source>Hi this is personal Test for source tag</source>   ",
            "&lt;source&gt;Hi this is personal Test for source tag&lt;/source&gt;   ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<SOURCE>{Upper case}</SOURCE>", "&lt;SOURCE&gt;{Upper case}&lt;/SOURCE&gt;",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase(
            "<source src=\"https://google.com\">Source with attribute</source>",
            "&lt;source src=&quot;https://google.com&quot;&gt;Source with attribute&lt;/source&gt;",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]

        [TestCase("<source></source>", "&lt;source&gt;&lt;/source&gt;",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup)]
        [TestCase(
            "<source>Hi this is personal Test for source tag</source>   ",
            "&lt;source&gt;Hi this is personal Test for source tag&lt;/source&gt;   ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup)]
        [TestCase("<SOURCE>Upper case</SOURCE>", "&lt;SOURCE&gt;Upper case&lt;/SOURCE&gt;",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup)]
        [TestCase(
            "<source src=\"https://google.com\">Source with attribute</source>",
            "&lt;source src=&quot;https://google.com&quot;&gt;Source with attribute&lt;/source&gt;",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup)]

        [TestCase("<source></source>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<source>Hi this is personal Test for source tag</source>   ", "    ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<SOURCE>Upper case</SOURCE>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<source src=\"https://google.com\">Source with attribute</source>", " ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)]

        [TestCase("<source></source>", "<source></source>",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase(
            "<source>Hi this is personal Test for source tag</source>   ",
            "<source>Hi this is personal Test for source tag</source>   ",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<SOURCE>Upper case</SOURCE>", "<SOURCE>Upper case</SOURCE>",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase(
            "<source src=\"https://google.com\">Source with attribute</source>",
            "<source src=\"https://google.com\">Source with attribute</source>",
            DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)]
        public void Html_Source_Tag_Should_Not_Be_Allowed(string html, string expectedOutput,
            DotNetNuke.Security.PortalSecurity.FilterFlag markup)
        {
            // Arrange
            var portalSecurity = new DotNetNuke.Security.PortalSecurity();

            // Act
            var filterOutput = portalSecurity.InputFilter(html, markup);

            // Assert
            Assert.AreEqual(filterOutput, expectedOutput);
        }
    }
}
