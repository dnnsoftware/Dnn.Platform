// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Web.UI;

    using DotNetNuke.UI.WebControls;

    public class DnnFileEditControl : IntegerEditControl
    {
        private DnnFilePickerUploader _fileControl;

        // private DnnFilePicker _fileControl;

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the current file extension filter.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string FileFilter { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the current file path.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string FilePath { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Loads the Post Back Data and determines whether the value has change.
        /// </summary>
        /// <remarks>
        ///   In this case because the <see cref = "_fileControl" /> is a contained control, we do not need
        ///   to process the PostBackData (it has been handled by the File Control).  We just use
        ///   this method as the Framework calls it for us.
        /// </remarks>
        /// <param name = "postDataKey">A key to the PostBack Data to load.</param>
        /// <param name = "postCollection">A name value collection of postback data.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;

            // string postedValue = postCollection[string.Format("{0}FileControl$dnnFileUploadFileId", postDataKey)];
            string postedValue = this._fileControl.FileID.ToString();
            if (!presentValue.Equals(postedValue))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Creates the control contained within this control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            // First clear the controls collection
            this.Controls.Clear();

            var userControl = this.Page.LoadControl("~/controls/filepickeruploader.ascx");
            this._fileControl = userControl as DnnFilePickerUploader;

            if (this._fileControl != null)
            {
                this._fileControl.ID = string.Format("{0}FileControl", this.ID);
                this._fileControl.FileFilter = this.FileFilter;
                this._fileControl.FilePath = this.FilePath;
                this._fileControl.FileID = this.IntegerValue;
                this._fileControl.UsePersonalFolder = true;
                this._fileControl.User = this.User;
                this._fileControl.Required = true;

                // Add table to Control
                this.Controls.Add(this._fileControl);
            }

            base.CreateChildControls();
        }

        protected override void OnInit(EventArgs e)
        {
            this.EnsureChildControls();
            base.OnInit(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Runs before the control is rendered.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this._fileControl.FileID = this.IntegerValue;

            if (this.Page != null)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Renders the control in edit mode.
        /// </summary>
        /// <param name = "writer">An HtmlTextWriter to render the control to.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }
    }
}
