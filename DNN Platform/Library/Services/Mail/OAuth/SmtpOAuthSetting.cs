// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail.OAuth;

using System;
using System.Runtime.Serialization;

/// <summary>Smtp auth setting.</summary>
[DataContract]
public class SmtpOAuthSetting
{
    /// <summary>Gets or sets the setting name.</summary>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary>Gets or sets the setting value.</summary>
    [DataMember(Name = "value")]
    public string Value { get; set; }

    /// <summary>Gets or sets the setting label.</summary>
    [DataMember(Name = "label")]
    public string Label { get; set; }

    /// <summary>Gets or sets the setting help text.</summary>
    [DataMember(Name = "help")]
    public string Help { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a secure setting.</summary>
    [DataMember(Name = "isSecure")]
    public bool IsSecure { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a required setting.</summary>
    [DataMember(Name = "isRequired")]
    public bool IsRequired { get; set; }

    /// <summary>Gets or sets a value indicating whether this setting is only used in server side.</summary>
    [IgnoreDataMember]
    public bool IsBackground { get; set; }
}
