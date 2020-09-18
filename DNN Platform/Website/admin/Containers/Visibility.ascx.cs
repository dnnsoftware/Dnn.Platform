// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Utilities;

    using Globals = DotNetNuke.Common.Globals;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : Containers.Visibility
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Handles the events for collapsing and expanding modules,
    /// Showing or hiding admin controls when preview is checked
    /// if personalization of the module container and title is allowed for that module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class Visibility : SkinObjectBase
    {
        private int _animationFrames = 5;
        private Panel _pnlModuleContent;

        public string ResourceFile
        {
            get
            {
                return Localization.GetResourceFile(this, "Visibility.ascx");
            }
        }

        public int AnimationFrames
        {
            get
            {
                return this._animationFrames;
            }

            set
            {
                this._animationFrames = value;
            }
        }

        public string BorderWidth { get; set; }

        public bool ContentVisible
        {
            get
            {
                switch (this.ModuleControl.ModuleContext.Configuration.Visibility)
                {
                    case VisibilityState.Maximized:
                    case VisibilityState.Minimized:
                        return DNNClientAPI.MinMaxContentVisibile(
                            this.cmdVisibility,
                            this.ModuleControl.ModuleContext.ModuleId,
                            this.ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.Minimized,
                            DNNClientAPI.MinMaxPersistanceType.Cookie);
                    default:
                        return true;
                }
            }

            set
            {
                DNNClientAPI.MinMaxContentVisibile(
                    this.cmdVisibility,
                    this.ModuleControl.ModuleContext.ModuleId,
                    this.ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.Minimized,
                    DNNClientAPI.MinMaxPersistanceType.Cookie,
                    value);
            }
        }

        // ReSharper disable InconsistentNaming
        // TODO can this be renamed with a capital M
        public string minIcon { get; set; }

        // ReSharper restore InconsistentNaming
        public string MaxIcon { get; set; }

        private string MinIconLoc
        {
            get
            {
                if (!string.IsNullOrEmpty(this.minIcon))
                {
                    return this.ModulePath + this.minIcon;
                }

                return Globals.ApplicationPath + "/images/min.gif"; // is ~/ the same as ApplicationPath in all cases?
            }
        }

        private string MaxIconLoc
        {
            get
            {
                if (!string.IsNullOrEmpty(this.MaxIcon))
                {
                    return this.ModulePath + this.MaxIcon;
                }

                return Globals.ApplicationPath + "/images/max.gif"; // is ~/ the same as ApplicationPath in all cases?
            }
        }

        private Panel ModuleContent
        {
            get
            {
                if (this._pnlModuleContent == null)
                {
                    Control objCtl = this.Parent.FindControl("ModuleContent");
                    if (objCtl != null)
                    {
                        this._pnlModuleContent = (Panel)objCtl;
                    }
                }

                return this._pnlModuleContent;
            }
        }

        private string ModulePath
        {
            get
            {
                return this.ModuleControl.ModuleContext.Configuration.ContainerPath.Substring(0, this.ModuleControl.ModuleContext.Configuration.ContainerPath.LastIndexOf("/") + 1);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdVisibility.Click += this.cmdVisibility_Click;

            try
            {
                if (!this.Page.IsPostBack)
                {
                    // public attributes
                    if (!string.IsNullOrEmpty(this.BorderWidth))
                    {
                        this.cmdVisibility.BorderWidth = Unit.Parse(this.BorderWidth);
                    }

                    if (this.ModuleControl.ModuleContext.Configuration != null)
                    {
                        // check if Personalization is allowed
                        if (this.ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.None)
                        {
                            this.cmdVisibility.Enabled = false;
                            this.cmdVisibility.Visible = false;
                        }

                        if (this.ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.Minimized)
                        {
                            // if visibility is set to minimized, then the client needs to set the cookie for maximized only and delete the cookie for minimized,
                            // instead of the opposite.  We need to notify the client of this
                            ClientAPI.RegisterClientVariable(this.Page, "__dnn_" + this.ModuleControl.ModuleContext.ModuleId + ":defminimized", "true", true);
                        }

                        if (!Globals.IsAdminControl())
                        {
                            if (this.cmdVisibility.Enabled)
                            {
                                if (this.ModuleContent != null)
                                {
                                    // EnableMinMax now done in prerender
                                }
                                else
                                {
                                    this.Visible = false;
                                }
                            }
                        }
                        else
                        {
                            this.Visible = false;
                        }
                    }
                    else
                    {
                        this.Visible = false;
                    }
                }
                else
                {
                    // since we disabled viewstate on the cmdVisibility control we need to check to see if we need hide this on postbacks as well
                    if (this.ModuleControl.ModuleContext.Configuration != null)
                    {
                        if (this.ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.None)
                        {
                            this.cmdVisibility.Enabled = false;
                            this.cmdVisibility.Visible = false;
                        }
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.ModuleContent != null && this.ModuleControl != null && !Globals.IsAdminControl())
            {
                switch (this.ModuleControl.ModuleContext.Configuration.Visibility)
                {
                    case VisibilityState.Maximized:
                    case VisibilityState.Minimized:
                        DNNClientAPI.EnableMinMax(
                            this.cmdVisibility,
                            this.ModuleContent,
                            this.ModuleControl.ModuleContext.ModuleId,
                            this.ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.Minimized,
                            this.MinIconLoc,
                            this.MaxIconLoc,
                            DNNClientAPI.MinMaxPersistanceType.Cookie,
                            this.AnimationFrames);
                        break;
                }
            }
        }

        private void cmdVisibility_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.ModuleContent != null)
                {
                    if (this.ModuleContent.Visible)
                    {
                        this.ContentVisible = false;
                    }
                    else
                    {
                        this.ContentVisible = true;
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
