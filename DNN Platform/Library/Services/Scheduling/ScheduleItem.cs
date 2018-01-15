#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

        public DateTime NextStart
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
                a = strObjectDependencies.ToLower().Split(',');
                int i;
                for (i = 0; i <= a.Length - 1; i++)
                {
                    if (ObjectDependencies.ToLower().IndexOf(a[i].Trim()) > -1)
                    {
                        return true;
                    }
                }
            }
            else if (ObjectDependencies.ToLower().IndexOf(strObjectDependencies.ToLower()) > -1)
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