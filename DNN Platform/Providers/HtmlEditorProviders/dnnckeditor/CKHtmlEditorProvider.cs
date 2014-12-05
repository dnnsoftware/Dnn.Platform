/*
 * CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor
{
    #region

    using System.Collections;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Modules.HTMLEditorProvider;

    using WatchersNET.CKEditor.Web;

    #endregion

    /// <summary>
    /// The CKEditor Provider.
    /// </summary>
    public class CKHtmlEditorProvider : HtmlEditorProvider
    {
        #region Constants and Fields

        /// <summary>
        /// The _additional toolbars.
        /// </summary>
        private ArrayList additionalToolbars = new ArrayList();

        /// <summary>
        /// The _html editor control.
        /// </summary>
        private CKEditorControl htmlEditorControl;

        /// <summary>
        /// The _root image directory.
        /// </summary>
        private string rootImageDirectory;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets AdditionalToolbars.
        /// </summary>
        public override ArrayList AdditionalToolbars
        {
            get
            {
                return this.additionalToolbars;
            }

            set
            {
                this.additionalToolbars = value;
            }
        }

        /// <summary>
        /// Gets or sets ControlID.
        /// </summary>
        public override string ControlID { get; set; }

        /// <summary>
        /// Gets or sets Height.
        /// </summary>
        public override Unit Height
        {
            get
            {
                return this.htmlEditorControl.Height;
            }

            set
            {
                this.htmlEditorControl.Height = value;
            }
        }

        /// <summary>
        /// Gets HtmlEditorControl.
        /// </summary>
        public override Control HtmlEditorControl
        {
            get
            {
                return this.htmlEditorControl;
            }
        }

        /// <summary>
        /// Gets or sets RootImageDirectory.
        /// </summary>
        public override string RootImageDirectory
        {
            get
            {
                if (this.rootImageDirectory == string.Empty)
                {
                    // Remove the Application Path from the Home Directory
                    return Globals.ApplicationPath != string.Empty
                               ? this.PortalSettings.HomeDirectory.Replace(Globals.ApplicationPath, string.Empty)
                               : this.PortalSettings.HomeDirectory;
                }

                return this.rootImageDirectory;
            }

            set
            {
                this.rootImageDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets Text.
        /// </summary>
        public override string Text
        {
            get
            {
                return this.htmlEditorControl.Value;
            }

            set
            {
                this.htmlEditorControl.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets Width.
        /// </summary>
        public override Unit Width
        {
            get
            {
                return this.htmlEditorControl.Width;
            }

            set
            {
                this.htmlEditorControl.Width = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The add toolbar.
        /// </summary>
        public override void AddToolbar()
        {
            // Must exist because it exists at the base class
        }

        /// <summary>
        /// The initialize.
        /// </summary>
        public override void Initialize()
        {
            this.htmlEditorControl = new CKEditorControl { ID = this.ControlID };
        }

        #endregion
    }
}