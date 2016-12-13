#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ListAttribute : Attribute
    {
        private readonly string _ListName;
        private readonly string _ParentKey;
        private readonly ListBoundField _TextField;
        private readonly ListBoundField _ValueField;

        /// <summary>
        /// Initializes a new instance of the ListAttribute class.
        /// </summary>
        /// <param name="listName">The name of the List to use for this property</param>
        /// <param name="parentKey">The key of the parent for this List</param>
        /// <param name="textField">Text Field.</param>
        /// <param name="valueField">Value Field.</param>
        public ListAttribute(string listName, string parentKey, ListBoundField valueField, ListBoundField textField)
        {
            _ListName = listName;
            _ParentKey = parentKey;
            _TextField = textField;
            _ValueField = valueField;
        }

        public string ListName
        {
            get
            {
                return _ListName;
            }
        }

        public string ParentKey
        {
            get
            {
                return _ParentKey;
            }
        }

        public ListBoundField TextField
        {
            get
            {
                return _TextField;
            }
        }

        public ListBoundField ValueField
        {
            get
            {
                return _ValueField;
            }
        }
    }
}
