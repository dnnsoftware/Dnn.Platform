// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists;

using System;
using System.IO;

using DotNetNuke.Common.Utilities;

/// <summary>Represents one entry in the Dnn lists.</summary>
[Serializable]
public class ListEntryInfo
{
    private string text = Null.NullString;

    /// <summary>Initializes a new instance of the <see cref="ListEntryInfo"/> class.</summary>
    public ListEntryInfo()
    {
        this.ParentKey = Null.NullString;
        this.Parent = Null.NullString;
        this.Description = Null.NullString;
        this.Text = Null.NullString;
        this.Value = Null.NullString;
        this.ListName = Null.NullString;
    }

    /// <summary>Gets a unique key for the entry.</summary>
    public string Key
    {
        get
        {
            string key = this.ParentKey.Replace(":", ".");
            if (!string.IsNullOrEmpty(key))
            {
                key += ".";
            }

            return key + this.ListName + ":" + this.Value;
        }
    }

    /// <summary>Gets a display name for the entry which includes the list name and the text.</summary>
    public string DisplayName
    {
        get
        {
            return this.ListName + ":" + this.Text;
        }
    }

    /// <summary>Gets the text value bypassing localization.</summary>
    /// <value>
    /// The text value of the list entry item as it was set originally.
    /// </value>
    public string TextNonLocalized
    {
        get
        {
            return this.text;
        }
    }

    /// <summary>Gets or sets the id of the entry.</summary>
    public int EntryID { get; set; }

    /// <summary>
    /// Gets or sets the id of the site (portal) this entry belongs to.
    /// Will be <see cref="Null.NullInteger"/> if not scoped to any site.
    /// </summary>
    public int PortalID { get; set; }

    /// <summary>Gets or sets the name of the list this entry belongs to.</summary>
    public string ListName { get; set; }

    /// <summary>Gets or sets the value of the entry.</summary>
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
                res = this.text;
            }

            return res;
        }

        set
        {
            this.text = value;
        }
    }

    /// <summary>Gets or sets the entry description.</summary>
    public string Description { get; set; }

    /// <summary>Gets or sets the id of the parent entry.</summary>
    public int ParentID { get; set; }

    /// <summary>Gets or sets the parent entry.</summary>
    public string Parent { get; set; }

    /// <summary>Gets or sets the level (how deep in the hierarchy).</summary>
    public int Level { get; set; }

    /// <summary>Gets or sets an integer that dictates the sort order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets the id of the definition.</summary>
    public int DefinitionID { get; set; }

    /// <summary>Gets or sets a value indicating whether the entry has any children.</summary>
    public bool HasChildren { get; set; }

    /// <summary>Gets or sets the parent key.</summary>
    public string ParentKey { get; set; }

    /// <summary>Gets or sets a value indicating whether the entry is part of a system list.</summary>
    public bool SystemList { get; set; }

    /// <summary>Gets the path to the localization resource file used for this entry.</summary>
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
