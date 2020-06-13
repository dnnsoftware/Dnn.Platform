// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.HtmlEditorManager.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.UI;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.HtmlEditorManager.ViewModels;
    using DotNetNuke.Modules.HtmlEditorManager.Views;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Mvp;
    using DotNetNuke.Web.UI;
    using DotNetNuke.Web.UI.WebControls;

    /// <summary>
    /// Presenter for Provider Configuration.
    /// </summary>
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public class ProviderConfigurationPresenter : ModulePresenter<IProviderConfigurationView, ProviderConfigurationViewModel>
    {
        /// <summary>The HTML editor node.</summary>
        private const string HtmlEditorNode = "/configuration/dotnetnuke/htmlEditor";

        /// <summary>The dot net nuke document.</summary>
        private XmlDocument dotnetNukeDocument;

        /// <summary>Initializes a new instance of the <see cref="ProviderConfigurationPresenter" /> class.</summary>
        /// <param name="view">the interface provider view.</param>
        public ProviderConfigurationPresenter(IProviderConfigurationView view)
            : base(view)
        {
            this.View.Initialize += this.View_Initialize;
            this.View.SaveEditorChoice += this.View_SaveEditorChoice;
            this.View.EditorChanged += this.View_EditorChanged;
        }

        /// <summary>Gets the DNN configuration.</summary>
        /// <value>The DNN configuration.</value>
        protected XmlDocument DNNConfiguration
        {
            get
            {
                if (this.dotnetNukeDocument == null)
                {
                    UserInfo currentUser = UserController.Instance.GetCurrentUserInfo();
                    if (currentUser != null && currentUser.IsSuperUser)
                    {
                        this.dotnetNukeDocument = Config.Load();
                    }
                }

                return this.dotnetNukeDocument;
            }
        }

        /// <summary>Handles the Initialize event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void View_Initialize(object sender, EventArgs e)
        {
            this.View.Model.Editors = this.GetAvailableEditors();
            this.View.Model.SelectedEditor = this.GetSelectedEditor();
            this.View.Editor.Controls.Add(this.LoadCurrentEditor());

            // hack: force DNN to load the Telerik Combobox skin. Needed for the RadEditor provider
            // TODO: Remove as the included file path isn't distributed with installs
            ClientResourceManager.RegisterStyleSheet(this.View.Editor.Page, "~/Portals/_default/Skins/_default/WebControlSkin/Default/ComboBox.Default.css");
        }

        /// <summary>Loads the current editor.</summary>
        /// <param name="editorName">Name of the editor.</param>
        /// <returns>The editor based on the current editor settings in the web configuration.</returns>
        private Control LoadCurrentEditor(string editorName)
        {
            XmlDocument dnnConfiguration = this.DNNConfiguration;
            XmlNode htmlProviderNode = this.GetHtmlEditorProviderNode(dnnConfiguration);
            XmlNode currentProvider = htmlProviderNode.SelectSingleNode("providers/add[@name='" + editorName + "']");

            var settingsAttribute = currentProvider.Attributes["settingsControlPath"];
            if (settingsAttribute != null)
            {
                try
                {
                    var controlPath = settingsAttribute.Value;
                    var control = (PortalModuleBase)((TemplateControl)this.View).LoadControl(controlPath);
                    control.ModuleConfiguration = this.ModuleContext.Configuration;
                    control.ID = Path.GetFileNameWithoutExtension(controlPath);
                    this.View.Model.CanSave = true;
                    return control;
                }
                catch (Exception)
                {
                    this.View.Model.CanSave = false;
                }
            }

            // Display a nice message to the user that this provider is not supported.
            return ((TemplateControl)this.View).LoadControl("~/DesktopModules/Admin/HtmlEditorManager/Controls/InvalidConfiguration.ascx");
        }

        /// <summary>Loads the current editor.</summary>
        /// <returns>The editor based on the current editor settings in the web configuration.</returns>
        private Control LoadCurrentEditor()
        {
            return this.LoadCurrentEditor(this.GetSelectedEditor());
        }

        /// <summary>Handles the Editor was changed event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EditorEventArgs"/> instance containing the event data.</param>
        private void View_EditorChanged(object sender, EditorEventArgs e)
        {
            this.View.Editor.Controls.Clear();
            this.View.Model.SelectedEditor = e.Editor;
            this.View.Model.CanSave = true;
        }

        /// <summary>Handles the SaveEditorChoice event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EditorEventArgs"/> instance containing the event data.</param>
        private void View_SaveEditorChoice(object sender, EditorEventArgs e)
        {
            this.SaveEditorInConfiguration(e.Editor);
            this.View.Refresh();
            this.View.Editor.Controls.Clear();
            this.View.Editor.Controls.Add(this.LoadCurrentEditor());
            this.View.Model.SelectedEditor = e.Editor;
        }

        /// <summary>Saves the editor in configuration.</summary>
        /// <param name="name">The name.</param>
        private void SaveEditorInConfiguration(string name)
        {
            XmlDocument dnnConfiguration = this.DNNConfiguration;
            XmlNode providerNode = this.GetHtmlEditorProviderNode(dnnConfiguration);

            if (providerNode != null)
            {
                providerNode.Attributes["defaultProvider"].Value = name;
                Config.Save(dnnConfiguration);
            }
        }

        /// <summary>Gets the HTML provider node.</summary>
        /// <param name="dnnConfiguration">The DNN configuration.</param>
        /// <returns>The XmlNode for the htmlEditor provider.</returns>
        private XmlNode GetHtmlEditorProviderNode(XmlDocument dnnConfiguration)
        {
            if (dnnConfiguration != null && dnnConfiguration.DocumentElement != null)
            {
                return dnnConfiguration.DocumentElement.SelectSingleNode(HtmlEditorNode);
            }

            return null;
        }

        /// <summary>Gets the provider list.</summary>
        /// <returns>A list of the installed providers.</returns>
        private List<string> GetAvailableEditors()
        {
            var editors = new List<string>();
            XmlDocument xmlConfig = this.DNNConfiguration;
            if (xmlConfig != null && xmlConfig.DocumentElement != null)
            {
                XmlNodeList editorNodes = xmlConfig.DocumentElement.SelectNodes(HtmlEditorNode + "/providers/add");
                if (editorNodes != null)
                {
                    int i = 0;
                    while (i < editorNodes.Count)
                    {
                        XmlNode node = editorNodes[i];
                        if (node.Attributes["name"] != null)
                        {
                            editors.Add(node.Attributes["name"].Value);
                        }

                        i = i + 1;
                    }
                }
            }

            return editors;
        }

        /// <summary>Gets the selected editor.</summary>
        /// <returns>The currently configured editor.</returns>
        private string GetSelectedEditor()
        {
            XmlDocument xmlConfig = this.DNNConfiguration;

            if (xmlConfig != null && xmlConfig.DocumentElement != null)
            {
                XmlNode editorProviderNode = xmlConfig.DocumentElement.SelectSingleNode(HtmlEditorNode);
                return editorProviderNode.Attributes["defaultProvider"].Value;
            }

            return string.Empty;
        }
    }
}
