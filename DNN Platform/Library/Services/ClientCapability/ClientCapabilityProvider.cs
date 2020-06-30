// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ClientCapability
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.ComponentModel;

    public abstract class ClientCapabilityProvider : IClientCapabilityProvider
    {
        public static IClientCapability CurrentClientCapability
        {
            get
            {
                return Instance().GetClientCapability(HttpContext.Current.Request);
            }
        }

        /// <summary>
        /// Gets a value indicating whether support detect the device whether is a tablet.
        /// </summary>
        public virtual bool SupportsTabletDetection
        {
            get
            {
                return true;
            }
        }

        public static ClientCapabilityProvider Instance()
        {
            return ComponentFactory.GetComponent<ClientCapabilityProvider>();
        }

        /// <summary>
        ///   Returns ClientCapability based on userAgent.
        /// </summary>
        /// <returns></returns>
        public abstract IClientCapability GetClientCapability(string userAgent);

        /// <summary>
        ///   Returns ClientCapability based on ClientCapabilityId.
        /// </summary>
        /// <returns></returns>
        public abstract IClientCapability GetClientCapabilityById(string clientId);

        /// <summary>
        /// Returns available Capability Values for every  Capability Name.
        /// </summary>
        /// <returns>
        /// Dictionary of Capability Name along with List of possible values of the Capability.
        /// </returns>
        /// <example>Capability Name = mobile_browser, value = Safari, Andriod Webkit. </example>
        public abstract IDictionary<string, List<string>> GetAllClientCapabilityValues();

        /// <summary>
        /// Returns All available Client Capabilities present.
        /// </summary>
        /// <returns>
        /// List of IClientCapability present.
        /// </returns>
        public abstract IQueryable<IClientCapability> GetAllClientCapabilities();

        /// <summary>
        ///   Returns ClientCapability based on HttpRequest.
        /// </summary>
        /// <returns></returns>
        public virtual IClientCapability GetClientCapability(HttpRequest httpRequest)
        {
            IClientCapability clientCapability = this.GetClientCapability(httpRequest.UserAgent);
            clientCapability.FacebookRequest = FacebookRequestController.GetFacebookDetailsFromRequest(httpRequest);

            return clientCapability;
        }
    }
}
