// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;

    public partial class LogTypeConfigInfo
    {
        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogTypeConfigInfo.Id' instead. Scheduled for removal in v11.0.0.")]
        public string ID
        {
            get => this.Id;
            set => this.Id = value;
        }

        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogTypeConfigInfo.LogTypePortalID' instead. Scheduled for removal in v11.0.0.")]
        public string LogTypePortalID
        {
            get => this.LogTypePortalId;
            set => this.LogTypePortalId = value;
        }
    }
}
