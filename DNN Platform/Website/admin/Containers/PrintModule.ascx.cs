﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
                    DisplayAction(action);
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
	
	private void DisplayAction(ModuleAction action)
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
	    
	    foreach (ModuleAction subAction in action.Actions) 
	    {
	    	DisplayAction(subAction);
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
