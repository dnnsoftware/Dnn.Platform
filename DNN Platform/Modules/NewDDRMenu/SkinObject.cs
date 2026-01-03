// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.NewDDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.DDRMenu;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using DotNetNuke.Web.NewDDRMenu.DNNCommon;
    using DotNetNuke.Web.NewDDRMenu.TemplateEngine;

    using Microsoft.Extensions.DependencyInjection;

    public class SkinObject : SkinObjectBase
    {
        private readonly ILocaliser localiser;
        private MenuBase menu;

        /// <summary>Initializes a new instance of the <see cref="SkinObject"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with ILocaliser. Scheduled removal in v12.0.0.")]
        public SkinObject()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SkinObject"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        public SkinObject(ILocaliser localiser)
        {
            this.localiser = localiser ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocaliser>();
        }

        public string MenuStyle { get; set; }

        public string NodeXmlPath { get; set; }

        public string NodeSelector { get; set; }

        public bool IncludeContext { get; set; }

        public bool IncludeHidden { get; set; }

        public string IncludeNodes { get; set; }

        public string ExcludeNodes { get; set; }

        public string NodeManipulator { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<ClientOption> ClientOptions { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<TemplateArgument> TemplateArguments { get; set; }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            using (new DNNContext(this))
            {
                try
                {
                    base.OnPreRender(e);

                    this.menu = MenuBase.Instantiate(this.localiser, this.MenuStyle);
                    this.menu.ApplySettings(
                        new Settings
                        {
                            MenuStyle = this.MenuStyle,
                            NodeXmlPath = this.NodeXmlPath,
                            NodeSelector = this.NodeSelector,
                            IncludeContext = this.IncludeContext,
                            IncludeHidden = this.IncludeHidden,
                            IncludeNodes = this.IncludeNodes,
                            ExcludeNodes = this.ExcludeNodes,
                            NodeManipulator = this.NodeManipulator,
                            ClientOptions = this.ClientOptions,
                            TemplateArguments = this.TemplateArguments,
                        });

                    if (string.IsNullOrEmpty(this.NodeXmlPath))
                    {
                        this.menu.RootNode =
                            new MenuNode(
                                this.localiser.LocaliseDNNNodeCollection(
                                    Navigation.GetNavigationNodes(
                                        this.ClientID,
                                        Navigation.ToolTipSource.None,
                                        -1,
                                        -1,
                                        DNNAbstract.GetNavNodeOptions(true))));
                    }

                    this.menu.PreRender();
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter writer)
        {
            using (new DNNContext(this))
            {
                try
                {
                    base.Render(writer);
                    this.menu.Render(writer);
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }
    }
}
