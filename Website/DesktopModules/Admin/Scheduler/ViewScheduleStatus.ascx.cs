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
using System.Collections.Generic;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.UI.Skins.Controls;

using Microsoft.VisualBasic;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Scheduler
{

	/// <summary>
	/// The ViewScheduleStatus PortalModuleBase is used to view the schedule Status
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
	///                       and localisation
	/// </history>
	public partial class ViewScheduleStatus : PortalModuleBase, IActionable
	{

		private ScheduleStatus Status;

		#region IActionable Members

		public ModuleActionCollection ModuleActions
		{
			get
			{
				var actionCollection = new ModuleActionCollection
								  {
									  {
										  GetNextActionID(), Localization.GetString(ModuleActionType.AddContent, LocalResourceFile), ModuleActionType.AddContent, "", "add.gif", EditUrl(), false,
										  SecurityAccessLevel.Admin, true, false
										  },
									  {
										  GetNextActionID(), Localization.GetString("ScheduleHistory.Action", LocalResourceFile), ModuleActionType.AddContent, "", "icon_profile_16px.gif",
										  EditUrl("", "", "History"), false, SecurityAccessLevel.Host, true, false
										  }
								  };
				return actionCollection;
			}
		}

		#endregion

		#region Private Methods

		private void BindData()
		{
			lblFreeThreads.Text = SchedulingProvider.Instance().GetFreeThreadCount().ToString();
			lblActiveThreads.Text = SchedulingProvider.Instance().GetActiveThreadCount().ToString();
			lblMaxThreads.Text = SchedulingProvider.Instance().GetMaxThreadCount().ToString();

			Collection arrScheduleQueue = SchedulingProvider.Instance().GetScheduleQueue();
			if (arrScheduleQueue.Count == 0)
			{
				pnlScheduleQueue.Visible = false;
			}
			else
			{
				dgScheduleQueue.DataSource = arrScheduleQueue;
				dgScheduleQueue.DataBind();
			}
			
			Collection arrScheduleProcessing = SchedulingProvider.Instance().GetScheduleProcessing();
			if (arrScheduleProcessing.Count == 0)
			{
				pnlScheduleProcessing.Visible = false;
			}
			else
			{

				dgScheduleProcessing.DataSource = arrScheduleProcessing;
				dgScheduleProcessing.DataBind();
			}
			if (arrScheduleProcessing.Count == 0 && arrScheduleQueue.Count == 0)
			{
				var strMessage = Localization.GetString("NoTasks", LocalResourceFile);
				UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.YellowWarning);
			}
		}

		private void BindStatus()
		{
			Status = SchedulingProvider.Instance().GetScheduleStatus();
			lblStatus.Text = Status.ToString();

			placeCommands.Visible = SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD;

			if (Status == ScheduleStatus.STOPPED && SchedulingProvider.SchedulerMode != SchedulerMode.DISABLED)
			{
				cmdStart.Enabled = true;
				cmdStop.Enabled = false;
			}
			else if (Status == ScheduleStatus.WAITING_FOR_REQUEST || SchedulingProvider.SchedulerMode == SchedulerMode.DISABLED)
			{
				cmdStart.Enabled = false;
				cmdStop.Enabled = false;
			}
			else
			{
				cmdStart.Enabled = false;
				cmdStop.Enabled = true;
			}
		}

		#endregion

		#region Protected Methods

		protected string GetOverdueText(double overdueBy)
		{
			return overdueBy > 0 ? overdueBy.ToString() : "";
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

			cmdStart.Click += OnStartClick;
			cmdStop.Click += OnStopClick;
			cmdCancel.NavigateUrl = Globals.NavigateURL();

			try
			{
				if (SchedulingProvider.Enabled)
				{
					if (!Page.IsPostBack)
					{
						BindData();
						BindStatus();
					}
				}
				else
				{
					UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DisabledMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					lblStatus.Text = Localization.GetString("Disabled", LocalResourceFile);
					cmdStart.Enabled = false;
					cmdStop.Enabled = false;
					lblFreeThreads.Text = "0";
					lblActiveThreads.Text = "0";
					lblMaxThreads.Text = "0";
					pnlScheduleQueue.Visible = false;
					pnlScheduleProcessing.Visible = false;
				}
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		/// <summary>
		/// cmdStart_Click runs when the Start Button is clicked
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
		///                       and localisation
		/// </history>
		protected void OnStartClick(Object sender, EventArgs e)
		{
			SchedulingProvider.Instance().StartAndWaitForResponse();
			BindData();
			BindStatus();
		}

		/// <summary>
		/// cmdStop_Click runs when the Stop Button is clicked
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
		///                       and localisation
		/// </history>
		protected void OnStopClick(Object sender, EventArgs e)
		{
			SchedulingProvider.Instance().Halt(Localization.GetString("ManuallyStopped", LocalResourceFile));
			BindData();
			BindStatus();
		}

		#endregion

	}
}