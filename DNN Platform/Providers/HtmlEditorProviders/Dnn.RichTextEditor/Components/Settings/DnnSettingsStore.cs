// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.Settings
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Entities.Portals;

    /// <summary>Stores settings in DNN's host settings and portal settings.</summary>
    public class DnnSettingsStore : ISettingsStore
    {
        private readonly IHostSettingsService hostSettingsService;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="DnnSettingsStore"/> class.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        public DnnSettingsStore(IHostSettingsService hostSettingsService, IPortalController portalController)
        {
            this.hostSettingsService = hostSettingsService;
            this.portalController = portalController;
        }

        /// <inheritdoc/>
        public string Get(int portalId, string key)
        {
            return portalId == EditorSettingsRepository.AllSites
                ? this.hostSettingsService.GetString(key, null)
                : PortalController.GetPortalSetting(this.portalController, key, portalId, null);
        }

        /// <inheritdoc/>
        public void Set(int portalId, string key, string value)
        {
            if (portalId == EditorSettingsRepository.AllSites)
            {
                this.hostSettingsService.Update(key, value ?? string.Empty);
            }
            else
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, key, value ?? string.Empty);
            }
        }
    }
}
