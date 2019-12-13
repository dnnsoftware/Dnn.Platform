// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities;

using Moq.Language.Flow;

namespace DotNetNuke.Tests.Providers.Caching
{
    public static class MoqExtensions
    {
        public static IReturnsResult<CachingProvider> ReturnsValidValue(this ISetup<CachingProvider, object> ret)
        {
            return ret.Returns(() => Constants.CACHEING_ValidValue);
        }
    }
}
