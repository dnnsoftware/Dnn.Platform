// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling;

using System;
using System.Collections;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

/// <summary>Represents one item in the scheduler.</summary>
[Serializable]
public class ScheduleItem : BaseEntityInfo, IHydratable
{
    private static readonly DateTime MinNextTime = DateTime.Now;
    private DateTime? nextStart;
    private Hashtable scheduleItemSettings;

    /// <summary>Initializes a new instance of the <see cref="ScheduleItem"/> class.</summary>
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

    /// <summary>Gets or sets the the event this item attaches to.</summary>
    public string AttachToEvent { get; set; }

    /// <summary>Gets or sets a value indicating whether cath-up is enabled.</summary>
    public bool CatchUpEnabled { get; set; }

    /// <summary>Gets or sets a value indicating whether the item is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Gets or sets the schedule start date.</summary>
    public DateTime ScheduleStartDate { get; set; }

    /// <summary>Gets or sets the friednly name for the item.</summary>
    public string FriendlyName { get; set; }

    /// <summary>Gets or sets the next start date.</summary>
    public virtual DateTime NextStart
    {
        get
        {
            if (!this.nextStart.HasValue)
            {
                this.nextStart = MinNextTime;
            }

            return this.nextStart.Value > MinNextTime ? this.nextStart.Value : MinNextTime;
        }

        set
        {
            this.nextStart = value;
        }
    }

    /// <summary>Gets or sets the object dependencies.</summary>
    public string ObjectDependencies { get; set; }

    /// <summary>Gets or sets a value indicating how many history items to keep.</summary>
    public int RetainHistoryNum { get; set; }

    /// <summary>Gets or sets the retry time lapse value.</summary>
    public int RetryTimeLapse { get; set; }

    /// <summary>Gets or sets the unit of measure for the retry time lapse value.</summary>
    public string RetryTimeLapseMeasurement { get; set; }

    /// <summary>Gets or sets the ID of the scheduled item.</summary>
    public int ScheduleID { get; set; }

    /// <summary>Gets or sets the servers this task should run on.</summary>
    public string Servers { get; set; }

    /// <summary>Gets or sets the recurrence time lapse value.</summary>
    public int TimeLapse { get; set; }

    /// <summary>Gets or sets the unit of measure for the recurrence time lapse value.</summary>
    public string TimeLapseMeasurement { get; set; }

    /// <summary>Gets or sets the full type name.</summary>
    public string TypeFullName { get; set; }

    /// <summary>Gets or sets the process group.</summary>
    public int ProcessGroup { get; set; }

    /// <summary>Gets or sets the <see cref="ScheduleSource"/>.</summary>
    public ScheduleSource ScheduleSource { get; set; }

    /// <summary>Gets or sets the ID of the running thread.</summary>
    public int ThreadID { get; set; }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public virtual void Fill(IDataReader dr)
    {
        this.FillInternal(dr);
    }

    /// <summary>Gets or sets a value indicating whether the item has object dependencies.</summary>
    /// <param name="strObjectDependencies">A string representing the name of the object dependencies.</param>
    /// <returns>A value indicating whether the item has object dependencies.</returns>
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

    /// <summary>Adds a schedule item setting value.</summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value of the setting.</param>
    public void AddSetting(string key, string value)
    {
        this.scheduleItemSettings.Add(key, value);
    }

    /// <summary>Gets a specific setting.</summary>
    /// <param name="key">The key of the setting to get.</param>
    /// <returns>The value of the setting.</returns>
    public virtual string GetSetting(string key)
    {
        if (this.scheduleItemSettings == null)
        {
            this.GetSettings();
        }

        if (this.scheduleItemSettings != null && this.scheduleItemSettings.ContainsKey(key))
        {
            return Convert.ToString(this.scheduleItemSettings[key]);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>Gets all the item settings.</summary>
    /// <returns>An <see cref="Hashtable"/> of all the settings.</returns>
    public virtual Hashtable GetSettings()
    {
        this.scheduleItemSettings = SchedulingProvider.Instance().GetScheduleItemSettings(this.ScheduleID);
        return this.scheduleItemSettings;
    }

    /// <inheritdoc/>
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
