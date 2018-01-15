#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Connections;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Connectors.Services
{
    [MenuPermission(MenuName = "Dnn.Connectors")]
    public class ConnectorsController : PersonaBarApiController
    {
        #region Properties

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConnectorsController));

        #endregion

        #region Public Methods

        [HttpGet]
        public HttpResponseMessage GetConnections()
        {
            var connections = GetConnections(PortalId)
                .ForEach(x =>
                {
                    {
                        x.HasConfig(PortalSettings.PortalId);
                    }
                })
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    type = c.Type,
                    displayName = c.DisplayName,
                    connected = c.HasConfig(PortalSettings.PortalId),
                    icon = Globals.ResolveUrl(c.IconUrl),
                    pluginFolder = Globals.ResolveUrl(c.PluginFolder),
                    configurations = c.GetConfig(PortalSettings.PortalId),
                    supportsMultiple = c.SupportsMultiple
                }).OrderBy(connection => connection.name).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, connections);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveConnection(object postData)
        {
            try
            {
                var jsonData = DotNetNuke.Common.Utilities.Json.Serialize(postData);
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] {new DynamicJsonConverter()});

                dynamic postObject = serializer.Deserialize(jsonData, typeof (object));

                var name = postObject.name;
                var displayName = postObject.displayName;
                var id = postObject.id;
                var connectors =
                    GetConnections(PortalId).ForEach(x =>
                    {
                        {
                            x.HasConfig(PortalSettings.PortalId);
                        }
                    })
                        .Where(
                            c =>
                                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

                var connector = connectors.FirstOrDefault(c =>
                    (c.SupportsMultiple && (c.Id == id || string.IsNullOrEmpty(c.Id))) ||
                    !c.SupportsMultiple);

                if (connector == null && string.IsNullOrEmpty(id) && connectors.Any(x => x.SupportsMultiple))
                {
                    connector = connectors.First();
                    connector.Id = null;
                    connector.DisplayName = null;
                }
                if (connector != null && !string.IsNullOrEmpty(displayName) && connector.DisplayName != displayName)
                {
                    connector.DisplayName = string.IsNullOrEmpty(displayName) ? "" : displayName;
                }

                bool validated = false;
                if (connector != null)
                {
                    var configs = GetConfigAsDictionary(postObject.configurations);
                    string customErrorMessage;
                    var saved = connector.SaveConfig(PortalSettings.PortalId, configs, ref validated,
                        out customErrorMessage);

                    if (!saved)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = false,
                            Validated = validated,
                            Message = string.IsNullOrEmpty(customErrorMessage)
                                ? Localization.GetString("ErrSavingConnectorSettings.Text", Constants.SharedResources)
                                : customErrorMessage
                        });
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Success = true,
                        Validated = validated,
                        connector?.Id
                    });
            }
            catch (Exception ex)
            {
                if (ex is ConnectorArgumentException)
                {
                    Logger.Warn(ex);
                }
                else
                {
                    Logger.Error(ex);
                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new {Success = false, Message = ex.Message});
            }
        }

        /// <summary>
        /// Delete a connector. Supported only for connectors with SupportsMultiple=true.
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteConnection(object postData)
        {
            try
            {
                var jsonData = DotNetNuke.Common.Utilities.Json.Serialize(postData);
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic postObject = serializer.Deserialize(jsonData, typeof(object));

                var name = postObject.name;
                var id = postObject.id;
                var connectors =
                    GetConnections(PortalId).ForEach(x =>
                    {
                        {
                            x.HasConfig(PortalSettings.PortalId);
                        }
                    })
                        .Where(
                            c =>
                                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

                var connector =
                    connectors.FirstOrDefault(c => c.SupportsMultiple && c.Id == id);
                if (connector != null)
                {
                    connector.DeleteConnector(PortalId);
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new
                        {
                            Success = true
                        });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Success = true,
                        Message =
                            Localization.GetString("ErrConnectorNotFound.Text", Components.Constants.LocalResourcesFile)
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage GetConnectionLocalizedString(string name, string culture)
        {
            var connection = GetConnections(PortalId).ForEach(x =>
            {
                {
                    x.HasConfig(PortalSettings.PortalId);
                }
            })
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (connection == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var resourceFile = string.Format("{0}/App_LocalResources/SharedResources.resx", connection.PluginFolder);
            IDictionary<string, string> localizedStrings;
            try
            {
                localizedStrings = Library.Controllers.LocalizationController.Instance.GetLocalizedDictionary(resourceFile, culture);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                localizedStrings = new Dictionary<string, string>();
            }

            return Request.CreateResponse(HttpStatusCode.OK, localizedStrings);
        }

        #endregion

        #region Public Methods
        private IList<IConnector> GetConnections(int portalId)
        {
            var connectors = ConnectionsManager.Instance.GetConnectors();
            var allConnectors = new List<IConnector>();
            foreach (var con in connectors)
            {
                allConnectors.AddRange(con.GetConnectors(portalId));
            }
            return allConnectors;
        }

        private IDictionary<string, string> GetConfigAsDictionary(dynamic configurations)
        {
            var configs = new Dictionary<string, string>();

            for (var i = 0; i < configurations.Count; i++)
            {
                var c = configurations[i];
                if (!configs.ContainsKey(c.name))
                {
                    configs.Add(c.name, c.value);
                }
                else
                {
                    configs[c.name] = c.value;
                }
            }

            return configs;
        }

        #endregion    }
    }
}
