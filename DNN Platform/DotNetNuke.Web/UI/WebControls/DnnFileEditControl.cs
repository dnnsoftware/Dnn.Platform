// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Specialized;
using System.Web.UI;

using DotNetNuke.UI.WebControls;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFileEditControl : IntegerEditControl
    {
        #region Private Fields

        private DnnFilePickerUploader _fileControl;
        //private DnnFilePicker _fileControl;

        #endregion

        #region Public Properties

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

        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Creates the control contained within this control
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            //First clear the controls collection
            Controls.Clear();

            var userControl = Page.LoadControl("~/controls/filepickeruploader.ascx");
            _fileControl = userControl as DnnFilePickerUploader;

            if (_fileControl != null)
            {
                _fileControl.ID = string.Format("{0}FileControl", ID);
                _fileControl.FileFilter = FileFilter;
                _fileControl.FilePath = FilePath;
                _fileControl.FileID = IntegerValue;
                _fileControl.UsePersonalFolder = true;
                _fileControl.User = User;
	            _fileControl.Required = true;

                //Add table to Control
                Controls.Add(_fileControl);
            }

            base.CreateChildControls();
        }

        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
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

            _fileControl.FileID = IntegerValue;

            if (Page != null)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Renders the control in edit mode
        /// </summary>
        /// <param name = "writer">An HtmlTextWriter to render the control to</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            RenderChildren(writer);
        }

        #endregion

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Loads the Post Back Data and determines whether the value has change
        /// </summary>
        /// <remarks>
        ///   In this case because the <see cref = "_fileControl" /> is a contained control, we do not need 
        ///   to process the PostBackData (it has been handled by the File Control).  We just use
        ///   this method as the Framework calls it for us.
        /// </remarks>
        /// <param name = "postDataKey">A key to the PostBack Data to load</param>
        /// <param name = "postCollection">A name value collection of postback data</param>
        /// -----------------------------------------------------------------------------
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = StringValue;
            //string postedValue = postCollection[string.Format("{0}FileControl$dnnFileUploadFileId", postDataKey)];
            string postedValue = _fileControl.FileID.ToString();
            if (!presentValue.Equals(postedValue))
            {
                Value = postedValue;
                dataChanged = true;
            }
            return dataChanged;
        }

        #endregion
    }
}
