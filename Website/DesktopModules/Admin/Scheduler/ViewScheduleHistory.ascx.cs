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
using System.Collections;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Modules.Admin.Scheduler
{

    /// <summary>
    /// The ViewScheduleHistory PortalModuleBase is used to view the schedule History
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    public partial class ViewScheduleHistory : PortalModuleBase, IActionable
    {

        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new ModuleActionCollection();
                Actions.Add(GetNextActionID(),
                            Localization.GetString(ModuleActionType.AddContent, LocalResourceFile),
                            ModuleActionType.AddContent,
                            "",
                            "add.gif",
                            EditUrl(),
                            false,
                            SecurityAccessLevel.Host,
                            true,
                            false);
                Actions.Add(GetNextActionID(),
                            Localization.GetString(ModuleActionType.ContentOptions, LocalResourceFile),
                            ModuleActionType.AddContent,
                            "",
                            "icon_scheduler_16px.gif",
                            EditUrl("", "", "Status"),
                            false,
                            SecurityAccessLevel.Host,
                            true,
                            false);
                return Actions;
            }
        }

        #endregion

		#region Event Handlers

        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
				cmdCancel.NavigateUrl = Globals.NavigateURL();
                dgHistory.NeedDataSource += OnGridNeedDataSource;
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnGridNeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            int scheduleID;
            if (Request.QueryString["ScheduleID"] != null)
            {
                //get history for specific scheduleid
                scheduleID = Convert.ToInt32(Request.QueryString["ScheduleID"]);
            }
            else
            {
                //get history for all schedules
                scheduleID = -1;
            }

            var arrSchedule = SchedulingProvider.Instance().GetScheduleHistory(scheduleID);

            dgHistory.DataSource = arrSchedule;
        }

		#endregion

		#region Protected Methods

        protected string GetNotesText(string notes)
        {
            if (!String.IsNullOrEmpty(notes))
            {
                notes = "<textarea rows=\"5\" cols=\"65\">" + notes + "</textarea>";
                return notes;
            }
            return "";
        }
		
		#endregion

    }
}