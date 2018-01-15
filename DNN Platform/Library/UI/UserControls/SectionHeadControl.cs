#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Utilities;

#endregion

namespace DotNetNuke.UI.UserControls
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SectionHeadControl is a user control that provides all the server code to allow a
    /// section to be collapsed/expanded, using user provided images for the button.
    /// </summary>
    /// <remarks>
    /// To use this control the user must provide somewhere in the asp page the
    /// implementation of the javascript required to expand/collapse the display.
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SectionHeadControl : UserControl
    {
		#region "Private Members"

        private bool _includeRule;
        private bool _isExpanded = true;
        private string _javaScript = "__dnn_SectionMaxMin";
        private string _maxImageUrl = "images/plus.gif";
        private string _minImageUrl = "images/minus.gif";
        protected ImageButton imgIcon;
        protected Label lblTitle;
        protected Panel pnlRule;
		
		#endregion

		#region "Public Properties"


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CssClass determines the Css Class used for the Title Text
        /// </summary>
        /// <value>A string representing the name of the css class</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string CssClass
        {
            get
            {
                return lblTitle.CssClass;
            }
            set
            {
                lblTitle.CssClass = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IncludeRule determines whether there is a horizontal rule displayed under the
        /// header text
        /// </summary>
        /// <value>A string representing true or false</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool IncludeRule
        {
            get
            {
                return _includeRule;
            }
            set
            {
                _includeRule = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IsExpanded determines whether the section is expanded or collapsed.
        /// </summary>
        /// <value>Boolean value that determines whether the panel is expanded (true)
        /// or collapsed (false).  The default is true.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool IsExpanded
        {
            get
            {
                return DNNClientAPI.MinMaxContentVisibile(imgIcon, -1, !_isExpanded, DNNClientAPI.MinMaxPersistanceType.Page);
            }
            set
            {
                _isExpanded = value;
                DNNClientAPI.MinMaxContentVisibile(imgIcon, -1, !_isExpanded, DNNClientAPI.MinMaxPersistanceType.Page, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// JavaScript is the name of the javascript function implementation.
        /// </summary>
        /// <value>A string representing the name of the javascript function implementation</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string JavaScript
        {
            get
            {
                return _javaScript;
            }
            set
            {
                _javaScript = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The MaxImageUrl is the url of the image displayed when the contained panel is
        /// collapsed.
        /// </summary>
        /// <value>A string representing the url of the Max Image</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string MaxImageUrl
        {
            get
            {
                return _maxImageUrl;
            }
            set
            {
                _maxImageUrl = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The MinImageUrl is the url of the image displayed when the contained panel is
        /// expanded.
        /// </summary>
        /// <value>A string representing the url of the Min Image</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string MinImageUrl
        {
            get
            {
                return _minImageUrl;
            }
            set
            {
                _minImageUrl = value;
            }
        }

 /// -----------------------------------------------------------------------------
 /// <summary>
 /// The ResourceKey is the key used to identify the Localization Resource for the
 /// title text.
 /// </summary>
 /// <value>A string representing the ResourceKey.</value>
 /// <remarks>
 /// </remarks>
 /// -----------------------------------------------------------------------------
        public string ResourceKey { get; set; }

 /// -----------------------------------------------------------------------------
 /// <summary>
 /// The Section is the Id of the DHTML object  that contains the xection content
 /// title text.
 /// </summary>
 /// <value>A string representing the Section.</value>
 /// <remarks>
 /// </remarks>
 /// -----------------------------------------------------------------------------
        public string Section { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Text is the name or title of the section
        /// </summary>
        /// <value>A string representing the Title Text.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string Text
        {
            get
            {
                return lblTitle.Text;
            }
            set
            {
                lblTitle.Text = value;
            }
        }

		#endregion

		#region "Event Handlers"
		
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Assign resource key to label for localization
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
				//set the resourcekey attribute to the label
                if (!String.IsNullOrEmpty(ResourceKey))
                {
                    lblTitle.Attributes["resourcekey"] = ResourceKey;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Renders the SectionHeadControl
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                var ctl = (HtmlControl) Parent.FindControl(Section);
                if (ctl != null)
                {
                    lblTitle.Attributes.Add("onclick", imgIcon.ClientID + ".click();");
                    lblTitle.Attributes.Add("style", "cursor: pointer");
                    DNNClientAPI.EnableMinMax(imgIcon, ctl, !IsExpanded, Page.ResolveUrl(MinImageUrl), Page.ResolveUrl(MaxImageUrl), DNNClientAPI.MinMaxPersistanceType.Page);
                }
				
                //optionlly show hr
                pnlRule.Visible = _includeRule;
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            var ctl = (HtmlControl) Parent.FindControl(Section);
            if (ctl != null)
            {
                IsExpanded = !IsExpanded;
            }
        }
		
		#endregion
    }
}
