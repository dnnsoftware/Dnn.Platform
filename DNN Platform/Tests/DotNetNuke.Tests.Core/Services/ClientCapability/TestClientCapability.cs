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
