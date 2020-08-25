// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Framework;

    public class ControlUtilities
    {
        public static T FindParentControl<T>(Control control)
            where T : Control
        {
            T parent = default(T);
            if (control.Parent == null)
            {
                parent = null;
            }
            else
            {
                var parentT = control.Parent as T;
                if (parentT != null)
                {
                    parent = parentT;
                }
                else
                {
                    parent = FindParentControl<T>(control.Parent);
                }
            }

            return parent;
        }

        public static T FindControl<T>(Control control, string id, bool recursive)
            where T : Control
        {
            T target = null;
            if (control.Parent != null)
            {
                target = control.Parent.FindControl(id) as T;

                if (target == null && recursive)
                {
                    target = FindControl<T>(control.Parent, id, true);
                }
            }

            return target;
        }

        public static T FindFirstDescendent<T>(Control control)
            where T : Control
        {
            return FindFirstDescendent<T>(control, idx => idx is T);
        }

        public static T FindFirstDescendent<T>(Control control, Predicate<Control> predicate)
            where T : Control
        {
            if (predicate(control))
            {
                return control as T;
            }

            foreach (Control childControl in control.Controls)
            {
                T descendent = FindFirstDescendent<T>(childControl, predicate);
                if (descendent != null)
                {
                    return descendent;
                }
            }

            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadControl loads a control and returns a reference to the control.
        /// </summary>
        /// <typeparam name="T">The type of control to Load.</typeparam>
        /// <param name="containerControl">The parent Container Control.</param>
        /// <param name="ControlSrc">The source for the control.  This can either be a User Control (.ascx) or a compiled
        /// control.</param>
        /// <returns>A Control of type T.</returns>
        /// -----------------------------------------------------------------------------
        public static T LoadControl<T>(TemplateControl containerControl, string ControlSrc)
            where T : Control
        {
            T ctrl;

            // load the control dynamically
            if (ControlSrc.EndsWith(".ascx", StringComparison.InvariantCultureIgnoreCase))
            {
                // load from a user control on the file system
                ctrl = (T)containerControl.LoadControl("~/" + ControlSrc);
            }
            else
            {
                // load from a typename in an assembly ( ie. server control )
                Type objType = Reflection.CreateType(ControlSrc);
                ctrl = (T)containerControl.LoadControl(objType, null);
            }

            return ctrl;
        }
    }
}
