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