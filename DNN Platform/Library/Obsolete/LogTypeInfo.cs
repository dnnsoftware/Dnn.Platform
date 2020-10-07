// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;

    public partial class LogTypeInfo
    {
        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogTypeCSSClass.LogTypeCSSClass' instead. Scheduled for removal in v11.0.0.")]
        public string LogTypeCSSClass
        {
            get => this.LogTypeCssClass;
            set => this.LogTypeCssClass = value;
        }
    }
}
