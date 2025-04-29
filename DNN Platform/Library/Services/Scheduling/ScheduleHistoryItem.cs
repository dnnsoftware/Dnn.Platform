// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling;

using System;
using System.Data;
using System.Text;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

[Serializable]
public class ScheduleHistoryItem : ScheduleItem
{
    private static readonly ILog TracelLogger = LoggerSource.Instance.GetLogger(typeof(ScheduleHistoryItem));

    private StringBuilder logNotes;
    private int scheduleHistoryID;
    private string server;
    private DateTime startDate;
    private bool succeeded;

    /// <summary>Initializes a new instance of the <see cref="ScheduleHistoryItem"/> class.</summary>
    public ScheduleHistoryItem()
    {
        this.scheduleHistoryID = Null.NullInteger;
        this.startDate = Null.NullDate;
        this.EndDate = Null.NullDate;
        this.succeeded = Null.NullBoolean;
        this.logNotes = new StringBuilder();
        this.server = Null.NullString;
    }

    /// <summary>Initializes a new instance of the <see cref="ScheduleHistoryItem"/> class.</summary>
    /// <param name="objScheduleItem"></param>
    public ScheduleHistoryItem(ScheduleItem objScheduleItem)
    {
        this.AttachToEvent = objScheduleItem.AttachToEvent;
        this.CatchUpEnabled = objScheduleItem.CatchUpEnabled;
        this.Enabled = objScheduleItem.Enabled;
        this.NextStart = objScheduleItem.NextStart;
        this.ObjectDependencies = objScheduleItem.ObjectDependencies;
        this.ProcessGroup = objScheduleItem.ProcessGroup;
        this.RetainHistoryNum = objScheduleItem.RetainHistoryNum;
        this.RetryTimeLapse = objScheduleItem.RetryTimeLapse;
        this.RetryTimeLapseMeasurement = objScheduleItem.RetryTimeLapseMeasurement;
        this.ScheduleID = objScheduleItem.ScheduleID;
        this.ScheduleSource = objScheduleItem.ScheduleSource;
        this.ThreadID = objScheduleItem.ThreadID;
        this.TimeLapse = objScheduleItem.TimeLapse;
        this.TimeLapseMeasurement = objScheduleItem.TimeLapseMeasurement;
        this.TypeFullName = objScheduleItem.TypeFullName;
        this.Servers = objScheduleItem.Servers;
        this.FriendlyName = objScheduleItem.FriendlyName;
        this.scheduleHistoryID = Null.NullInteger;
        this.startDate = Null.NullDate;
        this.EndDate = Null.NullDate;
        this.succeeded = Null.NullBoolean;
        this.logNotes = new StringBuilder();
        this.server = Null.NullString;
        this.ScheduleStartDate = objScheduleItem.ScheduleStartDate != Null.NullDate
            ? objScheduleItem.ScheduleStartDate
            : Null.NullDate;
    }

    public double ElapsedTime
    {
        get
        {
            try
            {
                if (this.EndDate == Null.NullDate && this.startDate != Null.NullDate)
                {
                    return DateTime.Now.Subtract(this.startDate).TotalSeconds;
                }
                else if (this.startDate != Null.NullDate)
                {
                    return this.EndDate.Subtract(this.startDate).TotalSeconds;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }

    public bool Overdue
    {
        get
        {
            if (this.NextStart < DateTime.Now && this.EndDate == Null.NullDate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public double OverdueBy
    {
        get
        {
            try
            {
                if (this.NextStart <= DateTime.Now && this.EndDate == Null.NullDate)
                {
                    return Math.Round(DateTime.Now.Subtract(this.NextStart).TotalSeconds);
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }

    public double RemainingTime
    {
        get
        {
            try
            {
                if (this.NextStart > DateTime.Now && this.EndDate == Null.NullDate)
                {
                    return Math.Round(this.NextStart.Subtract(DateTime.Now).TotalSeconds);
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <inheritdoc/>
    public override DateTime NextStart { get; set; }

    public DateTime EndDate { get; set; }

    public string LogNotes
    {
        get
        {
            return this.logNotes.ToString();
        }

        set
        {
            this.logNotes = new StringBuilder(value);
        }
    }

    public int ScheduleHistoryID
    {
        get
        {
            return this.scheduleHistoryID;
        }

        set
        {
            this.scheduleHistoryID = value;
        }
    }

    public string Server
    {
        get
        {
            return this.server;
        }

        set
        {
            this.server = value;
        }
    }

    public DateTime StartDate
    {
        get
        {
            return this.startDate;
        }

        set
        {
            this.startDate = value;
        }
    }

    public bool Succeeded
    {
        get
        {
            return this.succeeded;
        }

        set
        {
            this.succeeded = value;
            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ScheduleHistoryItem.Succeeded Info (ScheduledTask {(value == false ? "Start" : "End")}): {this.FriendlyName}");
            }
        }
    }

    public virtual void AddLogNote(string notes)
    {
        this.logNotes.Append(notes);
        if (TracelLogger.IsTraceEnabled)
        {
            TracelLogger.Trace(notes.Replace(@"<br/>", Environment.NewLine));
        }
    }

    /// <inheritdoc/>
    public override void Fill(IDataReader dr)
    {
        this.ScheduleHistoryID = Null.SetNullInteger(dr["ScheduleHistoryID"]);
        this.StartDate = Null.SetNullDateTime(dr["StartDate"]);
        this.EndDate = Null.SetNullDateTime(dr["EndDate"]);
        this.Succeeded = Null.SetNullBoolean(dr["Succeeded"]);
        this.LogNotes = Null.SetNullString(dr["LogNotes"]);
        this.Server = Null.SetNullString(dr["Server"]);
        this.FillInternal(dr);
    }
}
