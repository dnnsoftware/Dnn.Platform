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

using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : Containers.Icon
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Contains the attributes of an Icon.  
    /// These are read into the PortalModuleBase collection as attributes for the icons within the module controls.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[sun1]	        2/1/2004	Created
    /// 	[Nik Kalyani]	10/15/2004	Replaced public members with properties and removed
    ///                                 brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class PrintModule : ActionBase
    {
		#region "Public Members"
		
        public string PrintIcon { get; set; }
		
		#endregion
		
		#region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                foreach (ModuleAction action in Actions)
                {
                    if (action.CommandName == ModuleActionType.PrintModule)
                    {
                        if (action.Visible)
                        {
                            if ((PortalSettings.UserMode == PortalSettings.Mode.Edit) || (action.Secure == SecurityAccessLevel.Anonymous || action.Secure == SecurityAccessLevel.View))
                            {
                                if (ModuleContext.Configuration.DisplayPrint)
                                {
                                    var ModuleActionIcon = new ImageButton();
                                    if (!String.IsNullOrEmpty(PrintIcon))
                                    {
                                        ModuleActionIcon.ImageUrl = ModuleContext.Configuration.ContainerPath.Substring(0, ModuleContext.Configuration.ContainerPath.LastIndexOf("/") + 1) + PrintIcon;
                                    }
                                    else
                                    {
                                        ModuleActionIcon.ImageUrl = "~/images/" + action.Icon;
                                    }
                                    ModuleActionIcon.ToolTip = action.Title;
                                    ModuleActionIcon.ID = "ico" + action.ID;
                                    ModuleActionIcon.CausesValidation = false;

                                    ModuleActionIcon.Click += IconAction_Click;

                                    Controls.Add(ModuleActionIcon);
                                }
                            }
                        }
                    }
                }
				
                //set visibility
                if (Controls.Count > 0)
                {
                    Visible = true;
                }
                else
                {
                    Visible = false;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void IconAction_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ProcessAction(((ImageButton) sender).ID.Substring(3));
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
		
		#endregion
    }
}