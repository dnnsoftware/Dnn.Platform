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
