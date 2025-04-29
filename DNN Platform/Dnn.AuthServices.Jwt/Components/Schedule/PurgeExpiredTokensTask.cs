// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Components.Schedule;

using System;

using Dnn.AuthServices.Jwt.Data;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;

/// <summary>Scheduled task to delete tokens that linger in the database after having expired.</summary>
public class PurgeExpiredTokensTask : SchedulerClient
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PurgeExpiredTokensTask));

    /// <summary>Initializes a new instance of the <see cref="PurgeExpiredTokensTask"/> class.</summary>
    /// <param name="objScheduleHistoryItem">The object used to record the results from this task.</param>
    public PurgeExpiredTokensTask(ScheduleHistoryItem objScheduleHistoryItem)
    {
        this.ScheduleHistoryItem = objScheduleHistoryItem;
    }

    /// <summary>Runs when the task is triggered by DNN.</summary>
    public override void DoWork()
    {
        try
        {
            Logger.Info("Starting PurgeExpiredTokensTask");
            DataService.Instance.DeleteExpiredTokens();
            Logger.Info("Finished PurgeExpiredTokensTask");
            this.ScheduleHistoryItem.Succeeded = true;
        }
        catch (Exception exc)
        {
            this.ScheduleHistoryItem.Succeeded = false;
            this.ScheduleHistoryItem.AddLogNote(string.Format("Purging expired tokens task failed: {0}.", exc.ToString()));
            this.Errored(ref exc);
            Logger.ErrorFormat("Error in PurgeExpiredTokensTask: {0}. {1}", exc.Message, exc.StackTrace);
            Exceptions.LogException(exc);
        }
    }
}
