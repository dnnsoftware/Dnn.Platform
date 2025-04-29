// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy;

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

/// <summary>Class of ScopeType.</summary>
/// <seealso cref="TermController"/>
[Serializable]
public class ScopeType : ScopeTypeMemberNameFixer, IHydratable
{
    /// <summary>Initializes a new instance of the <see cref="ScopeType"/> class.</summary>
    public ScopeType()
        : this(Null.NullString)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ScopeType"/> class.</summary>
    /// <param name="scopeType"></param>
    public ScopeType(string scopeType)
    {
        this.ScopeTypeId = Null.NullInteger;
        this.ScopeType = scopeType;
    }

    public int ScopeTypeId { get; set; }

    /// <inheritdoc/>
    public int KeyID
    {
        get
        {
            return this.ScopeTypeId;
        }

        set
        {
            this.ScopeTypeId = value;
        }
    }

    /// <inheritdoc/>
    public void Fill(IDataReader dr)
    {
        this.ScopeTypeId = Null.SetNullInteger(dr["ScopeTypeID"]);
        this.ScopeType = Null.SetNullString(dr["ScopeType"]);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.ScopeType;
    }
}
