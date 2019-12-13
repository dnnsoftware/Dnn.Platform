using System.Collections.Generic;

namespace DotNetNuke.Framework.Internal.Reflection
{
    public interface IAssemblyLocator
    {
        IEnumerable<IAssembly> Assemblies { get; }
    }
}
