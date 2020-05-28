// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Entities.Portals
{
    [Serializable]
    public class PortalGroupInfo : BaseEntityInfo, IHydratable
    {

        public int PortalGroupId { get; set; }

        public string AuthenticationDomain { get; set; }

        public int MasterPortalId { get; set; }

        public string MasterPortalName
        {
            get
            {
                string portalName = String.Empty;
                if (MasterPortalId > -1)
                {
                    var portal = PortalController.Instance.GetPortal(MasterPortalId);
                    if (portal != null)
                    {
                        portalName = portal.PortalName;
                    }
                }
                return portalName;
            }
        }

        [Required()]
        public string PortalGroupDescription { get; set; }

        [Required()]
        public string PortalGroupName { get; set; }

        #region IHydratable Members

        public int KeyID
        {
            get
            {
                return PortalGroupId;
            }
            set
            {
                PortalGroupId = value;
            }
        }

        public void Fill(IDataReader dr)
        {
            FillInternal(dr);

            PortalGroupId = Null.SetNullInteger(dr["PortalGroupID"]);
            PortalGroupName = Null.SetNullString(dr["PortalGroupName"]);
            PortalGroupDescription = Null.SetNullString(dr["PortalGroupDescription"]);
            MasterPortalId = Null.SetNullInteger(dr["MasterPortalID"]);
            AuthenticationDomain = Null.SetNullString(dr["AuthenticationDomain"]);
        }

        #endregion
    }
}
