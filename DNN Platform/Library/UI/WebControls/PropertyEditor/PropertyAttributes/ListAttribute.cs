// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ListAttribute : Attribute
    {
        private readonly string _ListName;
        private readonly string _ParentKey;
        private readonly ListBoundField _TextField;
        private readonly ListBoundField _ValueField;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListAttribute"/> class.
        /// </summary>
        /// <param name="listName">The name of the List to use for this property.</param>
        /// <param name="parentKey">The key of the parent for this List.</param>
        /// <param name="textField">Text Field.</param>
        /// <param name="valueField">Value Field.</param>
        public ListAttribute(string listName, string parentKey, ListBoundField valueField, ListBoundField textField)
        {
            this._ListName = listName;
            this._ParentKey = parentKey;
            this._TextField = textField;
            this._ValueField = valueField;
        }

        public string ListName
        {
            get
            {
                return this._ListName;
            }
        }

        public string ParentKey
        {
            get
            {
                return this._ParentKey;
            }
        }

        public ListBoundField TextField
        {
            get
            {
                return this._TextField;
            }
        }

        public ListBoundField ValueField
        {
            get
            {
                return this._ValueField;
            }
        }
    }
}
