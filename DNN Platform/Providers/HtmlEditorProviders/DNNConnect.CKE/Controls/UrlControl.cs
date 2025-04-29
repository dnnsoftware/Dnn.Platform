// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Controls;

using System;
using System.Collections;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

/// <summary>The url control.</summary>
public abstract class UrlControl : UserControl
{
    /// <summary>The _local resource file.</summary>
    private string localResourceFile;

    /// <summary>The with events field folders.</summary>
    private DropDownList folders;

    /// <summary>Initializes a new instance of the <see cref="UrlControl"/> class.</summary>
    protected UrlControl()
    {
        this.PreRender += this.Page_PreRender;
    }

    /// <summary>Gets or sets a value indicating whether [reload files].</summary>
    /// <value>
    ///   <see langword="true"/> if [reload files]; otherwise, <see langword="false"/>.
    /// </value>
    public bool ReloadFiles
    {
        get
        {
            return this.ViewState["ReloadFiles"] == null || Convert.ToBoolean(this.ViewState["ReloadFiles"]);
        }

        set
        {
            this.ViewState["ReloadFiles"] = value;
        }
    }

    /// <summary>Gets or sets the file filter.</summary>
    public string FileFilter
    {
        get
        {
            return this.ViewState["FileFilter"] != null
                ? Convert.ToString(this.ViewState["FileFilter"])
                : string.Empty;
        }

        set
        {
            this.ViewState["FileFilter"] = value;
        }
    }

    /// <summary>Gets or sets the local resource file.</summary>
    public string LocalResourceFile
    {
        get
        {
            return string.IsNullOrEmpty(this.localResourceFile)
                ? string.Format(
                    "{0}/{1}/URLControl.ascx.resx",
                    this.TemplateSourceDirectory.Replace(
                        "Providers/HtmlEditorProviders/CKEditor", "controls"),
                    Localization.LocalResourceDirectory)
                : this.localResourceFile;
        }

        set
        {
            this.localResourceFile = value;
        }
    }

    /// <summary>Gets or sets the width.</summary>
    public string Width
    {
        get
        {
            return Convert.ToString(this.ViewState["SkinControlWidth"]);
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            this.Folders.Width = Unit.Parse(value);
            this.Files.Width = Unit.Parse(value);
            this.ViewState["SkinControlWidth"] = value;
        }
    }

    /// <summary>Gets or sets the width.</summary>
    public int PortalId
    {
        get
        {
            return Convert.ToInt32(this.ViewState["PortalId"]);
        }

        set
        {
            this.ViewState["PortalId"] = value;
        }
    }

    /// <summary>Gets or sets the url.</summary>
    public string Url
    {
        get
        {
            string url = string.Empty;
            if (this.Files.SelectedItem == null)
            {
                return url;
            }

            url = !string.IsNullOrEmpty(this.Files.SelectedItem.Value)
                ? string.Format("FileID={0}", this.Files.SelectedItem.Value)
                : string.Empty;

            return url;
        }

        set
        {
            this.ViewState["Url"] = value;
        }
    }

    /// <summary>Gets or sets the files.</summary>
    /// <value>
    /// The files.
    /// </value>
    protected DropDownList Files { get; set; }

    /// <summary>Gets or sets the file label.</summary>
    /// <value>
    /// The file label.
    /// </value>
    protected Label FileLabel { get; set; }

    /// <summary>Gets or sets the folder label.</summary>
    /// <value>
    /// The folder label.
    /// </value>
    protected Label FolderLabel { get; set; }

    /// <summary>Gets or sets the folders.</summary>
    protected DropDownList Folders
    {
        get
        {
            return this.folders;
        }

        set
        {
            if (this.folders != null)
            {
                this.folders.SelectedIndexChanged -= this.Folders_SelectedIndexChanged;
            }

            this.folders = value;
            if (this.folders != null)
            {
                this.folders.SelectedIndexChanged += this.Folders_SelectedIndexChanged;
            }
        }
    }

    /// <summary>Binds the data.</summary>
    public void BindData()
    {
        this.LoadFolders();

        this.Files.Items.Clear();
        this.Files.DataSource = this.GetFileList(true);
        this.Files.DataBind();

        this.ReloadFiles = false;

        var url = Convert.ToString(this.ViewState["Url"]);

        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        var urlType = Globals.GetURLType(url).ToString("g").Substring(0, 1);
        if (urlType == "F")
        {
            if (url.ToLower().StartsWith("fileid="))
            {
                var objFile = FileManager.Instance.GetFile(int.Parse(url.Substring(7)));

                if (objFile != null)
                {
                    url = objFile.Folder + objFile.FileName;

                    var fileName = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1);
                    var folderPath = url.Replace(fileName, string.Empty);

                    if (this.Folders.Items.FindByValue(folderPath) != null)
                    {
                        this.Folders.ClearSelection();
                        this.Folders.Items.FindByValue(folderPath).Selected = true;
                    }
                    else if (this.Folders.Items.Count > 0)
                    {
                        this.Folders.ClearSelection();
                        this.Folders.Items[0].Selected = true;
                    }

                    // Reload files list
                    this.Files.Items.Clear();
                    this.Files.DataSource = this.GetFileList(true);
                    this.Files.DataBind();

                    if (this.Files.Items.FindByText(fileName) != null)
                    {
                        this.Files.ClearSelection();
                        this.Files.SelectedIndex = this.Files.Items.IndexOf(this.Files.Items.FindByText(fileName));
                    }
                }
            }
        }

        this.ViewState["Url"] = url;
    }

    /// <summary>The get file list.</summary>
    /// <param name="noneSpecified">
    /// The none specified.
    /// </param>
    /// <returns>
    /// The <see cref="ArrayList"/>.
    /// </returns>
    private ArrayList GetFileList(bool noneSpecified)
    {
        return Globals.GetFileList(
            this.PortalId, this.FileFilter, noneSpecified, this.Folders.SelectedItem.Value, false);
    }

    /// <summary>The load folders.</summary>
    private void LoadFolders()
    {
        this.Folders.Items.Clear();

        var foldersList = FolderManager.Instance.GetFolders(this.PortalId);

        foreach (ListItem folderItem in from FolderInfo folder in foldersList
                 select
                     new ListItem
                     {
                         Text =
                             folder.FolderPath == Null.NullString
                                 ? Localization.GetString(
                                     "Root", this.LocalResourceFile)
                                 : folder.FolderPath,
                         Value = folder.FolderPath,
                     })
        {
            this.Folders.Items.Add(folderItem);
        }
    }

    /// <summary>Handles the PreRender event of the Page control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Page_PreRender(object sender, EventArgs e)
    {
        try
        {
            if (this.ReloadFiles)
            {
                this.BindData();
            }
        }
        catch (Exception exc)
        {
            // Let's detect possible problems
            Exceptions.LogException(new Exception("Error rendering URLControl subcontrols.", exc));
        }
    }

    /// <summary>Handles the SelectedIndexChanged event of the Folders control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Folders_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.Files.Items.Clear();
        this.Files.DataSource = this.GetFileList(true);
        this.Files.DataBind();

        if (this.Folders.SelectedIndex >= 0)
        {
            this.ViewState["LastFolderPath"] = this.Folders.SelectedValue;
        }
        else
        {
            this.ViewState["LastFolderPath"] = string.Empty;
        }

        if (this.Files.SelectedIndex >= 0)
        {
            this.ViewState["LastFileName"] = this.Files.SelectedValue;
        }
        else
        {
            this.ViewState["LastFileName"] = string.Empty;
        }
    }
}
