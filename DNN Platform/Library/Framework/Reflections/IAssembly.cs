using System;

namespace DotNetNuke.Framework.Internal.Reflection
{
    //interface to allowing mocking of System.Reflection.Assembly
    public interface IAssembly
    {
        Type[] GetTypes();
    }
}
