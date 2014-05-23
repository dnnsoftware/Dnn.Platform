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

using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Security
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditRoles PortalModuleBase is used to manage a Security Role
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class EditGroups : PortalModuleBase
    {
		#region "Private Members"

        private int RoleGroupID = -1;

		#endregion

		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdDelete.Click += cmdDelete_Click;
            cmdUpdate.Click += cmdUpdate_Click;

            try
            {
                if ((Request.QueryString["RoleGroupID"] != null))
                {
                    RoleGroupID = Int32.Parse(Request.QueryString["RoleGroupID"]);
                }
                if (Page.IsPostBack == false)
                {
                    ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"));

                    if (RoleGroupID != -1)
                    {
                        RoleGroupInfo objRoleGroupInfo = RoleController.GetRoleGroup(PortalId, RoleGroupID);
                        if (objRoleGroupInfo != null)
                        {
                            txtRoleGroupName.Text = objRoleGroupInfo.RoleGroupName;
                            txtDescription.Text = objRoleGroupInfo.Description;

                            //Check if Group has any roles assigned
                            int roleCount = RoleController.Instance.GetRoles(PortalId, r => r.RoleGroupID == RoleGroupID).Count;

                            if (roleCount > 0)
                            {
                                cmdDelete.Visible = false;
                            }
                        }
                        else //security violation attempt to access item not related to this Module
                        {
                            Response.Redirect(Globals.NavigateURL("Security Roles"));
                        }
                    }
                    else
                    {
                        cmdDelete.Visible = false;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the update Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// 	[jlucarino]	2/26/2009	Added CreatedByUserID and LastModifiedByUserID
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    var objRoleGroupInfo = new RoleGroupInfo();
                    objRoleGroupInfo.PortalID = PortalId;
                    objRoleGroupInfo.RoleGroupID = RoleGroupID;
                    objRoleGroupInfo.RoleGroupName = txtRoleGroupName.Text;
                    objRoleGroupInfo.Description = txtDescription.Text;
                    if (RoleGroupID == -1)
                    {
                        try
                        {
                            RoleController.AddRoleGroup(objRoleGroupInfo);
                        }
                        catch
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DuplicateRoleGroup", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                        Response.Redirect(Globals.NavigateURL(TabId, ""));
                    }
                    else
                    {
                        RoleController.UpdateRoleGroup(objRoleGroupInfo);
                        Response.Redirect(Globals.NavigateURL(TabId, "", "RoleGroupID=" + RoleGroupID));
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdDelete_Click runs when the delete Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                RoleController.DeleteRoleGroup(PortalId, RoleGroupID);
                Response.Redirect(Globals.NavigateURL());
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdCancel_Click runs when the cancel Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (RoleGroupID == -1)
                {
                    Response.Redirect(Globals.NavigateURL(TabId, ""));
                }
                else
                {
                    Response.Redirect(Globals.NavigateURL(TabId, "", "RoleGroupID=" + RoleGroupID));
                }
            }
            catch (Exception exc) //
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
		
		#endregion
    }
}