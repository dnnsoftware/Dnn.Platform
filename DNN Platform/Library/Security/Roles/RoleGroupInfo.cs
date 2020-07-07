// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security.Roles.Internal;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Roles
    /// Class:      RoleGroupInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The RoleGroupInfo class provides the Entity Layer RoleGroup object.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class RoleGroupInfo : BaseEntityInfo, IHydratable, IXmlSerializable
    {
        private string _Description;
        private int _PortalID = Null.NullInteger;
        private int _RoleGroupID = Null.NullInteger;
        private string _RoleGroupName;
        private Dictionary<string, RoleInfo> _Roles;

        public RoleGroupInfo()
        {
        }

        public RoleGroupInfo(int roleGroupID, int portalID, bool loadRoles)
        {
            this._PortalID = portalID;
            this._RoleGroupID = roleGroupID;
            if (loadRoles)
            {
                this.GetRoles();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Roles for this Role Group.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, RoleInfo> Roles
        {
            get
            {
                if (this._Roles == null && this.RoleGroupID > Null.NullInteger)
                {
                    this.GetRoles();
                }

                return this._Roles;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the RoleGroup Id.
        /// </summary>
        /// <value>An Integer representing the Id of the RoleGroup.</value>
        /// -----------------------------------------------------------------------------
        public int RoleGroupID
        {
            get
            {
                return this._RoleGroupID;
            }

            set
            {
                this._RoleGroupID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Portal Id for the RoleGroup.
        /// </summary>
        /// <value>An Integer representing the Id of the Portal.</value>
        /// -----------------------------------------------------------------------------
        public int PortalID
        {
            get
            {
                return this._PortalID;
            }

            set
            {
                this._PortalID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the RoleGroup Name.
        /// </summary>
        /// <value>A string representing the Name of the RoleGroup.</value>
        /// -----------------------------------------------------------------------------
        public string RoleGroupName
        {
            get
            {
                return this._RoleGroupName;
            }

            set
            {
                this._RoleGroupName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets an sets the Description of the RoleGroup.
        /// </summary>
        /// <value>A string representing the description of the RoleGroup.</value>
        /// -----------------------------------------------------------------------------
        public string Description
        {
            get
            {
                return this._Description;
            }

            set
            {
                this._Description = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int KeyID
        {
            get
            {
                return this.RoleGroupID;
            }

            set
            {
                this.RoleGroupID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a RoleGroupInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            this.RoleGroupID = Null.SetNullInteger(dr["RoleGroupId"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.RoleGroupName = Null.SetNullString(dr["RoleGroupName"]);
            this.Description = Null.SetNullString(dr["Description"]);

            // Fill base class fields
            this.FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the RoleGroupInfo.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a RoleGroupInfo from an XmlReader.
        /// </summary>
        /// <param name="reader">The XmlReader to use.</param>
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
                                this.ReadRoles(reader);
                            }

                            break;
                        case "rolegroupname":
                            this.RoleGroupName = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            this.Description = reader.ReadElementContentAsString();
                            break;
                        default:
                            if (reader.NodeType == XmlNodeType.Element && !string.IsNullOrEmpty(reader.Name))
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
        /// Writes a RoleGroupInfo to an XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to use.</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("rolegroup");

            // write out properties
            writer.WriteElementString("rolegroupname", this.RoleGroupName);
            writer.WriteElementString("description", this.Description);

            // Write start of roles
            writer.WriteStartElement("roles");

            // Iterate through roles
            if (this.Roles != null)
            {
                foreach (RoleInfo role in this.Roles.Values)
                {
                    role.WriteXml(writer);
                }
            }

            // Write end of Roles
            writer.WriteEndElement();

            // Write end of main element
            writer.WriteEndElement();
        }

        private void GetRoles()
        {
            this._Roles = new Dictionary<string, RoleInfo>();
            foreach (var role in RoleController.Instance.GetRoles(this.PortalID, r => r.RoleGroupID == this.RoleGroupID))
            {
                this._Roles[role.RoleName] = role;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a Roles from an XmlReader.
        /// </summary>
        /// <param name="reader">The XmlReader to use.</param>
        /// -----------------------------------------------------------------------------
        private void ReadRoles(XmlReader reader)
        {
            reader.ReadStartElement("roles");
            this._Roles = new Dictionary<string, RoleInfo>();
            do
            {
                reader.ReadStartElement("role");
                var role = new RoleInfo();
                role.ReadXml(reader);
                this._Roles.Add(role.RoleName, role);
            }
            while (reader.ReadToNextSibling("role"));
        }
    }
}
