// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Tests.Core.Services.ClientCapability
{
    public class TestClientCapability : DotNetNuke.Services.ClientCapability.ClientCapability
    {
        public override string this[string name]
        {
            get
            {
                if (Capabilities.ContainsKey(name))
                {
                    return Capabilities[name];
                }

                return string.Empty;
            }
        }
    }
}
