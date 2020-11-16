// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Groups
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Tokens;

    public class GroupInfo : RoleInfo, IPropertyAccess
    {
        // private RoleInfo roleInfo;
        public GroupInfo()
        {
        }

        public GroupInfo(RoleInfo roleInfo)
        {
            this.RoleID = roleInfo.RoleID;
            this.RoleName = roleInfo.RoleName;
            this.Description = roleInfo.Description;
            this.PortalID = roleInfo.PortalID;
            this.SecurityMode = roleInfo.SecurityMode;
            this.ServiceFee = roleInfo.ServiceFee;
            this.RSVPCode = roleInfo.RSVPCode;
        }

        // public RoleInfo Role {
        //    get {
        //        return roleInfo;
        //    }
        //    set {
        //        roleInfo = value;
        //    }
        // }
        // public int GroupId {
        //    get {
        //        return RoleID;
        //    }
        // }

        // public int ModuleId { get; set; }
        public string Street
        {
            get
            {
                return this.GetString("Street", string.Empty);
            }

            set
            {
                this.SetString("Street", value);
            }
        }

        public string City
        {
            get
            {
                return this.GetString("City", string.Empty);
            }

            set
            {
                this.SetString("City", value);
            }
        }

        public string Region
        {
            get
            {
                return this.GetString("Region", string.Empty);
            }

            set
            {
                this.SetString("Region", value);
            }
        }

        public string Country
        {
            get
            {
                return this.GetString("Country", string.Empty);
            }

            set
            {
                this.SetString("Country", value);
            }
        }

        public string PostalCode
        {
            get
            {
                return this.GetString("PostalCode", string.Empty);
            }

            set
            {
                this.SetString("PostalCode", value);
            }
        }

        public string Website
        {
            get
            {
                return this.GetString("Website", string.Empty);
            }

            set
            {
                this.SetString("Website", value);
            }
        }

        public bool Featured
        {
            get
            {
                return Convert.ToBoolean(this.GetString("Featured", "false"));
            }

            set
            {
                this.SetString("Featured", value.ToString());
            }
        }

        private string GetString(string keyName, string defaultValue)
        {
            if (this.Settings == null)
            {
                return defaultValue;
            }

            if (this.Settings.ContainsKey(keyName))
            {
                return this.Settings[keyName];
            }
            else
            {
                return defaultValue;
            }
        }

        private void SetString(string keyName, string value)
        {
            if (this.Settings.ContainsKey(keyName))
            {
                this.Settings[keyName] = value;
            }
            else
            {
                this.Settings.Add(keyName, value);
            }
        }
    }
}
