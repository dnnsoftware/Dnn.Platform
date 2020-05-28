// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Xml.Serialization;

#endregion

namespace DotNetNuke.Services.EventQueue.Config
{
    [Serializable]
    public class PublishedEvent
    {
        public string EventName { get; set; }

        public string Subscribers { get; set; }
    }
}
