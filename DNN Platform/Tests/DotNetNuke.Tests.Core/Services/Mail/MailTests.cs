using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
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
            Assert.AreEqual("Hello World", result.Trim());
        }
        [Test]
        public void GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly()
        {
            // special character
            MailMessage mailMessage = new MailMessage()
            {
                IsBodyHtml = false
            };
            ContentType plain = new ContentType("text/plain")
            {
                CharSet = "us-ascii"
            };
            AlternateView plainView = AlternateView.CreateAlternateViewFromString("body\n", plain);
            
            DotNetNuke.Services.Mail.Mail.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

            AssertEqualAlternativeView(plainView, mailMessage.AlternateViews[0]);
            Assert.AreEqual(1, mailMessage.AlternateViews.Count);
        }

        [Test]
        public void GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews()
        {
            // special character
            MailMessage mailMessage = new MailMessage()
            {
                IsBodyHtml = true
            };
            ContentType plain = new ContentType("text/plain")
            {
                CharSet = "us-ascii"
            };
            ContentType html = new ContentType("text/html")
            {
                CharSet = "us-ascii"
            };
            AlternateView plainView = AlternateView.CreateAlternateViewFromString("body\n", plain);
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString("body\n", html);

            DotNetNuke.Services.Mail.Mail.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

            AssertEqualAlternativeView(plainView, mailMessage.AlternateViews[0]);
            AssertEqualAlternativeView(htmlView, mailMessage.AlternateViews[1]);
            Assert.AreEqual(2, mailMessage.AlternateViews.Count);
        }

        private static void AssertEqualAlternativeView(AlternateView expected, AlternateView actual)
        {
            Assert.AreEqual(expected.ContentType, expected.ContentType);
            Assert.AreEqual(expected.ContentStream, expected.ContentStream);
        }
        
        [Test]
        public void GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii()
        {
            // special character
            MailMessage mailMessage = new MailMessage()
            {
                IsBodyHtml = true
            };
            ContentType plain = new ContentType("text/plain")
            {
                CharSet = "us-ascii"
            };
            ContentType html = new ContentType("text/html")
            {
                CharSet = "us-ascii"
            };

            DotNetNuke.Services.Mail.Mail.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

            Assert.AreEqual(plain, mailMessage.AlternateViews[0].ContentType);
            Assert.AreEqual(html, mailMessage.AlternateViews[1].ContentType);

            // no special character
            mailMessage = new MailMessage()
            {
                IsBodyHtml = true
            };

            DotNetNuke.Services.Mail.Mail.AddAlternateView(mailMessage, "body", Encoding.ASCII);

            Assert.AreEqual(plain, mailMessage.AlternateViews[0].ContentType);
            Assert.AreEqual(html, mailMessage.AlternateViews[1].ContentType);
        }

        [Test]
        public void GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAwaysUTF8()
        {
            // special character
            MailMessage mailMessage = new MailMessage()
            {
                IsBodyHtml = true
            };
            ContentType plain = new ContentType("text/plain")
            {
                CharSet = "utf-8"
            };
            ContentType html = new ContentType("text/html")
            {
                CharSet = "utf-8"
            };

            DotNetNuke.Services.Mail.Mail.AddAlternateView(mailMessage, "body\n", Encoding.UTF8);

            Assert.AreEqual(plain, mailMessage.AlternateViews[0].ContentType);
            Assert.AreEqual(html, mailMessage.AlternateViews[1].ContentType);

            // no special character
            mailMessage = mailMessage = new MailMessage()
            {
                IsBodyHtml = true
            };

            DotNetNuke.Services.Mail.Mail.AddAlternateView(mailMessage, "body", Encoding.UTF8);

            Assert.AreEqual(plain, mailMessage.AlternateViews[0].ContentType);
            Assert.AreEqual(html, mailMessage.AlternateViews[1].ContentType);
        }
    }
}
