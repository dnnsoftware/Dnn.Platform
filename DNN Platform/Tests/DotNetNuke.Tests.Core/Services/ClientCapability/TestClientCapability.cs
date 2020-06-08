// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
