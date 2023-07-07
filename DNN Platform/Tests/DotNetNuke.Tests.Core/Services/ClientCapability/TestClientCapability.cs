// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.ClientCapability
{
    using System.Collections.Generic;

    public class TestClientCapability : DotNetNuke.Services.ClientCapability.ClientCapability
    {
        public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public override string this[string name]
        {
            get
            {
                return this.Properties.TryGetValue(name, out var value) ? value : string.Empty;
            }
        }
    }
}
