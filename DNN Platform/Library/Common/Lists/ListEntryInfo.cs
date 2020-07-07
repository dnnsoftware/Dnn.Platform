// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists
{
    using System;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common.Utilities;

    [Serializable]
    public class ListEntryInfo
    {
        private string _Text = Null.NullString;

        public ListEntryInfo()
        {
            this.ParentKey = Null.NullString;
            this.Parent = Null.NullString;
            this.Description = Null.NullString;
            this.Text = Null.NullString;
            this.Value = Null.NullString;
            this.ListName = Null.NullString;
        }

        public string Key
        {
            get
            {
                string _Key = this.ParentKey.Replace(":", ".");
                if (!string.IsNullOrEmpty(_Key))
                {
                    _Key += ".";
                }

                return _Key + this.ListName + ":" + this.Value;
            }
        }

        public string DisplayName
        {
            get
            {
                return this.ListName + ":" + this.Text;
            }
        }

        /// <summary>
        /// Gets the text value bypassing localization.
        /// </summary>
        /// <value>
        /// The text value of the list entry item as it was set originally.
        /// </value>
        public string TextNonLocalized
        {
            get
            {
                return this._Text;
            }
        }

        public int EntryID { get; set; }

        public int PortalID { get; set; }

        public string ListName { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// Gets or sets localized text value of the list entry item. An attempt is made to look up the key "[ParentKey].[Value].Text" in the resource file
        /// "App_GlobalResources/List_[ListName]". If not found the original (database) value is used.
        /// </summary>
        /// <value>
        /// Localized text value.
        /// </value>
        public string Text
        {
            get
            {
                string res = null;
                try
                {
                    string key;
                    if (string.IsNullOrEmpty(this.ParentKey))
                    {
                        key = this.Value + ".Text";
                    }
                    else
                    {
                        key = this.ParentKey + '.' + this.Value + ".Text";
                    }

                    res = Services.Localization.Localization.GetString(key, this.ResourceFileRoot);
                }
                catch
                {
                    // ignore
                }

                if (string.IsNullOrEmpty(res))
                {
                    res = this._Text;
                }

                return res;
            }

            set
            {
                this._Text = value;
            }
        }

        public string Description { get; set; }

        public int ParentID { get; set; }

        public string Parent { get; set; }

        public int Level { get; set; }

        public int SortOrder { get; set; }

        public int DefinitionID { get; set; }

        public bool HasChildren { get; set; }

        public string ParentKey { get; set; }

        public bool SystemList { get; set; }

        internal string ResourceFileRoot
        {
            get
            {
                var listName = this.ListName.Replace(":", ".");
                if (listName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
                {
                    listName = Globals.CleanFileName(listName);
                }

                return "~/App_GlobalResources/List_" + listName + ".resx";
            }
        }
    }
}
