// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Data;
using System.Text;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Scheduling
{
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
            _ScheduleHistoryID = Null.NullInteger;
            _StartDate = Null.NullDate;
            EndDate = Null.NullDate;
            _Succeeded = Null.NullBoolean;
            _LogNotes = new StringBuilder();
            _Server = Null.NullString;
        }

        public ScheduleHistoryItem(ScheduleItem objScheduleItem)
        {
            AttachToEvent = objScheduleItem.AttachToEvent;
            CatchUpEnabled = objScheduleItem.CatchUpEnabled;
            Enabled = objScheduleItem.Enabled;
            NextStart = objScheduleItem.NextStart;
            ObjectDependencies = objScheduleItem.ObjectDependencies;
            ProcessGroup = objScheduleItem.ProcessGroup;
            RetainHistoryNum = objScheduleItem.RetainHistoryNum;
            RetryTimeLapse = objScheduleItem.RetryTimeLapse;
            RetryTimeLapseMeasurement = objScheduleItem.RetryTimeLapseMeasurement;
            ScheduleID = objScheduleItem.ScheduleID;
            ScheduleSource = objScheduleItem.ScheduleSource;
            ThreadID = objScheduleItem.ThreadID;
            TimeLapse = objScheduleItem.TimeLapse;
            TimeLapseMeasurement = objScheduleItem.TimeLapseMeasurement;
            TypeFullName = objScheduleItem.TypeFullName;
            Servers = objScheduleItem.Servers;
            FriendlyName = objScheduleItem.FriendlyName;
            _ScheduleHistoryID = Null.NullInteger;
            _StartDate = Null.NullDate;
            EndDate = Null.NullDate;
            _Succeeded = Null.NullBoolean;
            _LogNotes = new StringBuilder();
            _Server = Null.NullString;
            ScheduleStartDate = objScheduleItem.ScheduleStartDate != Null.NullDate
                                    ? objScheduleItem.ScheduleStartDate
                                    : Null.NullDate;
        }

        public override DateTime NextStart { get; set; }

        public double ElapsedTime
        {
            get
            {
                try
                {
                    if (EndDate == Null.NullDate && _StartDate != Null.NullDate)
                    {
                        return DateTime.Now.Subtract(_StartDate).TotalSeconds;
                    }
                    else if (_StartDate != Null.NullDate)
                    {
                        return EndDate.Subtract(_StartDate).TotalSeconds;
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

        public DateTime EndDate { get; set; }

        public string LogNotes
        {
            get
            {
                return _LogNotes.ToString();
            }
            set
            {
                _LogNotes = new StringBuilder(value);
            }
        }

        public bool Overdue
        {
            get
            {
                if (NextStart < DateTime.Now && EndDate == Null.NullDate)
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
                    if (NextStart <= DateTime.Now && EndDate == Null.NullDate)
                    {
                        return Math.Round(DateTime.Now.Subtract(NextStart).TotalSeconds);
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
                    if (NextStart > DateTime.Now && EndDate == Null.NullDate)
                    {
                        return Math.Round(NextStart.Subtract(DateTime.Now).TotalSeconds);
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

        public int ScheduleHistoryID
        {
            get
            {
                return _ScheduleHistoryID;
            }
            set
            {
                _ScheduleHistoryID = value;
            }
        }

        public string Server
        {
            get
            {
                return _Server;
            }
            set
            {
                _Server = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                _StartDate = value;
            }
        }

        public bool Succeeded
        {
            get
            {
                return _Succeeded;
            }
            set
            {
                _Succeeded = value;
                if (_tracelLogger.IsDebugEnabled)
                    _tracelLogger.Debug($"ScheduleHistoryItem.Succeeded Info (ScheduledTask {(value == false ? "Start" : "End")}): {FriendlyName}");
            }
        }

        public virtual void AddLogNote(string notes)
        {
            _LogNotes.Append(notes);
            if (_tracelLogger.IsTraceEnabled)
                _tracelLogger.Trace(notes.Replace(@"<br/>", Environment.NewLine));
        }

        public override void Fill(IDataReader dr)
        {
            ScheduleHistoryID = Null.SetNullInteger(dr["ScheduleHistoryID"]);
            StartDate = Null.SetNullDateTime(dr["StartDate"]);
            EndDate = Null.SetNullDateTime(dr["EndDate"]);
            Succeeded = Null.SetNullBoolean(dr["Succeeded"]);
            LogNotes = Null.SetNullString(dr["LogNotes"]);
            Server = Null.SetNullString(dr["Server"]);
            FillInternal(dr);
        }
    }
}
