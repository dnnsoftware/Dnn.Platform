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
        public ListEntryInfo()
        {
            ParentKey = Null.NullString;
            Parent = Null.NullString;
            Description = Null.NullString;
            Text = Null.NullString;
            Value = Null.NullString;
            ListName = Null.NullString;
        }

        public int EntryID { get; set; }

        public int PortalID { get; set; }

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

        public string ListName { get; set; }

        public string DisplayName
        {
            get
            {
                return ListName + ":" + Text;
            }
        }

        public string Value { get; set; }

        public string Text { get; set; }

        public string Description { get; set; }

        public int ParentID { get; set; }

        public string Parent { get; set; }

        public int Level { get; set; }

        public int SortOrder { get; set; }

        public int DefinitionID { get; set; }

        public bool HasChildren { get; set; }

        public string ParentKey { get; set; }

        public bool SystemList { get; set; }
    }
}