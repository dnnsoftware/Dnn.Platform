// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Scheduling
{
    [Serializable]
    public class ScheduleItem : BaseEntityInfo, IHydratable
    {
        #region Private Members

        private static readonly DateTime MinNextTime = DateTime.Now; 
        private DateTime? _NextStart;
        private Hashtable _ScheduleItemSettings;

        #endregion

        #region Constructors

        public ScheduleItem()
        {
            ScheduleID = Null.NullInteger;
            TypeFullName = Null.NullString;
            TimeLapse = Null.NullInteger;
            TimeLapseMeasurement = Null.NullString;
            RetryTimeLapse = Null.NullInteger;
            RetryTimeLapseMeasurement = Null.NullString;
            ObjectDependencies = Null.NullString;
            RetainHistoryNum = Null.NullInteger;
            CatchUpEnabled = Null.NullBoolean;
            Enabled = Null.NullBoolean;
            AttachToEvent = Null.NullString;
            ThreadID = Null.NullInteger;
            ProcessGroup = Null.NullInteger;
            Servers = Null.NullString;
            ScheduleStartDate = Null.NullDate;
        }

        #endregion

        #region Persisted Properties

        public string AttachToEvent { get; set; }

        public bool CatchUpEnabled { get; set; }

        public bool Enabled { get; set; }

        public DateTime ScheduleStartDate { get; set; }

        public string FriendlyName { get; set; }

        public virtual DateTime NextStart
        {
            get
            {
                if (!_NextStart.HasValue)
                {
                    _NextStart = MinNextTime;
                }
                return _NextStart.Value > MinNextTime ? _NextStart.Value : MinNextTime;
            }
            set
            {
                _NextStart = value;
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

        #endregion

        #region IHydratable Members

        public int KeyID
        {
            get
            {
                return ScheduleID;
            }
            set
            {
                ScheduleID = value;
            }
        }

        public virtual void Fill(IDataReader dr)
        {
            FillInternal(dr);
        }

        #endregion

        public bool HasObjectDependencies(string strObjectDependencies)
        {
            if (strObjectDependencies.IndexOf(",") > -1)
            {
                string[] a;
                a = strObjectDependencies.ToLowerInvariant().Split(',');
                int i;
                for (i = 0; i <= a.Length - 1; i++)
                {
                    if (ObjectDependencies.IndexOf(a[i].Trim(), StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return true;
                    }
                }
            }
            else if (ObjectDependencies.IndexOf(strObjectDependencies, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return true;
            }
            return false;
        }

        #region "Public Methods"

        public void AddSetting(string Key, string Value)
        {
            _ScheduleItemSettings.Add(Key, Value);
        }

        public virtual string GetSetting(string Key)
        {
            if (_ScheduleItemSettings == null)
            {
                GetSettings();
            }
            if (_ScheduleItemSettings != null && _ScheduleItemSettings.ContainsKey(Key))
            {
                return Convert.ToString(_ScheduleItemSettings[Key]);
            }
            else
            {
                return "";
            }
        }

        public virtual Hashtable GetSettings()
        {
            _ScheduleItemSettings = SchedulingProvider.Instance().GetScheduleItemSettings(ScheduleID);
            return _ScheduleItemSettings;
        }

        protected override void FillInternal(IDataReader dr)
        {
            ScheduleID = Null.SetNullInteger(dr["ScheduleID"]);
            FriendlyName = Null.SetNullString(dr["FriendlyName"]);
            TypeFullName = Null.SetNullString(dr["TypeFullName"]);
            TimeLapse = Null.SetNullInteger(dr["TimeLapse"]);
            TimeLapseMeasurement = Null.SetNullString(dr["TimeLapseMeasurement"]);
            RetryTimeLapse = Null.SetNullInteger(dr["RetryTimeLapse"]);
            RetryTimeLapseMeasurement = Null.SetNullString(dr["RetryTimeLapseMeasurement"]);
            ObjectDependencies = Null.SetNullString(dr["ObjectDependencies"]);
            AttachToEvent = Null.SetNullString(dr["AttachToEvent"]);
            RetainHistoryNum = Null.SetNullInteger(dr["RetainHistoryNum"]);
            CatchUpEnabled = Null.SetNullBoolean(dr["CatchUpEnabled"]);
            Enabled = Null.SetNullBoolean(dr["Enabled"]);
            Servers = Null.SetNullString(dr["Servers"]);

            var schema = dr.GetSchemaTable();
            if (schema != null)
            {
                if (schema.Select("ColumnName = 'NextStart'").Length > 0)
                {
                    NextStart = Null.SetNullDateTime(dr["NextStart"]);
                }
                if (schema.Select("ColumnName = 'ScheduleStartDate'").Length > 0)
                {
                    ScheduleStartDate = Null.SetNullDateTime(dr["ScheduleStartDate"]);
                }
            }
            //Fill BaseEntityInfo
            base.FillInternal(dr);
        }

        #endregion
    }
}
