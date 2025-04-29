// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System;

using DotNetNuke.Abstractions.Logging;

public partial class LogTypeInfo
{
    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogTypeInfo.LogTypeCssClass' instead. Scheduled removal in v11.0.0.")]
    public string LogTypeCSSClass
    {
        get => ((ILogTypeInfo)this).LogTypeCssClass;
        set => ((ILogTypeInfo)this).LogTypeCssClass = value;
    }
}
