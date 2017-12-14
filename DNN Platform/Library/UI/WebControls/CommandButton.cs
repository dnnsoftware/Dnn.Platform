#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Web.UI.WebControls;

using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Project:    DotNetNuke
    /// Class:      CommandButton
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CommandButton Class provides an enhanced Button control for DotNetNuke
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:CommandButton runat=server></{0}:CommandButton>")]
    public class CommandButton : WebControl, INamingContainer
    {
        private ImageButton icon;
        private LinkButton link;
        private LiteralControl separator;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Separator between Buttons
        /// </summary>
        /// <remarks>Defaults to 1 non-breaking spaces</remarks>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string ButtonSeparator
        {
            get
            {
                EnsureChildControls();
                return separator.Text;
            }
            set
            {
                EnsureChildControls();
                separator.Text = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets whether the control causes Validation to occur
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool CausesValidation
        {
            get
            {
                EnsureChildControls();
                return link.CausesValidation;
            }
            set
            {
                EnsureChildControls();
                icon.CausesValidation = value;
                link.CausesValidation = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the command argument for this command button
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string CommandArgument
        {
            get
            {
                EnsureChildControls();
                return link.CommandArgument;
            }
            set
            {
                EnsureChildControls();
                icon.CommandArgument = value;
                link.CommandArgument = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the command name for this command button
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string CommandName
        {
            get
            {
                EnsureChildControls();
                return link.CommandName;
            }
            set
            {
                EnsureChildControls();
                icon.CommandName = value;
                link.CommandName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets whether the link is displayed
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool DisplayLink
        {
            get
            {
                EnsureChildControls();
                return link.Visible;
            }
            set
            {
                EnsureChildControls();
                link.Visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets whether the icon is displayed
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool DisplayIcon
        {
            get
            {
                EnsureChildControls();
                return icon.Visible;
            }
            set
            {
                EnsureChildControls();
                icon.Visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Image used for the Icon
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string ImageUrl
        {
            get
            {
                EnsureChildControls();                
                if (string.IsNullOrEmpty(icon.ImageUrl))
                    icon.ImageUrl = Entities.Icons.IconController.IconURL(IconKey, IconSize, IconStyle);

                return icon.ImageUrl;
            }
            set
            {
                EnsureChildControls();
                icon.ImageUrl = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Icon Key to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string IconKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Icon Siz to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string IconSize { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Icon Style to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string IconStyle { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the "onClick" Attribute
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string OnClick
        {
            get
            {
                EnsureChildControls();
                return link.Attributes["onclick"];
            }
            set
            {
                EnsureChildControls();
                if (String.IsNullOrEmpty(value))
                {
                    icon.Attributes.Remove("onclick");
                    link.Attributes.Remove("onclick");
                }
                else
                {
                    icon.Attributes.Add("onclick", value);
                    link.Attributes.Add("onclick", value);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the "OnClientClick" Property
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string OnClientClick
        {
            get
            {
                EnsureChildControls();
                return link.OnClientClick;
            }
            set
            {
                EnsureChildControls();
                icon.OnClientClick = value;
                link.OnClientClick = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Resource Key used for the Control
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string ResourceKey
        {
            get
            {
                EnsureChildControls();
                return link.Attributes["resourcekey"];
            }
            set
            {
                EnsureChildControls();
                if (String.IsNullOrEmpty(value))
                {
                    icon.Attributes.Remove("resourcekey");
                    link.Attributes.Remove("resourcekey");
                }
                else
                {
                    icon.Attributes.Add("resourcekey", value);
                    link.Attributes.Add("resourcekey", value);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Text used for the Control
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Text
        {
            get
            {
                EnsureChildControls();
                return link.Text;
            }
            set
            {
                EnsureChildControls();
                link.Text = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the tooltip resource key used for the Control
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string ToolTipKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Validation Group that this control "validates"
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return link.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                icon.ValidationGroup = value;
                link.ValidationGroup = value;
            }
        }

        public string LocalResourceFile { get; set; }

        public event EventHandler Click;
        public event CommandEventHandler Command;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateChildControls overrides the Base class's method to correctly build the
        /// control based on the configuration
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            Controls.Clear();
            if (String.IsNullOrEmpty(CssClass))
            {
                CssClass = "CommandButton";
            }
            icon = new ImageButton();
            icon.Visible = true;
            icon.CausesValidation = true;
            icon.Click += RaiseImageClick;
            icon.Command += RaiseCommand;
            Controls.Add(icon);
            separator = new LiteralControl();
            separator.Text = "&nbsp;";
            Controls.Add(separator);
            link = new LinkButton();
            link.Visible = true;
            link.CausesValidation = true;
            link.Click += RaiseClick;
            link.Command += RaiseCommand;
            Controls.Add(link);
            if (DisplayIcon && !String.IsNullOrEmpty(ImageUrl))
            {
                icon.EnableViewState = EnableViewState;
            }
            if (DisplayLink)
            {
                link.CssClass = CssClass;
                link.EnableViewState = EnableViewState;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnButtonClick raises the CommandButton control's Click event
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnButtonClick(EventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnCommand raises the CommandButton control's Command event
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnCommand(CommandEventArgs e)
        {
            if (Command != null)
            {
                Command(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs just before the Render phase of the Page Life Cycle
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            EnsureChildControls();
            separator.Visible = DisplayLink && DisplayIcon;

            LocalResourceFile = UIUtilities.GetLocalResourceFile(this);

            var tooltipText = string.Empty;
            if (!string.IsNullOrEmpty(ToolTipKey))
            {
                tooltipText = Localization.GetString(ToolTipKey, LocalResourceFile);
            }

            if (string.IsNullOrEmpty(tooltipText) && !string.IsNullOrEmpty(ToolTip))
            {
                tooltipText = ToolTip;
            }

            if (!string.IsNullOrEmpty(tooltipText))
            {
                icon.ToolTip = link.ToolTip = icon.AlternateText = tooltipText;
            }
        }

        public void RegisterForPostback()
        {
            AJAX.RegisterPostBackControl(link);
            AJAX.RegisterPostBackControl(icon);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RaiseClick runs when one of the contained Link buttons is clciked
        /// </summary>
        /// <remarks>It raises a Click Event.
        /// </remarks>
        /// <param name="sender"> The object that triggers the event</param>
        /// <param name="e">An EventArgs object</param>
        /// -----------------------------------------------------------------------------
        private void RaiseClick(object sender, EventArgs e)
        {
            OnButtonClick(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RaiseCommand runs when one of the contained Link buttons is clicked
        /// </summary>
        /// <remarks>It raises a Command Event.
        /// </remarks>
        /// <param name="sender"> The object that triggers the event</param>
        /// <param name="e">An CommandEventArgs object</param>
        /// -----------------------------------------------------------------------------
        private void RaiseCommand(object sender, CommandEventArgs e)
        {
            OnCommand(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RaiseImageClick runs when the Image button is clicked
        /// </summary>
        /// <remarks>It raises a Command Event.
        /// </remarks>
        /// <param name="sender"> The object that triggers the event</param>
        /// <param name="e">An ImageClickEventArgs object</param>
        /// -----------------------------------------------------------------------------
        protected void RaiseImageClick(object sender, ImageClickEventArgs e)
        {
            OnButtonClick(new EventArgs());
        }
    }
}
