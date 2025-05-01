// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;

    using DotNetNuke.Abstractions.Logging;

    /// <content>The obsolete properties for <see cref="LogTypeConfigInfo"/>.</content>
    public partial class LogTypeConfigInfo
    {
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogTypeConfigInfo.Id' instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public string ID
        {
            get => ((ILogTypeConfigInfo)this).Id;
            set => ((ILogTypeConfigInfo)this).Id = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogTypeConfigInfo.LogTypePortalId' instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public string LogTypePortalID
        {
            get => ((ILogTypeConfigInfo)this).LogTypePortalId;
            set => ((ILogTypeConfigInfo)this).LogTypePortalId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant
    }
}
