// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using DNNConnect.CKEditorProvider.Web;
using DotNetNuke.Common;
using DotNetNuke.Modules.HTMLEditorProvider;

namespace DNNConnect.CKEditorProvider
{

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
        private EditorControl htmlEditorControl;

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
                return additionalToolbars;
            }

            set
            {
                additionalToolbars = value;
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
                return htmlEditorControl.Height;
            }

            set
            {
                htmlEditorControl.Height = value;
            }
        }

        /// <summary>
        /// Gets HtmlEditorControl.
        /// </summary>
        public override Control HtmlEditorControl
        {
            get
            {
                return htmlEditorControl;
            }
        }

        /// <summary>
        /// Gets or sets RootImageDirectory.
        /// </summary>
        public override string RootImageDirectory
        {
            get
            {
                if (rootImageDirectory == string.Empty)
                {
                    // Remove the Application Path from the Home Directory
                    return Globals.ApplicationPath != string.Empty
                               ? PortalSettings.HomeDirectory.Replace(Globals.ApplicationPath, string.Empty)
                               : PortalSettings.HomeDirectory;
                }

                return rootImageDirectory;
            }

            set
            {
                rootImageDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets Text.
        /// </summary>
        public override string Text
        {
            get
            {
                return htmlEditorControl.Value;
            }

            set
            {
                htmlEditorControl.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets Width.
        /// </summary>
        public override Unit Width
        {
            get
            {
                return htmlEditorControl.Width;
            }

            set
            {
                htmlEditorControl.Width = value;
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
            htmlEditorControl = new EditorControl { ID = ControlID };
        }

        #endregion
    }
}