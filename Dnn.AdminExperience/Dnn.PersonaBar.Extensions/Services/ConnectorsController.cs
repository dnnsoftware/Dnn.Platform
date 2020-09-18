// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Connectors.Services
{
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

    [MenuPermission(MenuName = "Dnn.Connectors")]
    public class ConnectorsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConnectorsController));

        [HttpGet]
        public HttpResponseMessage GetConnections()
        {
            var connections = this.GetConnections(this.PortalId)
                .ForEach(x =>
                {
                    {
                        x.HasConfig(this.PortalSettings.PortalId);
                    }
                })
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    type = c.Type,
                    displayName = c.DisplayName,
                    connected = c.HasConfig(this.PortalSettings.PortalId),
                    icon = Globals.ResolveUrl(c.IconUrl),
                    pluginFolder = Globals.ResolveUrl(c.PluginFolder),
                    configurations = c.GetConfig(this.PortalSettings.PortalId),
                    supportsMultiple = c.SupportsMultiple
                }).OrderBy(connection => connection.name).ToList();
            return this.Request.CreateResponse(HttpStatusCode.OK, connections);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveConnection(object postData)
        {
            try
            {
                var jsonData = DotNetNuke.Common.Utilities.Json.Serialize(postData);
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

                dynamic postObject = serializer.Deserialize(jsonData, typeof(object));

                var name = postObject.name;
                var displayName = postObject.displayName;
                var id = postObject.id;
                var connectors =
                    this.GetConnections(this.PortalId).ForEach(x =>
                    {
                        {
                            x.HasConfig(this.PortalSettings.PortalId);
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
                    var configs = this.GetConfigAsDictionary(postObject.configurations);
                    string customErrorMessage;
                    var saved = connector.SaveConfig(this.PortalSettings.PortalId, configs, ref validated,
                        out customErrorMessage);

                    if (!saved)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = false,
                            Validated = validated,
                            Message = string.IsNullOrEmpty(customErrorMessage)
                                ? Localization.GetString("ErrSavingConnectorSettings.Text", Constants.SharedResources)
                                : customErrorMessage
                        });
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK,
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
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { Success = false, Message = ex.Message });
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
                    this.GetConnections(this.PortalId).ForEach(x =>
                    {
                        {
                            x.HasConfig(this.PortalSettings.PortalId);
                        }
                    })
                        .Where(
                            c =>
                                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

                var connector =
                    connectors.FirstOrDefault(c => c.SupportsMultiple && c.Id == id);
                if (connector != null)
                {
                    connector.DeleteConnector(this.PortalId);
                    return this.Request.CreateResponse(HttpStatusCode.OK,
                        new
                        {
                            Success = true
                        });
                }
                return this.Request.CreateResponse(HttpStatusCode.OK,
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
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage GetConnectionLocalizedString(string name, string culture)
        {
            var connection = this.GetConnections(this.PortalId).ForEach(x =>
            {
                {
                    x.HasConfig(this.PortalSettings.PortalId);
                }
            })
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (connection == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
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

            return this.Request.CreateResponse(HttpStatusCode.OK, localizedStrings);
        }

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
    }
}
