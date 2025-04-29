// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls;

using System;

using DotNetNuke.Instrumentation;

/// Project:    DotNetNuke
/// Namespace:  DotNetNuke.UI.WebControls
/// Class:      SettingInfo
/// <summary>The SettingInfo class provides a helper class for the Settings Editor.</summary>
public class SettingInfo
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SettingInfo));
    private Type type;

    /// <summary>Initializes a new instance of the <see cref="SettingInfo"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public SettingInfo(object name, object value)
    {
        this.Name = Convert.ToString(name);
        this.Value = value;
        this.type = value.GetType();
        this.Editor = EditorInfo.GetEditor(-1);
        string strValue = Convert.ToString(value);
        bool isFound = false;
        if (this.type.IsEnum)
        {
            isFound = true;
        }

        if (!isFound)
        {
            try
            {
                bool boolValue = bool.Parse(strValue);
                this.Editor = EditorInfo.GetEditor("Checkbox");
                isFound = true;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

        if (!isFound)
        {
            try
            {
                int intValue = int.Parse(strValue);
                this.Editor = EditorInfo.GetEditor("Integer");
                isFound = true;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }
    }

    public string Name { get; set; }

    public object Value { get; set; }

    public string Editor { get; set; }

    public Type Type
    {
        get
        {
            return this.type;
        }

        set
        {
            this.type = value;
        }
    }
}
