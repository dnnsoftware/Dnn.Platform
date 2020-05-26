// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Framework;

namespace DotNetNuke.Common.Internal
{
    public class TestableGlobals : ServiceLocator<IGlobals, TestableGlobals>
    {
        protected override Func<IGlobals> GetFactory()
        {
            return () => new GlobalsImpl();
        }
    }
}
