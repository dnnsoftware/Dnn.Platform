// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

using System;

namespace Dnn.PersonaBar.Servers.Components.Log
{
    public class LogFile
    {
        public string Name { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public string Size { get; set; }
    }
}