// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// This class exists solely to maintain compatibility between the original VB version
    /// which supported ScopeType.ScopeType and the c# version which doesn't allow members with
    /// the same naem as their parent type.
    /// </summary>
    [Serializable]
    public abstract class ScopeTypeMemberNameFixer
    {
        public string ScopeType { get; set; }
    }

    /// <summary>
    /// Class of ScopeType.
    /// </summary>
    /// <seealso cref="TermController"/>
    [Serializable]
    public class ScopeType : ScopeTypeMemberNameFixer, IHydratable
    {
        public ScopeType()
            : this(Null.NullString)
        {
        }

        public ScopeType(string scopeType)
        {
            this.ScopeTypeId = Null.NullInteger;
            this.ScopeType = scopeType;
        }

        public int ScopeTypeId { get; set; }

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

        public void Fill(IDataReader dr)
        {
            this.ScopeTypeId = Null.SetNullInteger(dr["ScopeTypeID"]);
            this.ScopeType = Null.SetNullString(dr["ScopeType"]);
        }

        public override string ToString()
        {
            return this.ScopeType;
        }
    }
}
