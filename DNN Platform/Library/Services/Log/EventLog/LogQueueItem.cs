// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Log.EventLog
{
    public class LogQueueItem
    {
        public LogInfo LogInfo { get; set; }

        public LogTypeConfigInfo LogTypeConfigInfo { get; set; }
    }
}
