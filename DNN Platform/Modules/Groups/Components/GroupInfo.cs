// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System.Xml;
using System.Xml.Serialization;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Security.Roles;
using System.Collections;
using DotNetNuke.Security.Roles.Internal;
using System;

#endregion

namespace DotNetNuke.Entities.Groups
{
    public class GroupInfo : RoleInfo, IPropertyAccess 
    {
        //private RoleInfo roleInfo;
        public GroupInfo() {
        
        }
        public GroupInfo(RoleInfo roleInfo) {
            RoleID = roleInfo.RoleID;
            RoleName = roleInfo.RoleName;
            Description = roleInfo.Description;
            PortalID = roleInfo.PortalID;
            SecurityMode = roleInfo.SecurityMode;
            ServiceFee = roleInfo.ServiceFee;
            RSVPCode = roleInfo.RSVPCode;

          



        }
        //public RoleInfo Role {
        //    get {
        //        return roleInfo;
        //    }
        //    set {
        //        roleInfo = value;
        //    }
        //}
        //public int GroupId {
        //    get {
        //        return RoleID;
        //    }
        //}
       
        //public int ModuleId { get; set; }

           
        public string Street {
            get {
                return GetString("Street", string.Empty);
            }
            set {
                SetString("Street",value);
            }
        }
        public string City {
            get {
                return GetString("City", string.Empty);
            }
            set {
                SetString("City", value);
            }
        }

        public string Region {
            get {
                return GetString("Region", string.Empty);
            }
            set {
                SetString("Region",value);
            }
        }

        public string Country {
            get {
                return GetString("Country", string.Empty);
            }
            set {
                SetString("Country",value);
            }
        }

        public string PostalCode {
            get {
                return GetString("PostalCode", string.Empty);
            }
            set {
                SetString("PostalCode",value);
            }
        }

        public string Website {
            get {
                return GetString("Website", string.Empty);
            }
            set {
                SetString("Website",value);
            }
        }

  
        public bool Featured {
            get {
                return Convert.ToBoolean(GetString("Featured","false"));
            }
            set {
                SetString("Featured", value.ToString());
            }
        }


      
       
        private string GetString(string keyName, string defaultValue) {
            if (Settings == null) {
                return defaultValue;
            }
            if (Settings.ContainsKey(keyName)) {
                return Settings[keyName];
            } else {
                return defaultValue;
            }
        }
        private void SetString(string keyName, string value) {
            if (Settings.ContainsKey(keyName)) {
                Settings[keyName] = value;
            } else {
                Settings.Add(keyName, value);
            }
        }
       
    }
}
