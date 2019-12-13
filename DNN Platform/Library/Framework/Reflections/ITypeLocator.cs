using System;
using System.Collections.Generic;

namespace DotNetNuke.Framework.Reflections
{
    public interface ITypeLocator
    {
        IEnumerable<Type> GetAllMatchingTypes(Predicate<Type> predicate);
    }
}
