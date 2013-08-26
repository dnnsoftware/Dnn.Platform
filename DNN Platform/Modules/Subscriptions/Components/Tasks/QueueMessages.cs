#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

namespace DotNetNuke.Subscriptions.Components.Tasks 
{

	/// <summary>
	/// 
	/// </summary>
	public class QueueMessages : Services.Scheduling.SchedulerClient
	{

		#region Constructor


		#endregion

		#region Public Method

		/// <summary>
		/// 
		/// </summary>
		public override void DoWork()
		{
			try
			{
				Progressing(); // OPTIONAL
			
				var currentRunDate = DateTime.Now;
				var lastInstantRun = DateTime.Now;
				var lastDailyRun = DateTime.MinValue;

				var colScheduleItemSettings = Services.Scheduling.SchedulingProvider.Instance().GetScheduleItemSettings(ScheduleHistoryItem.ScheduleID);


				ScheduleHistoryItem.Succeeded = true; // REQUIRED
				//ScheduleHistoryItem.AddLogNote(strResults); // OPTIONAL
			}
			catch (Exception exc)
			{
				ScheduleHistoryItem.Succeeded = false; // REQUIRED
				//ScheduleHistoryItem.AddLogNote("Q&A Emailer Task Failed. " + exc); // OPTIONAL
				//this.Errored(exc); // REQUIRED
				Services.Exceptions.Exceptions.LogException(exc); // OPTIONAL
				throw;
			}
		}

		#endregion
	
	}
}