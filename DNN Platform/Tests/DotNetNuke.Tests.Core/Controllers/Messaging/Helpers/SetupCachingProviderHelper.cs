using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using Moq;

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Helpers
{
    public class SetupCachingProviderHelper
    {
        public static void SetupCachingProvider(Mock<CachingProvider> mockCachingProvider)
        {
            mockCachingProvider.Setup(c => c.GetItem(It.IsAny<string>()))
                .Returns<string>((key =>
                {
                    if (key.Contains("Portal-1_"))
                    {
                        var portals = new List<PortalInfo> { new PortalInfo() { PortalID = 0 } };

                        return portals;
                    }
                    return key.Contains("PortalGroups") ? new List<PortalGroupInfo>() : null;
                }));
        }
    }
}
