﻿#region Copyright
// 
// DotNetNuke® - https://www.dnnsoftware.com
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

using System;
using System.Web.UI;
using DotNetNuke.Framework;

namespace DotNetNuke.Common.Utilities
{
    public class JavaScriptUtils: ServiceLocator<IJavaScriptUtils,JavaScriptUtils>, IJavaScriptUtils
    {
        public void RegisterJavascriptVariable(string variableName, object value, Page page, Type type)
        {
            var valueAsJson = Json.Serialize(value);

            var script = string.Format("var {0} = {1};", variableName, valueAsJson);

            if (ScriptManager.GetCurrent(page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(page, type, variableName, script, true);
            }
            else
            {
                page.ClientScript.RegisterStartupScript(type, variableName, script, true);
            }
        }

        protected override Func<IJavaScriptUtils> GetFactory()
        {
            return () => new JavaScriptUtils();
        }
    }
}
