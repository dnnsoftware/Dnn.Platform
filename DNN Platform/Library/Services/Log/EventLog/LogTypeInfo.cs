
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DotNetNuke.Services.Log.EventLog
{
    [Serializable]
    public class LogTypeInfo
    {
        public string LogTypeCSSClass { get; set; }

        public string LogTypeDescription { get; set; }

        public string LogTypeFriendlyName { get; set; }

        public string LogTypeKey { get; set; }

        public string LogTypeOwner { get; set; }
    }
}
