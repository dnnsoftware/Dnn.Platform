#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Web.UI;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.UI
{
    public class ControlUtilities
    {
        public static T FindParentControl<T>(Control control) where T : Control
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

        public static T FindControl<T>(Control control, string id, bool recursive) where T : Control
        {
            T target = null;
            if(control.Parent != null)
            {
                target = control.Parent.FindControl(id) as T;

                if (target == null && recursive)
                {
                    target = FindControl<T>(control.Parent, id, true);
                }
            }
 
            return target;
        }

        public static T FindFirstDescendent<T>(Control control)  where T : Control
        {
          return FindFirstDescendent<T>(control, idx => idx is T);
        }

        public static T FindFirstDescendent<T>(Control control, Predicate<Control> predicate) where T : Control
        {
          if (predicate(control)) return control as T;

          foreach (Control childControl in control.Controls) 
          {
            T descendent = FindFirstDescendent<T>(childControl, predicate); 
            if (descendent != null)      
              return descendent; 
          }

          return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadControl loads a control and returns a reference to the control
        /// </summary>
        /// <typeparam name="T">The type of control to Load</typeparam>
        /// <param name="containerControl">The parent Container Control</param>
        /// <param name="ControlSrc">The source for the control.  This can either be a User Control (.ascx) or a compiled
        /// control.</param>
        /// <returns>A Control of type T</returns>
        /// -----------------------------------------------------------------------------
        public static T LoadControl<T>(TemplateControl containerControl, string ControlSrc) where T : Control
        {
            T ctrl;

            //load the control dynamically
            if (ControlSrc.ToLower().EndsWith(".ascx"))
            {
				//load from a user control on the file system
                ctrl = (T) containerControl.LoadControl("~/" + ControlSrc);
            }
            else
            {
				//load from a typename in an assembly ( ie. server control )
                Type objType = Reflection.CreateType(ControlSrc);
                ctrl = (T) containerControl.LoadControl(objType, null);
            }
            return ctrl;
        }
    }
}
