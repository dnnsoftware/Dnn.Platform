// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services.Dto;

using System;
using System.Runtime.Serialization;

[DataContract]
public class WebServer
{
    [DataMember(Name = "serverId")]
    public int ServerId { get; set; }

    [DataMember(Name = "serverGroup")]
    public string ServerGroup { get; set; }

    [DataMember(Name = "serverName")]
    public string ServerName { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "lastActivityDate")]
    public DateTime LastActivityDate { get; set; }

    [DataMember(Name = "isActive")]
    public bool IsActive { get; set; }

    [DataMember(Name = "isCurrent")]
    public bool IsCurrent { get; set; }
}
