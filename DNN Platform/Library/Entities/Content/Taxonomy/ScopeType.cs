#region Usings

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
    /// <summary>
    /// This class exists solely to maintain compatibility between the original VB version
    /// which supported ScopeType.ScopeType and the c# version which doesn't allow members with
    /// the same naem as their parent type
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
	    public ScopeType() : this(Null.NullString)
        {
        }

        public ScopeType(string scopeType)
        {
            ScopeTypeId = Null.NullInteger;
            ScopeType = scopeType;
        }

        public int ScopeTypeId { get; set; }

	    public void Fill(IDataReader dr)
        {
            ScopeTypeId = Null.SetNullInteger(dr["ScopeTypeID"]);
            ScopeType = Null.SetNullString(dr["ScopeType"]);
        }

        public int KeyID
        {
            get
            {
                return ScopeTypeId;
            }
            set
            {
                ScopeTypeId = value;
            }
        }

        public override string ToString()
        {
            return ScopeType;
        }
    }
}
