// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System.Web.Mvc;

    /// <summary>
    /// Internal services framework contract used by the MVC pipeline to manage script and anti-forgery support.
    /// </summary>
    internal interface IMvcServiceFrameworkInternals
    {
        /// <summary>
        /// Gets a value indicating whether AJAX anti-forgery support has been requested for the current request.
        /// </summary>
        bool IsAjaxAntiForgerySupportRequired { get; }

        /// <summary>
        /// Gets a value indicating whether AJAX script support has been requested for the current request.
        /// </summary>
        bool IsAjaxScriptSupportRequired { get; }

        /// <summary>
        /// Registers services-framework AJAX anti-forgery scripts and variables on the page.
        /// </summary>
        void RegisterAjaxAntiForgery();

        /// <summary>
        /// Registers services-framework AJAX scripts and variables on the page.
        /// </summary>
        void RegisterAjaxScript();
    }
}
