// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.DDRMenu.Localisation;

    public partial class MenuView : ModuleBase
    {
        private MenuBase menu;

        protected override void OnPreRender(EventArgs e)
        {
            using (new DNNContext(this))
            {
                try
                {
                    base.OnPreRender(e);

                    var menuStyle = this.GetStringSetting("MenuStyle");
                    if (string.IsNullOrEmpty(menuStyle))
                    {
                        this.menu = null;
                        return;
                    }

                    var menuSettings = new Settings
                    {
                        MenuStyle = this.GetStringSetting("MenuStyle"),
                        NodeXmlPath = this.GetStringSetting("NodeXmlPath"),
                        NodeSelector = this.GetStringSetting("NodeSelector"),
                        IncludeContext = this.GetBoolSetting("IncludeContext"),
                        IncludeHidden = this.GetBoolSetting("IncludeHidden"),
                        IncludeNodes = this.GetStringSetting("IncludeNodes"),
                        ExcludeNodes = this.GetStringSetting("ExcludeNodes"),
                        NodeManipulator = this.GetStringSetting("NodeManipulator"),
                        TemplateArguments =
                                            DDRMenu.Settings.TemplateArgumentsFromSettingString(this.GetStringSetting("TemplateArguments")),
                        ClientOptions =
                                            DDRMenu.Settings.ClientOptionsFromSettingString(this.GetStringSetting("ClientOptions")),
                    };

                    MenuNode rootNode = null;
                    if (string.IsNullOrEmpty(menuSettings.NodeXmlPath))
                    {
                        rootNode =
                            new MenuNode(
                                Localiser.LocaliseDNNNodeCollection(
                                    Navigation.GetNavigationNodes(
                                        this.ClientID,
                                        Navigation.ToolTipSource.None,
                                        -1,
                                        -1,
                                        DNNAbstract.GetNavNodeOptions(true))));
                    }

                    this.menu = MenuBase.Instantiate(menuStyle);
                    this.menu.RootNode = rootNode;
                    this.menu.ApplySettings(menuSettings);

                    this.menu.PreRender();
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }

        protected override void Render(HtmlTextWriter htmlWriter)
        {
            using (new DNNContext(this))
            {
                try
                {
                    base.Render(htmlWriter);
                    if (this.menu == null)
                    {
                        htmlWriter.WriteEncodedText("Please specify menu style in settings.");
                    }
                    else
                    {
                        this.menu.Render(htmlWriter);
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }
    }
}
