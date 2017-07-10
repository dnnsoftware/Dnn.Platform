#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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

namespace Dnn.PersonaBar.UI.Services
{
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class MenuExtensionsController : PersonaBarApiController
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(MenuExtensionsController));

        #region Public API methods

        /// <summary>
        /// Retrieve a list of extensions for menu. 
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetExtensions(string menu)
        {
            try
            {
                var menuItem = PersonaBarRepository.Instance.GetMenuItem(menu);

                if (menuItem != null)
                {
                    var extensions = PersonaBarExtensionRepository.Instance.GetExtensions(menuItem.MenuId)
                        .Where(IsVisible)
                        .Select(t => new
                        {
                            identifier = t.Identifier,
                            folderName = t.FolderName,
                            container = t.Container,
                            path = GetExtensionPathByController(t),
                            settings = GetExtensionSettings(t)
                        });

                    return Request.CreateResponse(HttpStatusCode.OK, extensions);
                }

                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        #endregion

        private string GetExtensionPathByController(PersonaBarExtension extension)
        {
            var menuItem = PersonaBarRepository.Instance.GetMenuItem(extension.MenuId);
            if (menuItem == null)
            {
                return extension.Path;
            }

            var extensionController = GetExtensionController(extension);
            var path = extensionController?.GetPath(extension);
            return !string.IsNullOrEmpty(path) ? path : extension.Path;
        }

        private bool IsVisible(PersonaBarExtension extension)
        {
            var extensionController = GetExtensionController(extension);
            return extensionController == null || extensionController.Visible(extension);
        }

        private IDictionary<string, object> GetExtensionSettings(PersonaBarExtension extension)
        {
            var extensionController = GetExtensionController(extension);
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