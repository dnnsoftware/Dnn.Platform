// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    [Serializable]
    public class ScheduleItem : BaseEntityInfo, IHydratable
    {
        private static readonly DateTime MinNextTime = DateTime.Now;
        private DateTime? _NextStart;
        private Hashtable _ScheduleItemSettings;

        public ScheduleItem()
        {
            this.ScheduleID = Null.NullInteger;
            this.TypeFullName = Null.NullString;
            this.TimeLapse = Null.NullInteger;
            this.TimeLapseMeasurement = Null.NullString;
            this.RetryTimeLapse = Null.NullInteger;
            this.RetryTimeLapseMeasurement = Null.NullString;
            this.ObjectDependencies = Null.NullString;
            this.RetainHistoryNum = Null.NullInteger;
            this.CatchUpEnabled = Null.NullBoolean;
            this.Enabled = Null.NullBoolean;
            this.AttachToEvent = Null.NullString;
            this.ThreadID = Null.NullInteger;
            this.ProcessGroup = Null.NullInteger;
            this.Servers = Null.NullString;
            this.ScheduleStartDate = Null.NullDate;
        }

        public string AttachToEvent { get; set; }

        public bool CatchUpEnabled { get; set; }

        public bool Enabled { get; set; }

        public DateTime ScheduleStartDate { get; set; }

        public string FriendlyName { get; set; }

        public virtual DateTime NextStart
        {
            get
            {
                if (!this._NextStart.HasValue)
                {
                    this._NextStart = MinNextTime;
                }

                return this._NextStart.Value > MinNextTime ? this._NextStart.Value : MinNextTime;
            }

            set
            {
                this._NextStart = value;
            }
        }

        public string ObjectDependencies { get; set; }

        public int RetainHistoryNum { get; set; }

        public int RetryTimeLapse { get; set; }

        public string RetryTimeLapseMeasurement { get; set; }

        public int ScheduleID { get; set; }

        public string Servers { get; set; }

        public int TimeLapse { get; set; }

        public string TimeLapseMeasurement { get; set; }

        public string TypeFullName { get; set; }

        public int ProcessGroup { get; set; }

        public ScheduleSource ScheduleSource { get; set; }

        public int ThreadID { get; set; }

        public int KeyID
        {
            get
            {
                return this.ScheduleID;
            }

            set
            {
                this.ScheduleID = value;
            }
        }

        public virtual void Fill(IDataReader dr)
        {
            this.FillInternal(dr);
        }

        public bool HasObjectDependencies(string strObjectDependencies)
        {
            if (strObjectDependencies.IndexOf(",") > -1)
            {
                string[] a;
                a = strObjectDependencies.ToLowerInvariant().Split(',');
                int i;
                for (i = 0; i <= a.Length - 1; i++)
                {
                    if (this.ObjectDependencies.IndexOf(a[i].Trim(), StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return true;
                    }
                }
            }
            else if (this.ObjectDependencies.IndexOf(strObjectDependencies, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return true;
            }

            return false;
        }

        public void AddSetting(string Key, string Value)
        {
            this._ScheduleItemSettings.Add(Key, Value);
        }

        public virtual string GetSetting(string Key)
        {
            if (this._ScheduleItemSettings == null)
            {
                this.GetSettings();
            }

            if (this._ScheduleItemSettings != null && this._ScheduleItemSettings.ContainsKey(Key))
            {
                return Convert.ToString(this._ScheduleItemSettings[Key]);
            }
            else
            {
                return string.Empty;
            }
        }

        public virtual Hashtable GetSettings()
        {
            this._ScheduleItemSettings = SchedulingProvider.Instance().GetScheduleItemSettings(this.ScheduleID);
            return this._ScheduleItemSettings;
        }

        protected override void FillInternal(IDataReader dr)
        {
            this.ScheduleID = Null.SetNullInteger(dr["ScheduleID"]);
            this.FriendlyName = Null.SetNullString(dr["FriendlyName"]);
            this.TypeFullName = Null.SetNullString(dr["TypeFullName"]);
            this.TimeLapse = Null.SetNullInteger(dr["TimeLapse"]);
            this.TimeLapseMeasurement = Null.SetNullString(dr["TimeLapseMeasurement"]);
            this.RetryTimeLapse = Null.SetNullInteger(dr["RetryTimeLapse"]);
            this.RetryTimeLapseMeasurement = Null.SetNullString(dr["RetryTimeLapseMeasurement"]);
            this.ObjectDependencies = Null.SetNullString(dr["ObjectDependencies"]);
            this.AttachToEvent = Null.SetNullString(dr["AttachToEvent"]);
            this.RetainHistoryNum = Null.SetNullInteger(dr["RetainHistoryNum"]);
            this.CatchUpEnabled = Null.SetNullBoolean(dr["CatchUpEnabled"]);
            this.Enabled = Null.SetNullBoolean(dr["Enabled"]);
            this.Servers = Null.SetNullString(dr["Servers"]);

            var schema = dr.GetSchemaTable();
            if (schema != null)
            {
                if (schema.Select("ColumnName = 'NextStart'").Length > 0)
                {
                    this.NextStart = Null.SetNullDateTime(dr["NextStart"]);
                }

                if (schema.Select("ColumnName = 'ScheduleStartDate'").Length > 0)
                {
                    this.ScheduleStartDate = Null.SetNullDateTime(dr["ScheduleStartDate"]);
                }
            }

            // Fill BaseEntityInfo
            base.FillInternal(dr);
        }
    }
}
