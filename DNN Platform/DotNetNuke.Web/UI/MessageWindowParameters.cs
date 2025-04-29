// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI;

using System.Web.UI.WebControls;

public class MessageWindowParameters
{
    private string message = string.Empty;
    private string title = string.Empty;
    private Unit windowHeight = Unit.Pixel(175);
    private Unit windowWidth = Unit.Pixel(350);

    /// <summary>Initializes a new instance of the <see cref="MessageWindowParameters"/> class.</summary>
    /// <param name="message">The message.</param>
    public MessageWindowParameters(string message)
    {
        this.message = message;
    }

    /// <summary>Initializes a new instance of the <see cref="MessageWindowParameters"/> class.</summary>
    /// <param name="message">The message.</param>
    /// <param name="title">The title.</param>
    public MessageWindowParameters(string message, string title)
    {
        this.message = message;
        this.title = title;
    }

    /// <summary>Initializes a new instance of the <see cref="MessageWindowParameters"/> class.</summary>
    /// <param name="message">The message.</param>
    /// <param name="title">The title.</param>
    /// <param name="windowWidth">The window width (in a format that can be parsed by <see cref="Unit.Parse(string)" />).</param>
    /// <param name="windowHeight">The window height (in a format that can be parsed by <see cref="Unit.Parse(string)" />).</param>
    public MessageWindowParameters(string message, string title, string windowWidth, string windowHeight)
    {
        this.message = message;
        this.title = title;
        this.windowWidth = Unit.Parse(windowWidth);
        this.windowHeight = Unit.Parse(windowHeight);
    }

    public string Message
    {
        get
        {
            return this.message;
        }

        set
        {
            // todo: javascript encode for onclick events
            this.message = value;
            this.message = this.message.Replace("'", "\\'");
            this.message = this.message.Replace("\"", "\\\"");
        }
    }

    public string Title
    {
        get
        {
            return this.title;
        }

        set
        {
            // todo: javascript encode for onclick events
            this.title = value;
            this.title = this.title.Replace("'", "\\'");
            this.title = this.title.Replace("\"", "\\\"");
        }
    }

    public Unit WindowWidth
    {
        get
        {
            return this.windowWidth;
        }

        set
        {
            this.windowWidth = value;
        }
    }

    public Unit WindowHeight
    {
        get
        {
            return this.windowHeight;
        }

        set
        {
            this.windowHeight = value;
        }
    }
}
