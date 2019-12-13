// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.Localization.Internal
{
    public class TestableLocalization : ServiceLocator<ILocalization, TestableLocalization>
    {
        protected override Func<ILocalization> GetFactory()
        {
            return () => new LocalizationImpl();
        }
    }
}
