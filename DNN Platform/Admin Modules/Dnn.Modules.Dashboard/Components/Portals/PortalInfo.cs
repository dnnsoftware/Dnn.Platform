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
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;

#endregion

namespace Dnn.Modules.Dashboard.Components.Portals
{
    public class PortalInfo
    {
        private int _Pages = Null.NullInteger;
        private int _Roles = Null.NullInteger;
        private int _Users = Null.NullInteger;

        public Guid GUID { get; set; }

        public int Pages
        {
            get
            {
                if (_Pages < 0)
                {
                    _Pages = TabController.Instance.GetTabsByPortal(PortalID).Count;
                }
                return _Pages;
            }
        }

        public int PortalID { get; set; }

        public string PortalName { get; set; }

        public int Roles
        {
            get
            {
                if (_Roles < 0)
                {
                    _Roles = RoleController.Instance.GetRoles(PortalID, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).Count;
                }
                return _Roles;
            }
        }

        public int Users
        {
            get
            {
                if (_Users < 0)
                {
                    _Users = UserController.GetUserCountByPortal(PortalID);
                }
                return _Users;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst 
            writer.WriteStartElement("portal");
            writer.WriteElementString("portalName", PortalName);
            writer.WriteElementString("GUID", GUID.ToString());
            writer.WriteElementString("pages", Pages.ToString());
            writer.WriteElementString("users", Users.ToString());
            writer.WriteElementString("roles", Roles.ToString());

            //Write end of Host Info 
            writer.WriteEndElement();
        }
    }
}