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

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [Serializable]
    public class RibbonBarToolInfo
    {
        public RibbonBarToolInfo()
        {
            ControlKey = "";
            ModuleFriendlyName = "";
            LinkWindowTarget = "";
            ToolName = "";
        }

        public RibbonBarToolInfo(string toolName, bool isHostTool, bool useButton, string linkWindowTarget, string moduleFriendlyName, string controlKey, bool showAsPopUp)
        {
            ToolName = toolName;
            IsHostTool = isHostTool;
            UseButton = useButton;
            LinkWindowTarget = linkWindowTarget;
            ModuleFriendlyName = moduleFriendlyName;
            ControlKey = controlKey;
            ShowAsPopUp = showAsPopUp;
        }

        public string ControlKey { get; set; }

        public bool IsHostTool { get; set; }

        public string LinkWindowTarget { get; set; }

        public string ModuleFriendlyName { get; set; }

        public bool ShowAsPopUp { get; set; }

        public string ToolName { get; set; }

        public bool UseButton { get; set; }
    }
}