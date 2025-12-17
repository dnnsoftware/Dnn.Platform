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
        private DnnFilePickerUploader fileControl;

        // private DnnFilePicker _fileControl;

        /// <summary>Gets or sets the current file extension filter.</summary>
        public string FileFilter { get; set; }

        /// <summary>Gets or sets the current file path.</summary>
        public string FilePath { get; set; }

        /// <remarks>
        ///   In this case because the <see cref = "fileControl" /> is a contained control, we do not need
        ///   to process the PostBackData (it has been handled by the File Control).  We just use
        ///   this method as the Framework calls it for us.
        /// </remarks>
        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;

            // string postedValue = postCollection[string.Format("{0}FileControl$dnnFileUploadFileId", postDataKey)];
            string postedValue = this.fileControl.FileID.ToString();
            if (!presentValue.Equals(postedValue, StringComparison.Ordinal))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            // First clear the controls collection
            this.Controls.Clear();

            var userControl = this.Page.LoadControl("~/controls/filepickeruploader.ascx");
            this.fileControl = userControl as DnnFilePickerUploader;

            if (this.fileControl != null)
            {
                this.fileControl.ID = string.Format("{0}FileControl", this.ID);
                this.fileControl.FileFilter = this.FileFilter;
                this.fileControl.FilePath = this.FilePath;
                this.fileControl.FileID = this.IntegerValue;
                this.fileControl.UsePersonalFolder = true;
                this.fileControl.User = this.User;
                this.fileControl.Required = true;

                // Add table to Control
                this.Controls.Add(this.fileControl);
            }

            base.CreateChildControls();
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            this.EnsureChildControls();
            base.OnInit(e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.fileControl.FileID = this.IntegerValue;

            if (this.Page != null)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        /// <inheritdoc />
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }
    }
}
