// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Permissions;

#endregion

namespace DotNetNuke.Entities.Modules
{
    [Serializable]
    public class PortalDesktopModuleInfo : BaseEntityInfo
    {
        private DesktopModuleInfo _desktopModule;
        private DesktopModulePermissionCollection _permissions;

        [XmlIgnore]
        public int PortalDesktopModuleID { get; set; }

        [XmlIgnore]
        public DesktopModuleInfo DesktopModule
        {
            get
            {
                if (_desktopModule == null)
                {
                    _desktopModule = DesktopModuleID > Null.NullInteger ? DesktopModuleController.GetDesktopModule(DesktopModuleID, PortalID) : new DesktopModuleInfo();
                }
                return _desktopModule;
            }
        }
        [XmlIgnore]
        public int DesktopModuleID { get; set; }

        public string FriendlyName { get; set; }

        public DesktopModulePermissionCollection Permissions
        {
            get
            {
                if (_permissions == null)
                {
                    _permissions = new DesktopModulePermissionCollection(DesktopModulePermissionController.GetDesktopModulePermissions(PortalDesktopModuleID));
                }
                return _permissions;
            }
        }

        [XmlIgnore]
        public int PortalID { get; set; }

        [XmlIgnore]
        public string PortalName { get; set; }
    }
}
