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
        private int mDefinitionID = Null.NullInteger;
        private bool mEnableSortOrder = Null.NullBoolean;
        private int mEntryCount;
        private bool mIsPopulated = Null.NullBoolean;
        private int mLevel;
        private string mName = Null.NullString;
        private string mParent = Null.NullString;
        private int mParentID;
        private string mParentKey = Null.NullString;
        private string mParentList = Null.NullString;
        private int mPortalID = Null.NullInteger;
        private bool mSystemList = Null.NullBoolean;

        public ListInfo(string Name)
        {
            mName = Name;
        }

        public ListInfo()
        {
        }

        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }

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

        public int Level
        {
            get
            {
                return mLevel;
            }
            set
            {
                mLevel = value;
            }
        }

        public int DefinitionID
        {
            get
            {
                return mDefinitionID;
            }
            set
            {
                mDefinitionID = value;
            }
        }

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

        public int EntryCount
        {
            get
            {
                return mEntryCount;
            }
            set
            {
                mEntryCount = value;
            }
        }

        public int PortalID
        {
            get
            {
                return mPortalID;
            }
            set
            {
                mPortalID = value;
            }
        }

        public int ParentID
        {
            get
            {
                return mParentID;
            }
            set
            {
                mParentID = value;
            }
        }

        public string ParentKey
        {
            get
            {
                return mParentKey;
            }
            set
            {
                mParentKey = value;
            }
        }

        public string Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }

        public string ParentList
        {
            get
            {
                return mParentList;
            }
            set
            {
                mParentList = value;
            }
        }

        public bool IsPopulated
        {
            get
            {
                return mIsPopulated;
            }
            set
            {
                mIsPopulated = value;
            }
        }

        public bool EnableSortOrder
        {
            get
            {
                return mEnableSortOrder;
            }
            set
            {
                mEnableSortOrder = value;
            }
        }

        public bool SystemList
        {
            get
            {
                return mSystemList;
            }
            set
            {
                mSystemList = value;
            }
        }
    }
}