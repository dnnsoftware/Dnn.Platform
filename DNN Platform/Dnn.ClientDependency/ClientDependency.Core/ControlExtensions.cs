using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ClientDependency.Core.Controls;
using System.Web;

namespace ClientDependency.Core
{
    public static class ControlExtensions
    {

        public static IEnumerable<Control> FlattenChildren(this Control control)
        {
            var children = control.Controls.Cast<Control>().ToArray();
            return children.SelectMany(FlattenChildren).Concat(children);
        }

    }
}
