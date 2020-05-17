﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Security.Roles
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Roles
    /// Class:      RoleGroupInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The RoleGroupInfo class provides the Entity Layer RoleGroup object
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class RoleGroupInfo : BaseEntityInfo, IHydratable, IXmlSerializable
    {
		#region "Private Members"
		
        private string _Description;
        private int _PortalID = Null.NullInteger;
        private int _RoleGroupID = Null.NullInteger;
        private string _RoleGroupName;
        private Dictionary<string, RoleInfo> _Roles;
		
		#endregion
		
		#region "Constructors"

        public RoleGroupInfo()
        {
        }

        public RoleGroupInfo(int roleGroupID, int portalID, bool loadRoles)
        {
            _PortalID = portalID;
            _RoleGroupID = roleGroupID;
            if (loadRoles)
            {
                GetRoles();
            }
        }
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the RoleGroup Id
        /// </summary>
        /// <value>An Integer representing the Id of the RoleGroup</value>
        /// -----------------------------------------------------------------------------
        public int RoleGroupID
        {
            get
            {
                return _RoleGroupID;
            }
            set
            {
                _RoleGroupID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Portal Id for the RoleGroup
        /// </summary>
        /// <value>An Integer representing the Id of the Portal</value>
        /// -----------------------------------------------------------------------------
        public int PortalID
        {
            get
            {
                return _PortalID;
            }
            set
            {
                _PortalID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the RoleGroup Name
        /// </summary>
        /// <value>A string representing the Name of the RoleGroup</value>
        /// -----------------------------------------------------------------------------
        public string RoleGroupName
        {
            get
            {
                return _RoleGroupName;
            }
            set
            {
                _RoleGroupName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an sets the Description of the RoleGroup
        /// </summary>
        /// <value>A string representing the description of the RoleGroup</value>
        /// -----------------------------------------------------------------------------
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Roles for this Role Group
        /// </summary>
        /// <returns>A Boolean</returns>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, RoleInfo> Roles
        {
            get
            {
                if (_Roles == null && RoleGroupID > Null.NullInteger)
                {
                    GetRoles();
                }
                return _Roles;
            }
        }
		
		#endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a RoleGroupInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            RoleGroupID = Null.SetNullInteger(dr["RoleGroupId"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            RoleGroupName = Null.SetNullString(dr["RoleGroupName"]);
            Description = Null.SetNullString(dr["Description"]);

            //Fill base class fields
            FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int KeyID
        {
            get
            {
                return RoleGroupID;
            }
            set
            {
                RoleGroupID = value;
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the RoleGroupInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a RoleGroupInfo from an XmlReader
        /// </summary>
        /// <param name="reader">The XmlReader to use</param>
        /// -----------------------------------------------------------------------------
        public void ReadXml(XmlReader reader)
        {
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
                        case "rolegroup":
                            break;
                        case "roles":
                            if (!reader.IsEmptyElement)
                            {
                                ReadRoles(reader);
                            }
                            break;
                        case "rolegroupname":
                            RoleGroupName = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            Description = reader.ReadElementContentAsString();
                            break;
                        default:
                            if(reader.NodeType == XmlNodeType.Element && !String.IsNullOrEmpty(reader.Name))
                            {
                                reader.ReadElementContentAsString();
                            }
                            break;
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a RoleGroupInfo to an XmlWriter
        /// </summary>
        /// <param name="writer">The XmlWriter to use</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst
            writer.WriteStartElement("rolegroup");

            //write out properties
            writer.WriteElementString("rolegroupname", RoleGroupName);
            writer.WriteElementString("description", Description);

            //Write start of roles
            writer.WriteStartElement("roles");
			
			//Iterate through roles
            if (Roles != null)
            {
                foreach (RoleInfo role in Roles.Values)
                {
                    role.WriteXml(writer);
                }
            }
			
            //Write end of Roles
            writer.WriteEndElement();

            //Write end of main element
            writer.WriteEndElement();
        }

        #endregion

        private void GetRoles()
        {
            _Roles = new Dictionary<string, RoleInfo>();
            foreach (var role in RoleController.Instance.GetRoles(PortalID, r => r.RoleGroupID == RoleGroupID))
            {
                _Roles[role.RoleName] = role;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a Roles from an XmlReader
        /// </summary>
        /// <param name="reader">The XmlReader to use</param>
        /// -----------------------------------------------------------------------------
        private void ReadRoles(XmlReader reader)
        {
            reader.ReadStartElement("roles");
            _Roles = new Dictionary<string, RoleInfo>();
            do
            {
                reader.ReadStartElement("role");
                var role = new RoleInfo();
                role.ReadXml(reader);
                _Roles.Add(role.RoleName, role);
            } while (reader.ReadToNextSibling("role"));
        }
    }
}
