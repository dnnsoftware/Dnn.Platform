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