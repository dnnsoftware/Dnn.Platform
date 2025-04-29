// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host;

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

/// <summary>Represents information about an IP Filter.</summary>
[Serializable]
public class IPFilterInfo : BaseEntityInfo, IHydratable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IPFilterInfo"/> class.
    /// Create new IPFilterInfo instance.
    /// </summary>
    /// <param name="ipAddress">The IP Address.</param>
    /// <param name="subnetMask">The Subnet Mask.</param>
    /// <param name="ruleType">The Rule Type, 1 to allow, 2 to deny.</param>
    public IPFilterInfo(string ipAddress, string subnetMask, int ruleType)
    {
        this.IPAddress = ipAddress;
        this.SubnetMask = subnetMask;
        this.RuleType = ruleType;
    }

    /// <summary>Initializes a new instance of the <see cref="IPFilterInfo"/> class.</summary>
    public IPFilterInfo()
    {
        this.IPAddress = string.Empty;
        this.SubnetMask = string.Empty;
        this.RuleType = -1;
    }

    /// <summary>Gets or sets the IP of the IP filter.</summary>
    public int IPFilterID { get; set; }

    /// <summary>Gets or sets the IP Adress for this filter.</summary>
    public string IPAddress { get; set; }

    /// <summary>Gets or sets the subnet mask if this filter is for a range.</summary>
    public string SubnetMask { get; set; }

    /// <summary>Gets or sets the type of filter (1 to allow, 2 to deny).</summary>
    public int RuleType { get; set; }

    /// <summary>Gets or sets the Key ID.</summary>
    /// <returns>KeyId of the IHydratable.Key.</returns>
    /// <remarks><seealso cref="Fill"></seealso>.</remarks>
    public int KeyID
    {
        get
        {
            return this.IPFilterID;
        }

        set
        {
            this.IPFilterID = value;
        }
    }

    /// <summary>Gets or sets some notes about this IP filter.</summary>
    public string Notes { get; set; }

    /// <summary>Fills an IPFilterInfo from a Data Reader.</summary>
    /// <param name="dr">The Data Reader to use.</param>
    /// <remarks>Standard IHydratable.Fill implementation.
    /// <seealso cref="KeyID"></seealso></remarks>
    public void Fill(IDataReader dr)
    {
        this.IPFilterID = Null.SetNullInteger(dr["IPFilterID"]);

        try
        {
            this.IPFilterID = Null.SetNullInteger(dr["IPFilterID"]);
        }
        catch (IndexOutOfRangeException)
        {
            // else swallow the error
        }

        this.IPAddress = Null.SetNullString(dr["IPAddress"]);
        this.SubnetMask = Null.SetNullString(dr["SubnetMask"]);
        this.RuleType = Null.SetNullInteger(dr["RuleType"]);
        this.Notes = Null.SetNullString(dr["Notes"]);

        this.FillInternal(dr);
    }
}
