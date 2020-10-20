﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Log.EventLog
{
    public interface ILogViewer
    {
        string EventFilter { get; set; }

        void BindData();
    }
}
