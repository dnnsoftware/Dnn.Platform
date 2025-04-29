// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Objects;

using System;

/// <summary>A Editor Host Setting Item.</summary>
[Serializable]
public class EditorHostSetting
{
    /// <summary>Initializes a new instance of the <see cref="EditorHostSetting"/> class.</summary>
    /// <param name="settingName">Name of the setting.</param>
    /// <param name="settingValue">The setting value.</param>
    public EditorHostSetting(string settingName, string settingValue)
    {
        this.Name = settingName;
        this.Value = settingValue;
    }

    /// <summary>Gets or sets the setting name.</summary>
    /// <value>
    /// The setting name.
    /// </value>
    public string Name { get; set; }

    /// <summary>Gets or sets the setting value.</summary>
    /// <value>
    /// The setting value.
    /// </value>
    public string Value { get; set; }
}
