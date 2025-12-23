// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Web.MvcPipeline.Modules;

    /// <summary>
    /// Razor result that renders a standardized DNN error message panel.
    /// </summary>
    public class ErrorRazorModuleResult : IRazorModuleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRazorModuleResult"/> class.
        /// </summary>
        /// <param name="heading">The error heading.</param>
        /// <param name="message">The error message.</param>
        public ErrorRazorModuleResult(string heading, string message)
        {
            this.Heading = heading;
            this.Message = message;
        }

        /// <summary>
        /// Gets the error heading.
        /// </summary>
        public string Heading { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; private set; }

        /// <inheritdoc/>
        public IHtmlString Execute(HtmlHelper htmlHelper)
        {
            return htmlHelper.ModuleMessage(this.Message, ModuleMessageType.Error, this.Heading);
        }
    }
}
