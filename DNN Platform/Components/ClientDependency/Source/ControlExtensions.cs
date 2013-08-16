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
            var children = control.Controls.Cast<Control>();
            return children.SelectMany(c => FlattenChildren(c)).Concat(children);
        }

        public static IEnumerable<Control> FlattenChildren<T>(this Control control)
        {
            var children = control.Controls
                .Cast<Control>()
                .Where(x => x is T);
            return children.SelectMany(c => FlattenChildren(c)).Concat(children);
        }



    }
}
