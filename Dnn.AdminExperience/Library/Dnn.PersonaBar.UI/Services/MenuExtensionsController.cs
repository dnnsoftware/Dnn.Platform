// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Repository;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;

    [MenuPermission(Scope = ServiceScope.Regular)]
    public class MenuExtensionsController : PersonaBarApiController
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(MenuExtensionsController));

        /// <summary>
        /// Retrieve a list of extensions for menu.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetExtensions(string menu)
        {
            try
            {
                var menuItem = PersonaBarRepository.Instance.GetMenuItem(menu);

                if (menuItem != null)
                {
                    var extensions = PersonaBarExtensionRepository.Instance.GetExtensions(menuItem.MenuId)
                        .Where(this.IsVisible)
                        .Select(t => new
                        {
                            identifier = t.Identifier,
                            folderName = t.FolderName,
                            container = t.Container,
                            path = this.GetExtensionPathByController(t),
                            settings = this.GetExtensionSettings(t),
                        });

                    return this.Request.CreateResponse(HttpStatusCode.OK, extensions);
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        private string GetExtensionPathByController(PersonaBarExtension extension)
        {
            var menuItem = PersonaBarRepository.Instance.GetMenuItem(extension.MenuId);
            if (menuItem == null)
            {
                return extension.Path;
            }

            var extensionController = this.GetExtensionController(extension);
            var path = extensionController?.GetPath(extension);
            return !string.IsNullOrEmpty(path) ? path : extension.Path;
        }

        private bool IsVisible(PersonaBarExtension extension)
        {
            var extensionController = this.GetExtensionController(extension);
            return extensionController == null || extensionController.Visible(extension);
        }

        private IDictionary<string, object> GetExtensionSettings(PersonaBarExtension extension)
        {
            var extensionController = this.GetExtensionController(extension);
            var settings = extensionController?.GetSettings(extension);
            return settings;
        }

        private IExtensionController GetExtensionController(PersonaBarExtension extension)
        {
            var identifier = extension.Identifier;
            var controller = extension.Controller;

            if (string.IsNullOrEmpty(controller))
            {
                return null;
            }

            try
            {
                var cacheKey = $"PersonaBarExtensionController_{identifier}";
                return Reflection.CreateObject(controller, cacheKey) as IExtensionController;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }
    }
}
