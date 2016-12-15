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

namespace DNNConnect.CKEditorProvider.Controls
{

    /// <summary>
    /// The url control.
    /// </summary>
    public abstract class UrlControl : UserControl
    {
        /// <summary>
        /// The _local resource file.
        /// </summary>
        private string _localResourceFile;

        /// <summary>
        /// The with events field folders.
        /// </summary>
        private DropDownList folders;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlControl"/> class.
        /// </summary>
        protected UrlControl()
        {
            PreRender += Page_PreRender;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether [reload files].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reload files]; otherwise, <c>false</c>.
        /// </value>
        public bool ReloadFiles
        {
            get
            {
                return ViewState["ReloadFiles"] == null || Convert.ToBoolean(ViewState["ReloadFiles"]);
            }

            set
            {
                ViewState["ReloadFiles"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the file filter.
        /// </summary>
        public string FileFilter
        {
            get
            {
                return ViewState["FileFilter"] != null
                           ? Convert.ToString(ViewState["FileFilter"])
                           : string.Empty;
            }

            set
            {
                ViewState["FileFilter"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the local resource file.
        /// </summary>
        public string LocalResourceFile
        {
            get
            {
                return string.IsNullOrEmpty(_localResourceFile)
                           ? string.Format(
                               "{0}/{1}/URLControl.ascx.resx",
                               TemplateSourceDirectory.Replace(
                                   "Providers/HtmlEditorProviders/CKEditor", "controls"),
                               Localization.LocalResourceDirectory)
                           : _localResourceFile;
            }

            set
            {
                _localResourceFile = value;
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public string Width
        {
            get
            {
                return Convert.ToString(ViewState["SkinControlWidth"]);
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                Folders.Width = Unit.Parse(value);
                Files.Width = Unit.Parse(value);
                ViewState["SkinControlWidth"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int PortalId
        {
            get
            {
                return Convert.ToInt32(ViewState["PortalId"]);
            }

            set
            {
                ViewState["PortalId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url
        {
            get
            {
                string url = string.Empty;
                if (Files.SelectedItem == null)
                {
                    return url;
                }

                url = !string.IsNullOrEmpty(Files.SelectedItem.Value)
                          ? string.Format("FileID={0}", Files.SelectedItem.Value)
                          : string.Empty;

                return url;
            }

            set
            {
                ViewState["Url"] = value;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        protected DropDownList Files { get; set; }

        /// <summary>
        /// Gets or sets the file label.
        /// </summary>
        /// <value>
        /// The file label.
        /// </value>
        protected Label FileLabel { get; set; }

        /// <summary>
        /// Gets or sets the folder label.
        /// </summary>
        /// <value>
        /// The folder label.
        /// </value>
        protected Label FolderLabel { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the folders.
        /// </summary>
        protected DropDownList Folders
        {
            get
            {
                return folders;
            }

            set
            {
                if (folders != null)
                {
                    folders.SelectedIndexChanged -= Folders_SelectedIndexChanged;
                }

                folders = value;
                if (folders != null)
                {
                    folders.SelectedIndexChanged += Folders_SelectedIndexChanged;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the data.
        /// </summary>
        public void BindData()
        {
            LoadFolders();

            Files.Items.Clear();
            Files.DataSource = GetFileList(true);
            Files.DataBind();

            ReloadFiles = false;

            var _url = Convert.ToString(ViewState["Url"]);

            if (string.IsNullOrEmpty(_url))
            {
                return;
            }

            var _urltype = Globals.GetURLType(_url).ToString("g").Substring(0, 1);

            if (_urltype == "F")
            {
                if (_url.ToLower().StartsWith("fileid="))
                {
                    var objFile = FileManager.Instance.GetFile(int.Parse(_url.Substring(7)));

                    if (objFile != null)
                    {
                        _url = objFile.Folder + objFile.FileName;

                        var fileName = _url.Substring(_url.LastIndexOf("/", StringComparison.Ordinal) + 1);
                        var folderPath = _url.Replace(fileName, string.Empty);

                        if (Folders.Items.FindByValue(folderPath) != null)
                        {
                            Folders.ClearSelection();
                            Folders.Items.FindByValue(folderPath).Selected = true;
                        }
                        else if (Folders.Items.Count > 0)
                        {
                            Folders.ClearSelection();
                            Folders.Items[0].Selected = true;
                        }

                        // Reload files list
                        Files.Items.Clear();
                        Files.DataSource = GetFileList(true);
                        Files.DataBind();

                        if (Files.Items.FindByText(fileName) != null)
                        {
                            Files.ClearSelection();
                            Files.SelectedIndex = Files.Items.IndexOf(Files.Items.FindByText(fileName));
                        }
                    }
                }
            }

            ViewState["Url"] = _url;
        }

        /// <summary>
        /// The get file list.
        /// </summary>
        /// <param name="noneSpecified">
        /// The none specified.
        /// </param>
        /// <returns>
        /// The <see cref="ArrayList"/>.
        /// </returns>
        private ArrayList GetFileList(bool noneSpecified)
        {
            return Globals.GetFileList(
                PortalId, FileFilter, noneSpecified, Folders.SelectedItem.Value, false);
        }

        /// <summary>
        /// The load folders.
        /// </summary>
        private void LoadFolders()
        {
            Folders.Items.Clear();

            var foldersList = FolderManager.Instance.GetFolders(PortalId);

            foreach (ListItem folderItem in from FolderInfo folder in foldersList
                                            select
                                                new ListItem
                                                {
                                                    Text =
                                                        folder.FolderPath == Null.NullString
                                                            ? Localization.GetString(
                                                                "Root", LocalResourceFile)
                                                            : folder.FolderPath,
                                                    Value = folder.FolderPath
                                                })
            {
                Folders.Items.Add(folderItem);
            }
        }

        /// <summary>
        /// Handles the PreRender event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Page_PreRender(object sender, EventArgs e)
        {
            try
            {
                if (ReloadFiles)
                {
                    BindData();
                }
            }
            catch (Exception exc)
            {
                // Let's detect possible problems
                Exceptions.LogException(new Exception("Error rendering URLControl subcontrols.", exc));
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the Folders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Folders_SelectedIndexChanged(object sender, EventArgs e)
        {
            Files.Items.Clear();
            Files.DataSource = GetFileList(true);
            Files.DataBind();

            if (Folders.SelectedIndex >= 0)
            {
                ViewState["LastFolderPath"] = Folders.SelectedValue;
            }
            else
            {
                ViewState["LastFolderPath"] = string.Empty;
            }

            if (Files.SelectedIndex >= 0)
            {
                ViewState["LastFileName"] = Files.SelectedValue;
            }
            else
            {
                ViewState["LastFileName"] = string.Empty;
            }
        }

        #endregion
    }
}