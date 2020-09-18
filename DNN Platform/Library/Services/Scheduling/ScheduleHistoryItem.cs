// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Data;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;

    [Serializable]
    public class ScheduleHistoryItem : ScheduleItem
    {
        private static readonly ILog _tracelLogger = LoggerSource.Instance.GetLogger(typeof(ScheduleHistoryItem));

        private StringBuilder _LogNotes;
        private int _ScheduleHistoryID;
        private string _Server;
        private DateTime _StartDate;
        private bool _Succeeded;

        public ScheduleHistoryItem()
        {
            this._ScheduleHistoryID = Null.NullInteger;
            this._StartDate = Null.NullDate;
            this.EndDate = Null.NullDate;
            this._Succeeded = Null.NullBoolean;
            this._LogNotes = new StringBuilder();
            this._Server = Null.NullString;
        }

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
            this._ScheduleHistoryID = Null.NullInteger;
            this._StartDate = Null.NullDate;
            this.EndDate = Null.NullDate;
            this._Succeeded = Null.NullBoolean;
            this._LogNotes = new StringBuilder();
            this._Server = Null.NullString;
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
                    if (this.EndDate == Null.NullDate && this._StartDate != Null.NullDate)
                    {
                        return DateTime.Now.Subtract(this._StartDate).TotalSeconds;
                    }
                    else if (this._StartDate != Null.NullDate)
                    {
                        return this.EndDate.Subtract(this._StartDate).TotalSeconds;
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

        public override DateTime NextStart { get; set; }

        public DateTime EndDate { get; set; }

        public string LogNotes
        {
            get
            {
                return this._LogNotes.ToString();
            }

            set
            {
                this._LogNotes = new StringBuilder(value);
            }
        }

        public int ScheduleHistoryID
        {
            get
            {
                return this._ScheduleHistoryID;
            }

            set
            {
                this._ScheduleHistoryID = value;
            }
        }

        public string Server
        {
            get
            {
                return this._Server;
            }

            set
            {
                this._Server = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return this._StartDate;
            }

            set
            {
                this._StartDate = value;
            }
        }

        public bool Succeeded
        {
            get
            {
                return this._Succeeded;
            }

            set
            {
                this._Succeeded = value;
                if (_tracelLogger.IsDebugEnabled)
                {
                    _tracelLogger.Debug($"ScheduleHistoryItem.Succeeded Info (ScheduledTask {(value == false ? "Start" : "End")}): {this.FriendlyName}");
                }
            }
        }

        public virtual void AddLogNote(string notes)
        {
            this._LogNotes.Append(notes);
            if (_tracelLogger.IsTraceEnabled)
            {
                _tracelLogger.Trace(notes.Replace(@"<br/>", Environment.NewLine));
            }
        }

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
}
