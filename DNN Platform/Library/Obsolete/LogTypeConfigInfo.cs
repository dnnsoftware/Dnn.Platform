// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;

    using DotNetNuke.Abstractions.Logging;

    public partial class LogTypeConfigInfo
    {
        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogTypeConfigInfo.Id' instead. Scheduled for removal in v11.0.0.")]
        public string ID
        {
            get => ((ILogTypeConfigInfo)this).Id;
            set => ((ILogTypeConfigInfo)this).Id = value;
        }

        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogTypeConfigInfo.LogTypePortalId' instead. Scheduled for removal in v11.0.0.")]
        public string LogTypePortalID
        {
            get => ((ILogTypeConfigInfo)this).LogTypePortalId;
            set => ((ILogTypeConfigInfo)this).LogTypePortalId = value;
        }
    }
}
