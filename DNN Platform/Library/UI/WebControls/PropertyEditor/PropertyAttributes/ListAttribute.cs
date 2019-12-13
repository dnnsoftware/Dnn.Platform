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
