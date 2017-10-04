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