﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.ControlKey = "";
            this.ModuleFriendlyName = "";
            this.LinkWindowTarget = "";
            this.ToolName = "";
        }

        public RibbonBarToolInfo(string toolName, bool isHostTool, bool useButton, string linkWindowTarget, string moduleFriendlyName, string controlKey, bool showAsPopUp)
        {
            this.ToolName = toolName;
            this.IsHostTool = isHostTool;
            this.UseButton = useButton;
            this.LinkWindowTarget = linkWindowTarget;
            this.ModuleFriendlyName = moduleFriendlyName;
            this.ControlKey = controlKey;
            this.ShowAsPopUp = showAsPopUp;
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
