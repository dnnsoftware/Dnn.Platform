// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteGroups
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    using PortalGroupInfo = Dnn.PersonaBar.SiteGroups.Models.PortalGroupInfo;
    using PortalInfo = Dnn.PersonaBar.SiteGroups.Models.PortalInfo;

    public class PortalGroupAdapter : IManagePortalGroups
    {
        IPortalGroupController PortalGroupController
        {
            get { return DotNetNuke.Entities.Portals.PortalGroupController.Instance; }
        }

        public IEnumerable<PortalGroupInfo> SiteGroups()
        {
            return this.PortalGroupController.GetPortalGroups().Select(g => new PortalGroupInfo
            {
                AuthenticationDomain = g.AuthenticationDomain,
                PortalGroupId = g.PortalGroupId,
                Description = g.PortalGroupDescription,
                MasterPortal = new PortalInfo
                {
                    PortalName = g.MasterPortalName,
                    PortalId = g.MasterPortalId
                },
                PortalGroupName = g.PortalGroupName,
                Portals = this.PortalsOfGroup(g.PortalGroupId, g.MasterPortalId)
                        .Select(p => new PortalInfo
                        {
                            PortalId = p.PortalID,
                            PortalName = p.PortalName
                        })

            });
        }

        public IEnumerable<PortalInfo> AvailablePortals()
        {
            return new PortalController().GetPortals()
                        .Cast<DotNetNuke.Entities.Portals.PortalInfo>()
                        .Where(x => x.PortalGroupID == Null.NullInteger)
                        .Select(p => new PortalInfo
                        {
                            PortalId = p.PortalID,
                            PortalName = p.PortalName
                        });
        }

        public int Save(PortalGroupInfo portalGroup)
        {
            if (portalGroup.PortalGroupId == -1)
            {
                return this.AddPortalGroup(portalGroup);
            }
            else
            {
                return this.UpdatePortalGroup(portalGroup);
            }
        }

        public void Delete(int portalGroupId)
        {
            var group = this.PortalGroupController.GetPortalGroups().Single(g => g.PortalGroupId == portalGroupId);
            this.PortalGroupController.DeletePortalGroup(group);
        }

        IEnumerable<DotNetNuke.Entities.Portals.PortalInfo> PortalsOfGroup(int groupId, int masterPortalId)
        {
            return this.PortalGroupController
                .GetPortalsByGroup(groupId)
                .Where(x => x.PortalID != masterPortalId);
        }

        int UpdatePortalGroup(PortalGroupInfo portalGroup)
        {
            UserCopiedCallback callback = delegate { };
            var @group = this.PortalGroupController.GetPortalGroups().Single(g => g.PortalGroupId == portalGroup.PortalGroupId);
            @group.PortalGroupName = portalGroup.PortalGroupName;
            @group.AuthenticationDomain = portalGroup.AuthenticationDomain;
            @group.PortalGroupDescription = portalGroup.Description;
            this.PortalGroupController.UpdatePortalGroup(@group);
            var currentPortals = this.PortalsOfGroup(portalGroup.PortalGroupId, portalGroup.MasterPortal.PortalId).ToList();
            foreach (var portal in currentPortals)
            {
                if (portalGroup.Portals == null || portalGroup.Portals.All(p => p.PortalId != portal.PortalID))
                    this.PortalGroupController.RemovePortalFromGroup(portal, @group, false, callback);
            }

            if (portalGroup.Portals != null)
                foreach (var portal in portalGroup.Portals)
                {
                    if (currentPortals.All(p => p.PortalID != portal.PortalId))
                    {
                        var p = new PortalController().GetPortal(portal.PortalId);
                        this.PortalGroupController.AddPortalToGroup(p, @group, callback);
                    }
                }
            return @group.PortalGroupId;
        }

        int AddPortalGroup(PortalGroupInfo portalGroup)
        {
            UserCopiedCallback callback = delegate { };
            var group = new DotNetNuke.Entities.Portals.PortalGroupInfo
            {
                AuthenticationDomain = portalGroup.AuthenticationDomain,
                MasterPortalId = portalGroup.MasterPortal.PortalId,
                PortalGroupDescription = portalGroup.Description,
                PortalGroupName = portalGroup.PortalGroupName
            };
            this.PortalGroupController.AddPortalGroup(@group);
            if (portalGroup.Portals != null)
            {
                foreach (var portal in portalGroup.Portals)
                {
                    var p = new PortalController().GetPortal(portal.PortalId);
                    this.PortalGroupController.AddPortalToGroup(p, @group, callback);
                }
            }
            return @group.PortalGroupId;
        }
    }
}