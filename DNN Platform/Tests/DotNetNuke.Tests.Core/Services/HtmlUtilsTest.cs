// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services
{
    using DotNetNuke.Common.Utilities;
    using NUnit.Framework;

    /// <summary>
    /// </summary>
    [TestFixture]
    public class HtmlUtilsTests
    {
        private const string HtmlStr =
            "Hello World!<br /><br />This is a sample HTML text for testing!<br /><br /><img alt=\"HappyFaceAlt\" title=\"HappyFaceTitle\" test=\"noShow\" src=\"/dotnetnuke_enterprise/Portals/0/Telerik/images/Emoticons/1.gif\" /><br /><br /><img alt=\"\" src=\"http://localhost/dotnetnuke_enterprise/Portals/0/aspnet.gif\" /><br /><br /><a href=\"https://www.dnnsoftware.com\">DotNetNuke Corp.</a>";

        private const string Filters = "alt|href|src|title";
        private string _expected = string.Empty;

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Timeout(2000)]
        public void DNN_12430_IsHtml_RegExTimeout()
        {
            var result =
                HtmlUtils.IsHtml(
                    @"'New Event: <a href=\""http://localhost/dnn540/Home/tabid/40/ModuleID/389/ItemID/8/mctl/EventDetails/Default.aspx"">Test</a> on Saturday, May 08, 2010 2:00 AM to Saturday, May 08, 2010 2:30 AM  - One time event - has been added");

            Assert.IsTrue(result);
        }

        [Test]
        [Timeout(2000)]
        public void DNN_12926_IsHtml_Detection()
        {
            var result = HtmlUtils.IsHtml("this is a test of dnnmail: <a href='https://www.dnnsoftware.com'>DotNetNuke</a>");

            Assert.IsTrue(result);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void HtmlUtils_CleanWithTagInfo_Should_Return_Clean_Content_With_Attribute_Values()
        {
            // Arrange
            this.SetUp();
            this._expected =
                "Hello World This is a sample HTML text for testing DotNetNuke Corp HappyFaceAlt HappyFaceTitle /dotnetnuke_enterprise/Portals/0/Telerik/images/Emoticons/1.gif http://localhost/dotnetnuke_enterprise/Portals/0/aspnet.gif https://www.dnnsoftware.com ";

            // Act
            object retValue = HtmlUtils.CleanWithTagInfo(HtmlStr, Filters, true);

            // Assert
            Assert.AreEqual(this._expected, retValue);

            // TearDown
            this.TearDown();
        }

        [Test]
        public void HtmlUtils_CleanWithTagInfo_Should_Return_Clean_Content_Without_Attribute_Values()
        {
            // Arrange
            this.SetUp();
            this._expected = "Hello World This is a sample HTML text for testing DotNetNuke Corp ";

            // Act
            object retValue = HtmlUtils.CleanWithTagInfo(HtmlStr, " ", true);

            // Assert
            Assert.AreEqual(this._expected, retValue);

            // TearDown
            this.TearDown();
        }

        [Test]
        public void HtmlUtils_StripUnspecifiedTags_Should_Return_Attribute_Values()
        {
            // Arrange
            this.SetUp();
            this._expected =
                "Hello World!This is a sample HTML text for testing!DotNetNuke Corp. \"HappyFaceAlt\" \"HappyFaceTitle\" \"/dotnetnuke_enterprise/Portals/0/Telerik/images/Emoticons/1.gif\" \"\" \"http://localhost/dotnetnuke_enterprise/Portals/0/aspnet.gif\" \"https://www.dnnsoftware.com\"";

            // Act
            object retValue = HtmlUtils.StripUnspecifiedTags(HtmlStr, Filters, false);

            // Assert
            Assert.AreEqual(this._expected, retValue);

            // TearDown
            this.TearDown();
        }

        [Test]
        public void HtmlUtils_StripUnspecifiedTags_Should_Not_Return_Attribute_Values()
        {
            // Arrange
            this.SetUp();
            this._expected = "Hello World!This is a sample HTML text for testing!DotNetNuke Corp.";

            // Act
            object retValue = HtmlUtils.StripUnspecifiedTags(HtmlStr, " ", false);

            // Assert
            Assert.AreEqual(this._expected, retValue);

            // TearDown
            this.TearDown();
        }
    }
}
