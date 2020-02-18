// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

namespace DotNetNuke.Common.Internal
{
    internal interface IEventHandlersContainer<T>
    {
        IEnumerable<Lazy<T>> EventHandlers { get; }
    }
}
