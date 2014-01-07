#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

#endregion

namespace DotNetNuke.Common.Lists
{
    [Serializable]
    public class ListEntryInfo
    {
        private int _DefinitionID;
        private string _Description = Null.NullString;
        private string _DisplayName = Null.NullString;
        private int _EntryID;
        private bool _HasChildren;
        private string _Key = Null.NullString;
        private int _Level;
        private string _ListName = Null.NullString;
        private string _Parent = Null.NullString;
        private int _ParentID;
        private string _ParentKey = Null.NullString;
        private int _PortalID;
        private int _SortOrder;
        private string _Text = Null.NullString;
        private string _Value = Null.NullString;
        private bool _systemlist;

        public int EntryID
        {
            get
            {
                return _EntryID;
            }
            set
            {
                _EntryID = value;
            }
        }

        public int PortalID
        {
            get
            {
                return _PortalID;
            }
            set
            {
                _PortalID = value;
            }
        }

        public string Key
        {
            get
            {
                string _Key = ParentKey.Replace(":", ".");
                if (!string.IsNullOrEmpty(_Key))
                {
                    _Key += ".";
                }
                return _Key + ListName + ":" + Value;
            }
        }

        public string ListName
        {
            get
            {
                return _ListName;
            }
            set
            {
                _ListName = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return ListName + ":" + Text;
            }
        }

        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }

        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
            }
        }

        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }

        public int ParentID
        {
            get
            {
                return _ParentID;
            }
            set
            {
                _ParentID = value;
            }
        }

        public string Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                _Parent = value;
            }
        }

        public int Level
        {
            get
            {
                return _Level;
            }
            set
            {
                _Level = value;
            }
        }

        public int SortOrder
        {
            get
            {
                return _SortOrder;
            }
            set
            {
                _SortOrder = value;
            }
        }

        public int DefinitionID
        {
            get
            {
                return _DefinitionID;
            }
            set
            {
                _DefinitionID = value;
            }
        }

        public bool HasChildren
        {
            get
            {
                return _HasChildren;
            }
            set
            {
                _HasChildren = value;
            }
        }

        public string ParentKey
        {
            get
            {
                return _ParentKey;
            }
            set
            {
                _ParentKey = value;
            }
        }

        public bool SystemList
        {
            get
            {
                return _systemlist;
            }
            set
            {
                _systemlist = value;
            }
        }
    }
}