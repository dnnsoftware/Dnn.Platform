using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IExtensionControlRenderer
    {
        string GetOutput(IExtensionPoint extension);
    }
}
