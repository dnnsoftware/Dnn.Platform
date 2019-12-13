using System;
using System.Collections.Generic;

namespace DotNetNuke.Common.Internal
{
    internal interface IEventHandlersContainer<T>
    {
        IEnumerable<Lazy<T>> EventHandlers { get; }
    }
}
