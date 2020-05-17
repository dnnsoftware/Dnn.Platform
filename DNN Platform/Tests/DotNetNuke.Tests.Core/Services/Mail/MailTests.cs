// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Services.Mail
{
    [TestFixture]
    public class MailTests
    {
        [Test]
        public void ConvertToText_returns_the_input_for_simple_strings()
        {
            //arrange
            Func<string, string> sut = DotNetNuke.Services.Mail.Mail.ConvertToText;
            //act
            var result = sut("Hello World");
            //assert
            Assert.AreEqual("Hello World", result);
        }
        [Test]
        public void ConvertToText_removes_tags()
        {
            //arrange
            Func<string, string> sut = DotNetNuke.Services.Mail.Mail.ConvertToText;
            //act
            var result = sut("<div class=\"x\"><p>Hello World</p></div>");
            //assert
            Assert.AreEqual("Hello World", result.Trim());
        }

        [Test]
        public void ConvertToText_removes_styles_including_css_defs()
        {
            //arrange
            Func<string, string> sut = DotNetNuke.Services.Mail.Mail.ConvertToText;
            //act
            var result = sut("<style>\r\nHello</style>World");     
            //assert
            Assert.AreEqual("World", result.Trim());
        }
    }
}
