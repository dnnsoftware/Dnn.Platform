#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Tokens;

#endregion

namespace DotNetNuke.Security.Roles
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Roles
    /// Class:      RoleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The RoleInfo class provides the Entity Layer Role object
    /// </summary>
    /// <history>
    ///     [cnurse]    05/23/2005  made compatible with .NET 2.0
    ///     [cnurse]    01/03/2006  added RoleGroupId property
    /// </history>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class RoleInfo : BaseEntityInfo, IHydratable, IXmlSerializable, IPropertyAccess
    {
        private RoleType _RoleType = RoleType.None;
        private bool _RoleTypeSet = Null.NullBoolean;
        private Dictionary<string, string> _settings;

        public RoleInfo()
        {
            TrialFrequency = "N";
            BillingFrequency = "N";
            RoleID = Null.NullInteger;
            IsSystemRole = false;
        }

        #region Public Properties
        /// <summary>
        /// Gets whether this role is a system role
        /// </summary>
        /// <value>A boolean representing whether this is a system role such as Administrators, Registered Users etc.</value>
        public bool IsSystemRole { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether users are automatically assigned to the role
        /// </summary>
        /// <value>A boolean (True/False)</value>
        /// -----------------------------------------------------------------------------
        public bool AutoAssignment { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Billing Frequency for the role
        /// </summary>
        /// <value>A String representing the Billing Frequency of the Role<br/>
        /// <ul>
        /// <list>N - None</list>
        /// <list>O - One time fee</list>
        /// <list>D - Daily</list>
        /// <list>W - Weekly</list>
        /// <list>M - Monthly</list>
        /// <list>Y - Yearly</list>
        /// </ul>
        /// </value>
        /// -----------------------------------------------------------------------------
        public string BillingFrequency { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the length of the billing period
        /// </summary>
        /// <value>An integer representing the length of the billing period</value>
        /// -----------------------------------------------------------------------------
        public int BillingPeriod { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an sets the Description of the Role
        /// </summary>
        /// <value>A string representing the description of the role</value>
        /// -----------------------------------------------------------------------------
        public string Description { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Icon File for the role
        /// </summary>
        /// <value>A string representing the Icon File for the role</value>
        /// -----------------------------------------------------------------------------
        public string IconFile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the role is public
        /// </summary>
        /// <value>A boolean (True/False)</value>
        /// -----------------------------------------------------------------------------
        public bool IsPublic { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Portal Id for the Role
        /// </summary>
        /// <value>An Integer representing the Id of the Portal</value>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int PortalID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Role Id
        /// </summary>
        /// <value>An Integer representing the Id of the Role</value>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int RoleID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the RoleGroup Id
        /// </summary>
        /// <value>An Integer representing the Id of the RoleGroup</value>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int RoleGroupID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Role Name
        /// </summary>
        /// <value>A string representing the name of the role</value>
        /// -----------------------------------------------------------------------------
        public string RoleName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Role Type
        /// </summary>
        /// <value>A enum representing the type of the role</value>
        /// -----------------------------------------------------------------------------
        public RoleType RoleType
        {
            get
            {
                if (!_RoleTypeSet)
                {
                    GetRoleType();
                    _RoleTypeSet = true;
                }
                return _RoleType;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the RSVP Code for the role
        /// </summary>
        /// <value>A string representing the RSVP Code for the role</value>
        /// -----------------------------------------------------------------------------
        public string RSVPCode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the role is a security role and can be used in Permission
        /// Grids etc.
        /// </summary>
        /// <value>A SecurityMode enum</value>
        /// -----------------------------------------------------------------------------
        public SecurityMode SecurityMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the fee for the role
        /// </summary>
        /// <value>A single number representing the fee for the role</value>
        /// -----------------------------------------------------------------------------
        public float ServiceFee { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the role settings
        /// </summary>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public Dictionary<string, string> Settings
        {
            get
            {
                return _settings ?? (_settings = (RoleID == Null.NullInteger)
                                                     ? new Dictionary<string, string>()
                                                     : TestableRoleController.Instance.GetRoleSettings(RoleID) as
                                                       Dictionary<string, string>);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the status for the role
        /// </summary>
        /// <value>An enumerated value Pending, Disabled, Approved</value>
        /// -----------------------------------------------------------------------------
        public RoleStatus Status { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the trial fee for the role
        /// </summary>
        /// <value>A single number representing the trial fee for the role</value>
        /// -----------------------------------------------------------------------------
        public float TrialFee { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Trial Frequency for the role
        /// </summary>
        /// <value>A String representing the Trial Frequency of the Role<br/>
        /// <ul>
        /// <list>N - None</list>
        /// <list>O - One time fee</list>
        /// <list>D - Daily</list>
        /// <list>W - Weekly</list>
        /// <list>M - Monthly</list>
        /// <list>Y - Yearly</list>
        /// </ul>
        /// </value>
        /// -----------------------------------------------------------------------------
        public string TrialFrequency { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the length of the trial period
        /// </summary>
        /// <value>An integer representing the length of the trial period</value>
        /// -----------------------------------------------------------------------------
        public int TrialPeriod { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the number of users in the role
        /// </summary>
        /// <value>An integer representing the number of users</value>
        /// -----------------------------------------------------------------------------
        public int UserCount { get; private set; }

        public string PhotoURL
        {
            get
            {
                string photoURL = Globals.ApplicationPath + "/images/sample-group-profile.jpg";

                if ((IconFile != null))
                {
                    if (!string.IsNullOrEmpty(IconFile))
                    {
                        IFileInfo fileInfo =
                            FileManager.Instance.GetFile(int.Parse(IconFile.Replace("FileID=", string.Empty)));
                        if ((fileInfo != null))
                        {
                            photoURL = FileManager.Instance.GetUrl(fileInfo);
                        }
                    }
                }
                return photoURL;
            }
        }

        #endregion

        #region Private Methods

        private void GetRoleType()
        {
            PortalInfo portal = new PortalController().GetPortal(PortalID);
            if (RoleID == portal.AdministratorRoleId)
            {
                _RoleType = RoleType.Administrator;
            }
            else if (RoleID == portal.RegisteredRoleId)
            {
                _RoleType = RoleType.RegisteredUser;
            }
            else if (RoleName == "Subscribers")
            {
                _RoleType = RoleType.Subscriber;
            }
            else if (RoleName == "Unverified Users")
            {
                _RoleType = RoleType.UnverifiedUser;
            }
        }

        #endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a RoleInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// <history>
        /// 	[cnurse]	03/17/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual void Fill(IDataReader dr)
        {
            RoleID = Null.SetNullInteger(dr["RoleId"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            RoleGroupID = Null.SetNullInteger(dr["RoleGroupId"]);
            RoleName = Null.SetNullString(dr["RoleName"]);
            Description = Null.SetNullString(dr["Description"]);
            ServiceFee = Null.SetNullSingle(dr["ServiceFee"]);
            BillingPeriod = Null.SetNullInteger(dr["BillingPeriod"]);
            BillingFrequency = Null.SetNullString(dr["BillingFrequency"]);
            TrialFee = Null.SetNullSingle(dr["TrialFee"]);
            TrialPeriod = Null.SetNullInteger(dr["TrialPeriod"]);
            TrialFrequency = Null.SetNullString(dr["TrialFrequency"]);
            IsPublic = Null.SetNullBoolean(dr["IsPublic"]);
            AutoAssignment = Null.SetNullBoolean(dr["AutoAssignment"]);
            RSVPCode = Null.SetNullString(dr["RSVPCode"]);
            IconFile = Null.SetNullString(dr["IconFile"]);
            
            //New properties may not be present if called before 6.2 Upgrade has been executed
            try
            {
                int mode = Null.SetNullInteger(dr["SecurityMode"]);
                switch (mode)
                {
                    case 0:
                        SecurityMode = SecurityMode.SecurityRole;
                        break;
                    case 1:
                        SecurityMode = SecurityMode.SocialGroup;
                        break;
                    default:
                        SecurityMode = SecurityMode.Both;
                        break;
                }


                int status = Null.SetNullInteger(dr["Status"]);
                switch (status)
                {
                    case -1:
                        Status = RoleStatus.Pending;
                        break;
                    case 0:
                        Status = RoleStatus.Disabled;
                        break;
                    default:
                        Status = RoleStatus.Approved;
                        break;
                }
                //check for values only relevant to UserRoles
                var schema = dr.GetSchemaTable();
                if (schema != null)
                {
                    if (schema.Select("ColumnName = 'UserCount'").Length > 0)
                    {
                        UserCount = Null.SetNullInteger(dr["UserCount"]);
                    }
                    if (schema.Select("ColumnName = 'IsSystemRole'").Length > 0)
                    {
                        IsSystemRole = Null.SetNullBoolean(dr["IsSystemRole"]);
                    }
                }
                
               
            }
            catch (IndexOutOfRangeException)
            {
                //do nothing
            }


            //Fill base class fields
            FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// <history>
        /// 	[cnurse]	03/17/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual int KeyID
        {
            get { return RoleID; }
            set { RoleID = value; }
        }

        #endregion

        #region IPropertyAccess Members

        public CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser,
                                  Scope accessLevel, ref bool propertyNotFound)
        {
            string OutputFormat = string.Empty;
            if (format == string.Empty)
            {
                OutputFormat = "g";
            }
            else
            {
                OutputFormat = format;
            }
            string propName = propertyName.ToLowerInvariant();
            switch (propName)
            {
                case "roleid":
                    return PropertyAccess.FormatString(RoleID.ToString(), format);
                case "groupid":
                    return PropertyAccess.FormatString(RoleID.ToString(), format);
                case "status":
                    return PropertyAccess.FormatString(Status.ToString(), format);
                case "groupname":
                    return PropertyAccess.FormatString(RoleName, format);
                case "rolename":
                    return PropertyAccess.FormatString(RoleName, format);
                case "groupdescription":
                    return PropertyAccess.FormatString(Description, format);
                case "description":
                    return PropertyAccess.FormatString(Description, format);
                case "usercount":
                    return PropertyAccess.FormatString(UserCount.ToString(), format);
                case "street":
                    return PropertyAccess.FormatString(GetString("Street", string.Empty), format);
                case "city":
                    return PropertyAccess.FormatString(GetString("City", string.Empty), format);
                case "region":
                    return PropertyAccess.FormatString(GetString("Region", string.Empty), format);
                case "country":
                    return PropertyAccess.FormatString(GetString("Country", string.Empty), format);
                case "postalcode":
                    return PropertyAccess.FormatString(GetString("PostalCode", string.Empty), format);
                case "website":
                    return PropertyAccess.FormatString(GetString("Website", string.Empty), format);
                case "datecreated":
                    return PropertyAccess.FormatString(CreatedOnDate.ToString(), format);
                case "photourl":
                    return PropertyAccess.FormatString(PhotoURL, format);
                case "stat_status":
                    return PropertyAccess.FormatString(GetString("stat_status", string.Empty), format);
                case "stat_photo":
                    return PropertyAccess.FormatString(GetString("stat_photo", string.Empty), format);
                case "stat_file":
                    return PropertyAccess.FormatString(GetString("stat_file", string.Empty), format);
                case "url":
                    return PropertyAccess.FormatString(GetString("URL", string.Empty), format);
                case "issystemrole":
                    return PropertyAccess.Boolean2LocalizedYesNo(IsSystemRole, formatProvider);
                case "grouptype":
                    return IsPublic ? "Public.Text" : "Private.Text";
                case "groupcreatorname":
                    return PropertyAccess.FormatString(GetString("GroupCreatorName", string.Empty), format);
                default:
                    if (Settings.ContainsKey(propertyName))
                    {
                        return PropertyAccess.FormatString(GetString(propertyName, string.Empty), format);
                    }

                    propertyNotFound = true;
                    return string.Empty;
            }

            
            
        }

        #endregion

        #region IXmlSerializable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the RoleInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a RoleInfo from an XmlReader
        /// </summary>
        /// <param name="reader">The XmlReader to use</param>
        /// -----------------------------------------------------------------------------
        public void ReadXml(XmlReader reader)
        {
            //Set status to approved by default
            Status = RoleStatus.Approved;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if (reader.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name.ToLowerInvariant())
                    {
                        case "rolename":
                            RoleName = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            Description = reader.ReadElementContentAsString();
                            break;
                        case "billingfrequency":
                            BillingFrequency = reader.ReadElementContentAsString();
                            if (string.IsNullOrEmpty(BillingFrequency))
                            {
                                BillingFrequency = "N";
                            }
                            break;
                        case "billingperiod":
                            BillingPeriod = reader.ReadElementContentAsInt();
                            break;
                        case "servicefee":
                            ServiceFee = reader.ReadElementContentAsFloat();
                            if (ServiceFee < 0)
                            {
                                ServiceFee = 0;
                            }
                            break;
                        case "trialfrequency":
                            TrialFrequency = reader.ReadElementContentAsString();
                            if (string.IsNullOrEmpty(TrialFrequency))
                            {
                                TrialFrequency = "N";
                            }
                            break;
                        case "trialperiod":
                            TrialPeriod = reader.ReadElementContentAsInt();
                            break;
                        case "trialfee":
                            TrialFee = reader.ReadElementContentAsFloat();
                            if (TrialFee < 0)
                            {
                                TrialFee = 0;
                            }
                            break;
                        case "ispublic":
                            IsPublic = reader.ReadElementContentAsBoolean();
                            break;
                        case "autoassignment":
                            AutoAssignment = reader.ReadElementContentAsBoolean();
                            break;
                        case "rsvpcode":
                            RSVPCode = reader.ReadElementContentAsString();
                            break;
                        case "iconfile":
                            IconFile = reader.ReadElementContentAsString();
                            break;
                        case "issystemrole":
                            IsSystemRole = reader.ReadElementContentAsBoolean();
                            break;
                        case "roletype":
                            switch (reader.ReadElementContentAsString())
                            {
                                case "adminrole":
                                    _RoleType = RoleType.Administrator;
                                    break;
                                case "registeredrole":
                                    _RoleType = RoleType.RegisteredUser;
                                    break;
                                case "subscriberrole":
                                    _RoleType = RoleType.Subscriber;
                                    break;
                                case "unverifiedrole":
                                    _RoleType = RoleType.UnverifiedUser;
                                    break;
                                default:
                                    _RoleType = RoleType.None;
                                    break;
                            }
                            _RoleTypeSet = true;
                            break;
                        case "securitymode":
                            switch (reader.ReadElementContentAsString())
                            {
                                case "securityrole":
                                    SecurityMode = SecurityMode.SecurityRole;
                                    break;
                                case "socialgroup":
                                    SecurityMode = SecurityMode.SocialGroup;
                                    break;
                                case "both":
                                    SecurityMode = SecurityMode.Both;
                                    break;
                            }                            
                            break;
                        case "status":
                            switch (reader.ReadElementContentAsString())
                            {
                                case "pending":
                                    Status = RoleStatus.Pending;
                                    break;
                                case "disabled":
                                    Status = RoleStatus.Disabled;
                                    break;
                                default:
                                    Status = RoleStatus.Approved;
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a RoleInfo to an XmlWriter
        /// </summary>
        /// <param name="writer">The XmlWriter to use</param>
        /// <history>
        /// 	[cnurse]	03/14/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst
            writer.WriteStartElement("role");

            //write out properties
            writer.WriteElementString("rolename", RoleName);
            writer.WriteElementString("description", Description);
            writer.WriteElementString("billingfrequency", BillingFrequency);
            writer.WriteElementString("billingperiod", BillingPeriod.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("servicefee", ServiceFee.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("trialfrequency", TrialFrequency);
            writer.WriteElementString("trialperiod", TrialPeriod.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("trialfee", TrialFee.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ispublic", IsPublic.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            writer.WriteElementString("autoassignment", AutoAssignment.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            writer.WriteElementString("rsvpcode", RSVPCode);
            writer.WriteElementString("iconfile", IconFile);
            writer.WriteElementString("issystemrole", IsSystemRole.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            switch (RoleType)
            {
                case RoleType.Administrator:
                    writer.WriteElementString("roletype", "adminrole");
                    break;
                case RoleType.RegisteredUser:
                    writer.WriteElementString("roletype", "registeredrole");
                    break;
                case RoleType.Subscriber:
                    writer.WriteElementString("roletype", "subscriberrole");
                    break;
                case RoleType.UnverifiedUser:
                    writer.WriteElementString("roletype", "unverifiedrole");
                    break;
                case RoleType.None:
                    writer.WriteElementString("roletype", "none");
                    break;
            }

            switch (SecurityMode)
            {
                case SecurityMode.SecurityRole:
                    writer.WriteElementString("securitymode", "securityrole");
                    break;
                case SecurityMode.SocialGroup:
                    writer.WriteElementString("securitymode", "socialgroup");
                    break;
                case SecurityMode.Both:
                    writer.WriteElementString("securitymode", "both");
                    break;
            }
            switch (Status)
            {
                case RoleStatus.Pending:
                    writer.WriteElementString("status", "pending");
                    break;
                case RoleStatus.Disabled:
                    writer.WriteElementString("status", "disabled");
                    break;
                case RoleStatus.Approved:
                    writer.WriteElementString("status", "approved");
                    break;
            }

            //Write end of main element
            writer.WriteEndElement();
        }

        #endregion

        private string GetString(string keyName, string defaultValue)
        {
            if (Settings == null)
            {
                return defaultValue;
            }
            if (Settings.ContainsKey(keyName))
            {
                return Settings[keyName];
            }

            return defaultValue;
        }
    }
}