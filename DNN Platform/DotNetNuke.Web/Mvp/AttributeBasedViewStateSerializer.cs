#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Linq;
using System.Reflection;
using System.Web.UI;

#endregion

namespace DotNetNuke.Web.Mvp
{
    public class AttributeBasedViewStateSerializer
    {
        private const BindingFlags MemberBindingFlags = BindingFlags.Instance | BindingFlags.Public;

        public static void DeSerialize(object value, StateBag state)
        {
            Type typ = value.GetType();

            //Parse all the Public instance properties
            foreach (PropertyInfo member in typ.GetProperties(MemberBindingFlags))
            {
                //Determine if they are attributed with a ViewState Attribute
                ViewStateAttribute attr = member.GetCustomAttributes(typeof (ViewStateAttribute), true).OfType<ViewStateAttribute>().FirstOrDefault();
                if ((attr != null))
                {
                    //Get object from ViewState bag
                    string viewStateKey = attr.ViewStateKey;
                    if (string.IsNullOrEmpty(viewStateKey))
                    {
                        //Use class member's name for Key
                        viewStateKey = member.Name;
                    }

                    member.SetValue(value, state[viewStateKey], null);
                }
            }
        }

        public static void Serialize(object value, StateBag state)
        {
            Type typ = value.GetType();

            //Parse all the Public instance properties
            foreach (PropertyInfo member in typ.GetProperties(MemberBindingFlags))
            {
                //Determine if they are attributed with a ViewState Attribute
                ViewStateAttribute attr = member.GetCustomAttributes(typeof (ViewStateAttribute), true).OfType<ViewStateAttribute>().FirstOrDefault();
                if ((attr != null))
                {
                    //Add property to ViewState bag
                    string viewStateKey = attr.ViewStateKey;
                    if (string.IsNullOrEmpty(viewStateKey))
                    {
                        //Use class member's name for Key
                        viewStateKey = member.Name;
                    }

                    state[viewStateKey] = member.GetValue(value, null);
                }
            }
        }
    }
}