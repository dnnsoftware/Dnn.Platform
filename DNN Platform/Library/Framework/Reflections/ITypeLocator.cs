// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

namespace DotNetNuke.Framework.Reflections
{
    public interface ITypeLocator
    {
        IEnumerable<Type> GetAllMatchingTypes(Predicate<Type> predicate);
    }
}
