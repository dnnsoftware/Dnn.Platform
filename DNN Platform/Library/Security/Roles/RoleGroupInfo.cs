// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles;

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

/// <summary>The RoleGroupInfo class provides the Entity Layer RoleGroup object.</summary>
[Serializable]
public class RoleGroupInfo : BaseEntityInfo, IHydratable, IXmlSerializable
{
    private string description;
    private int portalID = Null.NullInteger;
    private int roleGroupID = Null.NullInteger;
    private string roleGroupName;
    private Dictionary<string, RoleInfo> roles;

    /// <summary>Initializes a new instance of the <see cref="RoleGroupInfo"/> class.</summary>
    public RoleGroupInfo()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RoleGroupInfo"/> class.</summary>
    /// <param name="roleGroupID"></param>
    /// <param name="portalID"></param>
    /// <param name="loadRoles"></param>
    public RoleGroupInfo(int roleGroupID, int portalID, bool loadRoles)
    {
        this.portalID = portalID;
        this.roleGroupID = roleGroupID;
        if (loadRoles)
        {
            this.GetRoles();
        }
    }

    /// <summary>Gets the Roles for this Role Group.</summary>
    public Dictionary<string, RoleInfo> Roles
    {
        get
        {
            if (this.roles == null && this.RoleGroupID > Null.NullInteger)
            {
                this.GetRoles();
            }

            return this.roles;
        }
    }

    /// <summary>Gets or sets the RoleGroup Id.</summary>
    public int RoleGroupID
    {
        get
        {
            return this.roleGroupID;
        }

        set
        {
            this.roleGroupID = value;
        }
    }

    /// <summary>Gets or sets the Portal Id for the RoleGroup.</summary>
    public int PortalID
    {
        get
        {
            return this.portalID;
        }

        set
        {
            this.portalID = value;
        }
    }

    /// <summary>Gets or sets the RoleGroup Name.</summary>
    public string RoleGroupName
    {
        get
        {
            return this.roleGroupName;
        }

        set
        {
            this.roleGroupName = value;
        }
    }

    /// <summary>Gets or sets the Description of the RoleGroup.</summary>
    public string Description
    {
        get
        {
            return this.description;
        }

        set
        {
            this.description = value;
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Fill(IDataReader dr)
    {
        this.RoleGroupID = Null.SetNullInteger(dr["RoleGroupId"]);
        this.PortalID = Null.SetNullInteger(dr["PortalID"]);
        this.RoleGroupName = Null.SetNullString(dr["RoleGroupName"]);
        this.Description = Null.SetNullString(dr["Description"]);

        // Fill base class fields
        this.FillInternal(dr);
    }

    /// <inheritdoc />
    public XmlSchema GetSchema()
    {
        return null;
    }

    /// <summary>Reads a RoleGroupInfo from an XmlReader.</summary>
    /// <inheritdoc />
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

    /// <summary>Writes a RoleGroupInfo to an XmlWriter.</summary>
    /// <inheritdoc />
    public void WriteXml(XmlWriter writer)
    {
        // Write start of main elements
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
        this.roles = new Dictionary<string, RoleInfo>();
        foreach (var role in RoleController.Instance.GetRoles(this.PortalID, r => r.RoleGroupID == this.RoleGroupID))
        {
            this.roles[role.RoleName] = role;
        }
    }

    /// <summary>Reads a Roles from an XmlReader.</summary>
    /// <param name="reader">The XmlReader to use.</param>
    private void ReadRoles(XmlReader reader)
    {
        reader.ReadStartElement("roles");
        this.roles = new Dictionary<string, RoleInfo>();
        do
        {
            reader.ReadStartElement("role");
            var role = new RoleInfo();
            role.ReadXml(reader);
            this.roles.Add(role.RoleName, role);
        }
        while (reader.ReadToNextSibling("role"));
    }
}
