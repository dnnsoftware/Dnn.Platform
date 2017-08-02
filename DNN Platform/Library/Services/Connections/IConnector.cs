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

using System.Collections.Generic;

namespace DotNetNuke.Services.Connections
{
    public interface IConnector
    {
        /// <summary>
        /// Id of the connector. It is required if SupportsMultiple is true.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Unique name of the connector. It is used to distinguish between different types of connectors.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Category of the connector. It can be used to sort similar type resources.
        /// </summary>
        ConnectorCategories Type { get; }

        /// <summary>
        /// Determines whether this connector supports multiple connections or not.
        /// </summary>
        bool SupportsMultiple { get; }

        /// <summary>
        /// Display name of the connector.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Icon url of the connector.
        /// </summary>
        string IconUrl { get; }

        /// <summary>
        /// Plugins folder for the connector.
        /// </summary>
        string PluginFolder { get; }

        /// <summary>
        /// Determines if it is engage connector or not.
        /// </summary>
        bool IsEngageConnector { get; }

        /// <summary>
        /// Checks if the connector has been configured or not.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        bool HasConfig(int portalId);

        /// <summary>
        /// Get the connector configuration.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        IDictionary<string, string> GetConfig(int portalId);

        /// <summary>
        /// Save the connector configuration. This will work as both edit and new  if SupportsMultiple is true.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="values"></param>
        /// <param name="validated"></param>
        /// <param name="customErrorMessage"></param>
        /// <returns></returns>
        bool SaveConfig(int portalId, IDictionary<string, string> values, ref bool validated, out string customErrorMessage);

        /// <summary>
        /// Get all the connectors of a particular type.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        IEnumerable<IConnector> GetConnectors(int portalId);

        /// <summary>
        /// Delete a connector. This is used only if SupportsMultiple is true.
        /// </summary>
        /// <param name="portalId"></param>
        void DeleteConnector(int portalId);
    }
}