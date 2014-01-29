#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.ProfessionalPreview
{
    public partial class ProfessionalPreview : PortalModuleBase
    {
        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            previewText.Text = Localization.GetString(ModuleConfiguration.ModuleTitle, LocalResourceFile);
            previewFrame.Attributes["src"] = "http://www.dotnetnuke.com/Products/CompareDotNetNukeEditions/tabid/2175/Default.aspx#" + ModuleConfiguration.ModuleTitle.Replace(" ", string.Empty);
            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("FeatureNotAvailable", LocalResourceFile), ModuleMessage.ModuleMessageType.BlueInfo);
        }
    }
}