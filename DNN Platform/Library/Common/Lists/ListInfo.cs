#region Usings

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;

#endregion

namespace DotNetNuke.Common.Lists
{
    [Serializable]
    public class ListInfo : BaseEntityInfo
    {
        public ListInfo() : this(String.Empty) { }

        public ListInfo(string Name)
        {
            SystemList = Null.NullBoolean;
            EnableSortOrder = Null.NullBoolean;
            IsPopulated = Null.NullBoolean;
            ParentList = Null.NullString;
            Parent = Null.NullString;
            ParentKey = Null.NullString;
            PortalID = Null.NullInteger;
            DefinitionID = Null.NullInteger;
            this.Name = Name;
        }

        public int DefinitionID { get; set; }

        public string DisplayName
        {
            get
            {
                string _DisplayName = Parent;
                if (!string.IsNullOrEmpty(_DisplayName))
                {
                    _DisplayName += ":";
                }
                return _DisplayName + Name;
            }
        }

        public bool EnableSortOrder { get; set; }

        public int EntryCount { get; set; }

        public bool IsPopulated { get; set; }

        public string Key
        {
            get
            {
                string _Key = ParentKey;
                if (!string.IsNullOrEmpty(_Key))
                {
                    _Key += ":";
                }
                return _Key + Name;
            }
        }

        public int Level { get; set; }

        public string Name { get; set; }

        public string Parent { get; set; }

        public int ParentID { get; set; }

        public string ParentKey { get; set; }

        public string ParentList { get; set; }

        public int PortalID { get; set; }

        public bool SystemList { get; set; }
    }
}
