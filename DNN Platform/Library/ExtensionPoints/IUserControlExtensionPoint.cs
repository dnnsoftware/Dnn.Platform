using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IUserControlExtensionPoint : IExtensionPoint
    {
        string UserControlSrc { get; }
        bool Visible { get; }
    }
}
