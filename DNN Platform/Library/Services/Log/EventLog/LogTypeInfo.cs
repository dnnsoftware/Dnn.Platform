// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

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
