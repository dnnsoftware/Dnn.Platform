// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.Log.EventLog
{
    public class LogQueueItem
    {
        public LogInfo LogInfo { get; set; }

        public LogTypeConfigInfo LogTypeConfigInfo { get; set; }
    }
}
