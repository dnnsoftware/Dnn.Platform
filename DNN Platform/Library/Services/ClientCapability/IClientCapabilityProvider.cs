// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Services.ClientCapability
{
    public interface IClientCapabilityProvider
    {
        /// <summary>
        ///   Returns ClientCapability based on userAgent
        /// </summary>
        IClientCapability GetClientCapability(string userAgent);

        /// <summary>
        ///   Returns ClientCapability based on ClientCapabilityId
        /// </summary>
        IClientCapability GetClientCapabilityById(string clientId);

        /// <summary>
        /// Returns available Capability Values for every  Capability Name
        /// </summary>
        /// <returns>
        /// Dictionary of Capability Name along with List of possible values of the Capability
        /// </returns>
        /// <example>Capability Name = mobile_browser, value = Safari, Andriod Webkit </example>
        IDictionary<string, List<string>> GetAllClientCapabilityValues();

        /// <summary>
        /// Returns All available Client Capabilities present
        /// </summary>
        /// <returns>
        /// List of IClientCapability present
        /// </returns>        
        IQueryable<IClientCapability> GetAllClientCapabilities();

        /// <summary>
        ///   Returns ClientCapability based on HttpRequest
        /// </summary>
        IClientCapability GetClientCapability(HttpRequest httpRequest);
    }
}
