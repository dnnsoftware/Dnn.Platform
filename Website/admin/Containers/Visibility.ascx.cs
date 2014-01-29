#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : Containers.Visibility
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Handles the events for collapsing and expanding modules, 
    /// Showing or hiding admin controls when preview is checked
    /// if personalization of the module container and title is allowed for that module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[sun1]	    2/1/2004	Created
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Visibility : SkinObjectBase
    {
#region "Private Members"

        private int _animationFrames = 5;
        private Panel _pnlModuleContent;

        private string MinIconLoc
        {
            get
            {
                if (!String.IsNullOrEmpty(minIcon))
                {
                    return ModulePath + minIcon;
                }
                
                return Globals.ApplicationPath + "/images/min.gif"; //is ~/ the same as ApplicationPath in all cases?
            }
        }

        private string MaxIconLoc
        {
            get
            {
                if (!String.IsNullOrEmpty(MaxIcon))
                {
                    return ModulePath + MaxIcon;
                }
                
                return Globals.ApplicationPath + "/images/max.gif"; //is ~/ the same as ApplicationPath in all cases?
            }
        }

        private Panel ModuleContent
        {
            get
            {
                if (_pnlModuleContent == null)
                {
                    Control objCtl = Parent.FindControl("ModuleContent");
                    if (objCtl != null)
                    {
                        _pnlModuleContent = (Panel) objCtl;
                    }
                }
                return _pnlModuleContent;
            }
        }

        private string ModulePath
        {
            get
            {
                return ModuleControl.ModuleContext.Configuration.ContainerPath.Substring(0, ModuleControl.ModuleContext.Configuration.ContainerPath.LastIndexOf("/") + 1);
            }
        }
		
		#endregion

		#region "Public Members"


        public int AnimationFrames
        {
            get
            {
                return _animationFrames;
            }
            set
            {
                _animationFrames = value;
            }
        }

        public string BorderWidth { get; set; }

        public bool ContentVisible
        {
            get
            {
                switch (ModuleControl.ModuleContext.Configuration.Visibility)
                {
                    case VisibilityState.Maximized:
                    case VisibilityState.Minimized:
                        return DNNClientAPI.MinMaxContentVisibile(cmdVisibility,
                                                                  ModuleControl.ModuleContext.ModuleId,
                                                                  ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.Minimized,
                                                                  DNNClientAPI.MinMaxPersistanceType.Cookie);
                    default:
                        return true;
                }
            }
            set
            {
                DNNClientAPI.MinMaxContentVisibile(cmdVisibility,
                                                   ModuleControl.ModuleContext.ModuleId,
                                                   ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.Minimized,
                                                   DNNClientAPI.MinMaxPersistanceType.Cookie,
                                                   value);
            }
        }

// ReSharper disable InconsistentNaming
//TODO can this be renamed with a capital M
        public string minIcon { get; set; }
// ReSharper restore InconsistentNaming

        public string MaxIcon { get; set; }

        public string ResourceFile
        {
            get
            {
                return Localization.GetResourceFile(this, "Visibility.ascx");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdVisibility.Click += cmdVisibility_Click;

            try
            {
                if (!Page.IsPostBack)
                {
					//public attributes
                    if (!String.IsNullOrEmpty(BorderWidth))
                    {
                        cmdVisibility.BorderWidth = Unit.Parse(BorderWidth);
                    }
                    if (ModuleControl.ModuleContext.Configuration != null)
                    {
						//check if Personalization is allowed
                        if (ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.None)
                        {
                            cmdVisibility.Enabled = false;
                            cmdVisibility.Visible = false;
                        }
                        if (ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.Minimized)
                        {
							//if visibility is set to minimized, then the client needs to set the cookie for maximized only and delete the cookie for minimized,
                            //instead of the opposite.  We need to notify the client of this
                            ClientAPI.RegisterClientVariable(Page, "__dnn_" + ModuleControl.ModuleContext.ModuleId + ":defminimized", "true", true);
                        }
                        if (!Globals.IsAdminControl())
                        {
                            if (cmdVisibility.Enabled)
                            {
                                if (ModuleContent != null)
                                {
									//EnableMinMax now done in prerender
                                }
                                else
                                {
                                    Visible = false;
                                }
                            }
                        }
                        else
                        {
                            Visible = false;
                        }
                    }
                    else
                    {
                        Visible = false;
                    }
                }
                else
                {
                    //since we disabled viewstate on the cmdVisibility control we need to check to see if we need hide this on postbacks as well
                    if (ModuleControl.ModuleContext.Configuration != null)
                    {
                        if (ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.None)
                        {
                            cmdVisibility.Enabled = false;
                            cmdVisibility.Visible = false;
                        }
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (ModuleContent != null && ModuleControl != null && !Globals.IsAdminControl())
            {
                switch (ModuleControl.ModuleContext.Configuration.Visibility)
                {
                    case VisibilityState.Maximized:
                    case VisibilityState.Minimized:
                        DNNClientAPI.EnableMinMax(cmdVisibility,
                                                  ModuleContent,
                                                  ModuleControl.ModuleContext.ModuleId,
                                                  ModuleControl.ModuleContext.Configuration.Visibility == VisibilityState.Minimized,
                                                  MinIconLoc,
                                                  MaxIconLoc,
                                                  DNNClientAPI.MinMaxPersistanceType.Cookie,
                                                  AnimationFrames);
                        break;
                }
            }
        }

        private void cmdVisibility_Click(Object sender, EventArgs e)
        {
            try
            {
                if (ModuleContent != null)
                {
                    if (ModuleContent.Visible)
                    {
                        ContentVisible = false;
                    }
                    else
                    {
                        ContentVisible = true;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
		
		#endregion
    }
}