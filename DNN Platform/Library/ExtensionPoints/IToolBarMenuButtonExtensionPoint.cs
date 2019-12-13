using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.ExtensionPoints
{
    public interface IToolBarMenuButtonExtensionPoint: IToolBarButtonExtensionPoint
    {
        List<IMenuButtonItemExtensionPoint> Items { get; }
        string MenuCssClass { get; }
    }
}
