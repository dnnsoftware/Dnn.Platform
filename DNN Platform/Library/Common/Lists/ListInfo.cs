// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;

    [Serializable]
    public class ListInfo : BaseEntityInfo
    {
        public ListInfo()
            : this(string.Empty)
        {
        }

        public ListInfo(string Name)
        {
            this.SystemList = Null.NullBoolean;
            this.EnableSortOrder = Null.NullBoolean;
            this.IsPopulated = Null.NullBoolean;
            this.ParentList = Null.NullString;
            this.Parent = Null.NullString;
            this.ParentKey = Null.NullString;
            this.PortalID = Null.NullInteger;
            this.DefinitionID = Null.NullInteger;
            this.Name = Name;
        }

        public string DisplayName
        {
            get
            {
                string _DisplayName = this.Parent;
                if (!string.IsNullOrEmpty(_DisplayName))
                {
                    _DisplayName += ":";
                }

                return _DisplayName + this.Name;
            }
        }

        public string Key
        {
            get
            {
                string _Key = this.ParentKey;
                if (!string.IsNullOrEmpty(_Key))
                {
                    _Key += ":";
                }

                return _Key + this.Name;
            }
        }

        public int DefinitionID { get; set; }

        public bool EnableSortOrder { get; set; }

        public int EntryCount { get; set; }

        public bool IsPopulated { get; set; }

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
