// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.ClientCapability
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TestClientCapability : DotNetNuke.Services.ClientCapability.ClientCapability
    {
        public override string this[string name]
        {
            get
            {
                if (this.Capabilities.ContainsKey(name))
                {
                    return this.Capabilities[name];
                }

                return string.Empty;
            }
        }
    }
}
