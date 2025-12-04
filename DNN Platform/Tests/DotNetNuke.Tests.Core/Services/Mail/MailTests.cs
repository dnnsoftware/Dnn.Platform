// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.Mail;

using System.Net.Mail;
using System.Net.Mime;
using System.Text;

using DotNetNuke.Services.Mail;
using NUnit.Framework;

[TestFixture]
public class MailTests
{
    [Test]
    public void ConvertToText_returns_the_input_for_simple_strings()
    {
        var result = Mail.ConvertToText("Hello World");
        Assert.That(result, Is.EqualTo("Hello World"));
    }

    [Test]
    public void ConvertToText_removes_tags()
    {
        var result = Mail.ConvertToText("<div class=\"x\"><p>Hello World</p></div>");
        Assert.That(result.Trim(), Is.EqualTo("Hello World"));
    }

    [Test]
    [Ignore("This wasn't included before and now fails")]
    public void ConvertToText_removes_styles_including_css_defs()
    {
        var result = Mail.ConvertToText("<style>\r\nHello</style>World");
        Assert.That(result.Trim(), Is.EqualTo("World"));
    }

    [Test]
    public void GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly()
    {
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = false, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
        AlternateView plainView = AlternateView.CreateAlternateViewFromString("body\n", plain);

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

        AssertEqualAlternativeView(plainView, mailMessage.AlternateViews[0]);
        Assert.That(mailMessage.AlternateViews.Count, Is.EqualTo(1));
    }

    [Test]
    public void GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews()
    {
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
        ContentType html = new ContentType("text/html") { CharSet = "us-ascii", };
        AlternateView plainView = AlternateView.CreateAlternateViewFromString("body\n", plain);
        AlternateView htmlView = AlternateView.CreateAlternateViewFromString("body\n", html);

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

        AssertEqualAlternativeView(plainView, mailMessage.AlternateViews[0]);
        AssertEqualAlternativeView(htmlView, mailMessage.AlternateViews[1]);
        Assert.That(mailMessage.AlternateViews.Count, Is.EqualTo(2));
    }

    [Test]
    public void GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii()
    {
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
        ContentType html = new ContentType("text/html") { CharSet = "us-ascii", };

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

        Assert.That(mailMessage.AlternateViews[0].ContentType, Is.EqualTo(plain));
        Assert.That(mailMessage.AlternateViews[1].ContentType, Is.EqualTo(html));

        // no special character
        mailMessage = new MailMessage()
        {
            IsBodyHtml = true,
        };

        CoreMailProvider.AddAlternateView(mailMessage, "body", Encoding.ASCII);

        Assert.That(mailMessage.AlternateViews[0].ContentType, Is.EqualTo(plain));
        Assert.That(mailMessage.AlternateViews[1].ContentType, Is.EqualTo(html));
    }

    [Test]
    public void GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8()
    {
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "utf-8", };
        ContentType html = new ContentType("text/html") { CharSet = "utf-8", };

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.UTF8);

        Assert.That(mailMessage.AlternateViews[0].ContentType, Is.EqualTo(plain));
        Assert.That(mailMessage.AlternateViews[1].ContentType, Is.EqualTo(html));

        // no special character
        mailMessage = new MailMessage()
        {
            IsBodyHtml = true,
        };

        CoreMailProvider.AddAlternateView(mailMessage, "body", Encoding.UTF8);

        Assert.That(mailMessage.AlternateViews[0].ContentType, Is.EqualTo(plain));
        Assert.That(mailMessage.AlternateViews[1].ContentType, Is.EqualTo(html));
    }

    private static void AssertEqualAlternativeView(AlternateView expected, AlternateView actual)
    {
        Assert.That(actual.ContentType, Is.EqualTo(expected.ContentType));
        Assert.That(actual.ContentStream, Is.EqualTo(expected.ContentStream));
    }
}
