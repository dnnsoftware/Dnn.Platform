// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.NewDDRMenu
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI;
    using DotNetNuke.Web.DDRMenu;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using DotNetNuke.Web.NewDDRMenu.DNNCommon;

    using Microsoft.Extensions.DependencyInjection;

    public partial class MenuView : ModuleBase
    {
        private readonly ILocaliser localiser;
        private MenuBase menu;

        /// <summary>Initializes a new instance of the <see cref="MenuView"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with ILocaliser. Scheduled removal in v12.0.0.")]
        public MenuView()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MenuView"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        public MenuView(ILocaliser localiser)
        {
            this.localiser = localiser ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocaliser>();
        }

        /// <inheritdoc/>
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
                                            NewDDRMenu.Settings.TemplateArgumentsFromSettingString(this.GetStringSetting("TemplateArguments")),
                        ClientOptions =
                                            NewDDRMenu.Settings.ClientOptionsFromSettingString(this.GetStringSetting("ClientOptions")),
                    };

                    MenuNode rootNode = null;
                    if (string.IsNullOrEmpty(menuSettings.NodeXmlPath))
                    {
                        rootNode =
                            new MenuNode(
                                this.localiser.LocaliseDNNNodeCollection(
                                    Navigation.GetNavigationNodes(
                                        this.ClientID,
                                        Navigation.ToolTipSource.None,
                                        -1,
                                        -1,
                                        DNNAbstract.GetNavNodeOptions(true))));
                    }

                    this.menu = MenuBase.Instantiate(this.localiser, menuStyle);
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

        /// <inheritdoc/>
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
