// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.Modules.DynamicContentManager.Components.Entities;
using DotNetNuke.Entities.Portals;

namespace Dnn.Modules.DynamicContentManager.Components
{
    public interface ISettingsManager
    {
        DCCSettings Get(int portalId, int moduleId);
        void Save(DCCSettings setting, PortalSettings portalSettings, int moduleId);
    }
}
