// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
