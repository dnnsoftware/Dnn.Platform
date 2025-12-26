// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Skin-related helper methods shared across MVC skin helper partials.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Gets the path to a resource file located in the specified template directory.
        /// </summary>
        /// <param name="templateSourceDirectory">The template source directory.</param>
        /// <param name="fileName">The resource file name.</param>
        /// <returns>The full relative path to the resource file.</returns>
        public static string GetResourceFile(string templateSourceDirectory, string fileName)
        {
            return templateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + fileName;
        }

        /// <summary>
        /// Gets the path to a resource file located under the admin skins directory.
        /// </summary>
        /// <param name="fileName">The resource file name.</param>
        /// <returns>The full relative path to the admin skins resource file.</returns>
        public static string GetSkinsResourceFile(string fileName)
        {
            return GetResourceFile("/admin/Skins", fileName);
        }

        /// <summary>
        /// Gets the dependency injection service provider from the current <see cref="DnnPageController"/>.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The service provider associated with the current controller.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current controller is not a <see cref="DnnPageController"/>.
        /// </exception>
        internal static IServiceProvider GetDependencyProvider(HtmlHelper htmlHelper)
        {
            var controller = htmlHelper.ViewContext.Controller as DnnPageController;

            if (controller == null)
            {
                throw new InvalidOperationException("The HtmlHelper can only be used from DnnPageController");
            }

            return controller.DependencyProvider;
        }
    }
}
