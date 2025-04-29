// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Module;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

using DNNConnect.CKEditorProvider.Constants;
using DNNConnect.CKEditorProvider.Helper;
using DNNConnect.CKEditorProvider.Objects;
using DNNConnect.CKEditorProvider.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

/// <summary>The Editor Config Manger Module.</summary>
public partial class EditorConfigManager : ModuleSettingsBase
{
    /// <summary>  Gets Current Language from Url.</summary>
    protected string LangCode
    {
        get
        {
            return CultureInfo.CurrentCulture.Name;
        }
    }

    /// <summary>  Gets the Name for the Current Resource file name.</summary>
    protected string ResXFile
    {
        get
        {
            return
                this.ResolveUrl(
                    string.Format(
                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/{0}/Options.aspx.resx",
                        Localization.LocalResourceDirectory));
        }
    }

    /// <summary>Gets or sets the editor options control.</summary>
    private CKEditorOptions EditorOptions { get; set; }

    /// <summary>Raises the <see cref="E:System.Web.UI.Control.Init" /> event.</summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnInit(EventArgs e)
    {
        this.InitializeComponent();
        base.OnInit(e);
    }

    /// <summary>Handles the Load event of the Page control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            this.EditorOptions =
                (CKEditorOptions)this.Page.LoadControl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/CKEditorOptions.ascx");

            this.EditorOptions.IsHostMode = true;

            this.EditorOptions.CurrentPortalOnly = this.PortalOnly.Checked;

            this.EditorOptions.CurrentOrSelectedTabId = this.PortalSettings.ActiveTab.TabID;
            this.EditorOptions.CurrentOrSelectedPortalId = this.PortalSettings.PortalId;

            this.EditorOptions.DefaultHostLoadMode = 0;

            this.EditorOptions.ID = "CKEditor_Options";

            this.OptionsPlaceHolder.Controls.Add(this.EditorOptions);

            if (this.Page.IsPostBack)
            {
                return;
            }

            this.SetLanguage();
        }
        catch (Exception exception)
        {
            Exceptions.ProcessPageLoadException(exception);
        }
    }

    private static Dictionary<int, HashSet<TreeNode>> GetModuleNodes(int portalId, ModuleController moduleController, List<EditorHostSetting> editorHostSettings)
    {
        var portalModules = moduleController.GetModules(portalId).Cast<ModuleInfo>();
        Dictionary<int, HashSet<TreeNode>> modulesNodes = new Dictionary<int, HashSet<TreeNode>>();

        foreach (var m in portalModules)
        {
            var moduleNode = new TreeNode
            {
                Value = $"m{m.ModuleID}",
                Text = m.ModuleTitle,
                ImageUrl =
                    SettingsUtil.CheckSettingsExistByKey(editorHostSettings, $"DNNCKMI#{m.ModuleID}#INS#")
                        ? "../images/ModuleHasSetting.png"
                        : "../images/ModuleNoSetting.png",
            };

            if (modulesNodes.ContainsKey(m.TabID))
            {
                var nodes = modulesNodes[m.TabID];
                nodes.Add(moduleNode);
            }
            else
            {
                var nodes = new HashSet<TreeNode>();
                nodes.Add(moduleNode);
                modulesNodes.Add(m.TabID, nodes);
            }
        }

        return modulesNodes;
    }

    private static void LoadNodesByTreeViewHelper(
        List<EditorHostSetting> editorHostSettings,
        TreeNode portalNode,
        Dictionary<int, HashSet<TreeNode>> modulesNodes,
        List<TabInfo> tabs)
    {
        Func<TabInfo, int> getNodeId = x => x.TabID;
        Func<TabInfo, int> getParentId = x => x.ParentId;
        Func<TabInfo, string> getNodeText = x => x.TabName;
        Func<TabInfo, string> getNodeValue = x => $"t{x.TabID}";
        Func<int, bool> getParentIdCheck = x => x != -1;
        Func<TabInfo, string> getNodeImageURL =
            x => SettingsUtil.CheckSettingsExistByKey(editorHostSettings, $"DNNCKT#{x.TabID}#")
                ? "../images/PageHasSetting.png"
                : "../images/PageNoSetting.png";

        TreeViewHelper<int> tvh = new TreeViewHelper<int>();
        tvh.LoadNodes(tabs, portalNode.ChildNodes, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, getParentIdCheck, modulesNodes);
    }

    /// <summary>
    /// Required method for Designer support - do not modify
    ///  the contents of the method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        try
        {
            if (!this.Page.IsPostBack)
            {
                this.BindPortalTabsAndModulesTree();
            }

            this.PortalTabsAndModulesTree.SelectedNodeChanged += this.PortalTabsAndModulesTree_SelectedNodeChanged;
            this.PortalTabsAndModulesTree.SelectedNodeStyle.ForeColor = Color.Gray;
            this.PortalOnly.CheckedChanged += this.PortalOnly_CheckedChanged;
        }
        catch (Exception exception)
        {
            Exceptions.ProcessModuleLoadException(this, exception);
        }
    }

    /// <summary>Handles the CheckedChanged event of the PortalOnly control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void PortalOnly_CheckedChanged(object sender, EventArgs e)
    {
        this.BindPortalTabsAndModulesTree();

        this.EditorOptions.Visible = true;
        this.ModuleInstanceInfoPlaceHolder.Visible = false;

        this.PortalTabsAndModulesTree.Nodes[0].Selected = true;

        ////
        this.PortalTabsAndModulesTree.SelectedNode.ExpandAll();

        this.EditorOptions.IsHostMode = true;

        this.EditorOptions.CurrentPortalOnly = this.PortalOnly.Checked;

        // Load Portal Settings for the selected Portal if exist
        var portalId = this.PortalTabsAndModulesTree.SelectedValue.Substring(1);
        var tabId = this.PortalTabsAndModulesTree.SelectedNode.ChildNodes[0].Value.Substring(1);

        this.EditorOptions.CurrentOrSelectedPortalId = this.EditorOptions.CurrentPortalOnly ? Convert.ToInt32(portalId) : -1;
        this.EditorOptions.CurrentOrSelectedTabId = this.EditorOptions.CurrentPortalOnly ? Convert.ToInt32(tabId) : -1;

        this.EditorOptions.DefaultHostLoadMode = 0;

        this.BindPortalTabsAndModulesTree();

        // Load Settings
        this.EditorOptions.BindOptionsData(true);
    }

    /// <summary>Loads the Settings based on the Selected Portal/Tab/Module.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void PortalTabsAndModulesTree_SelectedNodeChanged(object sender, EventArgs e)
    {
        if (this.PortalTabsAndModulesTree.SelectedNode == null)
        {
            return;
        }

        this.EditorOptions.IsHostMode = true;
        this.EditorOptions.CurrentPortalOnly = this.PortalOnly.Checked;

        if (this.PortalTabsAndModulesTree.SelectedNode == null)
        {
            return;
        }

        if (this.PortalTabsAndModulesTree.SelectedValue.StartsWith("h"))
        {
            this.EditorOptions.Visible = true;
            this.ModuleInstanceInfoPlaceHolder.Visible = false;

            // Load Portal Settings for the selected Portal if exist
            var portalId = this.PortalTabsAndModulesTree.SelectedValue.Substring(1);
            var tabId = this.PortalTabsAndModulesTree.SelectedNode.ChildNodes[0].Value.Substring(1);

            int temp;
            this.EditorOptions.CurrentOrSelectedPortalId = int.TryParse(portalId, out temp) ? temp : -1;
            this.EditorOptions.CurrentOrSelectedTabId = Convert.ToInt32(tabId);

            this.EditorOptions.DefaultHostLoadMode = -1;

            // Load Settings
            this.EditorOptions.BindOptionsData(true);
        }

        if (this.PortalTabsAndModulesTree.SelectedValue.StartsWith("p"))
        {
            this.EditorOptions.Visible = true;
            this.ModuleInstanceInfoPlaceHolder.Visible = false;

            // Load Portal Settings for the selected Portal if exist
            var portalId = this.PortalTabsAndModulesTree.SelectedValue.Substring(1);
            var tabId = this.PortalTabsAndModulesTree.SelectedNode.ChildNodes[0].Value.Substring(1);

            this.EditorOptions.CurrentOrSelectedPortalId = Convert.ToInt32(portalId);
            this.EditorOptions.CurrentOrSelectedTabId = Convert.ToInt32(tabId);

            this.EditorOptions.DefaultHostLoadMode = 0;

            // Load Settings
            this.EditorOptions.BindOptionsData(true);
        }
        else if (this.PortalTabsAndModulesTree.SelectedValue.StartsWith("t"))
        {
            this.EditorOptions.Visible = true;
            this.ModuleInstanceInfoPlaceHolder.Visible = false;

            // Load Tab Settings for the selected Tab if exist
            var portalId = this.PortalTabsAndModulesTree.SelectedNode.Parent.Value.Substring(1);
            var tabId = this.PortalTabsAndModulesTree.SelectedValue.Substring(1);

            this.EditorOptions.CurrentOrSelectedPortalId = Convert.ToInt32(portalId);
            this.EditorOptions.CurrentOrSelectedTabId = Convert.ToInt32(tabId);

            this.EditorOptions.DefaultHostLoadMode = 1;

            // Load Settings
            this.EditorOptions.BindOptionsData(true);
        }
        else if (this.PortalTabsAndModulesTree.SelectedValue.StartsWith("m"))
        {
            this.EditorOptions.Visible = false;

            // Show Info Notice
            this.ModuleInstanceInfoPlaceHolder.Visible = true;
        }
    }

    /// <summary>Sets the language for all Controls.</summary>
    private void SetLanguage()
    {
        this.ModuleHeader.Text = Localization.GetString("ModuleHeader.Text", this.ResXFile, this.LangCode);
        this.PortalOnlyLabel.Text = Localization.GetString("PortalOnlyLabel.Text", this.ResXFile, this.LangCode);
        this.PortalOnly.Text = Localization.GetString("PortalOnly.Text", this.ResXFile, this.LangCode);
        this.HostHasSettingLabel.Text = Localization.GetString(
            "HostHasSettingLabel.Text", this.ResXFile, this.LangCode);
        this.HostNoSettingLabel.Text = Localization.GetString(
            "HostNoSettingLabel.Text", this.ResXFile, this.LangCode);
        this.PortalHasSettingLabel.Text = Localization.GetString(
            "PortalHasSettingLabel.Text", this.ResXFile, this.LangCode);
        this.PortalNoSettingLabel.Text = Localization.GetString(
            "PortalNoSettingLabel.Text", this.ResXFile, this.LangCode);
        this.PageHasSettingLabel.Text = Localization.GetString(
            "PageHasSettingLabel.Text", this.ResXFile, this.LangCode);
        this.PageNoSettingLabel.Text = Localization.GetString(
            "PageNoSettingLabel.Text", this.ResXFile, this.LangCode);
        this.ModuleHasSettingLabel.Text = Localization.GetString(
            "ModuleHasSettingLabel.Text", this.ResXFile, this.LangCode);
        this.ModuleNoSettingLabel.Text = Localization.GetString(
            "ModuleNoSettingLabel.Text", this.ResXFile, this.LangCode);
        this.IconLegendLabel.Text = Localization.GetString(
            "IconLegendLabel.Text", this.ResXFile, this.LangCode);
        this.ModuleInstanceInfo.Text = Localization.GetString("ModuleError.Text", this.ResXFile, this.LangCode);
    }

    /// <summary>Renders the Portal <see cref="Tabs"/> and Module Tree.</summary>
    private void BindPortalTabsAndModulesTree()
    {
        this.PortalTabsAndModulesTree.Nodes.Clear();

        var moduleController = new ModuleController();

        var settingsDictionary = EditorController.GetEditorHostSettings();

        if (this.PortalOnly.Checked)
        {
            this.RenderPortalNode(
                new PortalController().GetPortal(this.PortalSettings.PortalId), moduleController, settingsDictionary);
        }
        else
        {
            var portals = new PortalController().GetPortals().Cast<PortalInfo>().ToList();
            this.RenderHostNode(portals, moduleController, settingsDictionary);
        }
    }

    private void RenderHostNode(IEnumerable<PortalInfo> portals, ModuleController moduleController, List<EditorHostSetting> editorHostSettings)
    {
        var hostKey = SettingConstants.HostKey;
        var hostSettingsExist = SettingsUtil.CheckSettingsExistByKey(editorHostSettings, hostKey);

        var hostNode = new TreeNode()
        {
            Text = Localization.GetString("AllPortals.Text", this.ResXFile, this.LangCode),
            Value = "h",
            ImageUrl =
                hostSettingsExist
                    ? "../images/HostHasSetting.png"
                    : "../images/HostNoSetting.png",
            Expanded = true,
        };

        foreach (var portal in portals)
        {
            this.RenderPortalNode(portal, moduleController, editorHostSettings, hostNode);
        }

        this.PortalTabsAndModulesTree.Nodes.Add(hostNode);
    }

    /// <summary>Renders the <paramref name="portal" /> node.</summary>
    /// <param name="portal">The <paramref name="portal" />.</param>
    /// <param name="moduleController">The module controller.</param>
    /// <param name="editorHostSettings">The editor host settings.</param>
    /// <param name="parentNode">The parent node.</param>
    private void RenderPortalNode(PortalInfo portal, ModuleController moduleController, List<EditorHostSetting> editorHostSettings, TreeNode parentNode = null)
    {
        var portalKey = SettingConstants.PortalKey(portal.PortalID);

        var portalSettingsExists = SettingsUtil.CheckSettingsExistByKey(editorHostSettings, portalKey);

        // Portals
        var portalNode = new TreeNode
        {
            Text = portal.PortalName,
            Value = $"p{portal.PortalID}",
            ImageUrl =
                portalSettingsExists
                    ? "../images/PortalHasSetting.png"
                    : "../images/PortalNoSetting.png",
            Expanded = this.PortalOnly.Checked,
        };

        Dictionary<int, HashSet<TreeNode>> modulesNodes = GetModuleNodes(portal.PortalID, moduleController, editorHostSettings);
        var tabs = TabController.GetPortalTabs(portal.PortalID, -1, false, null, true, false, true, true, false);

        LoadNodesByTreeViewHelper(editorHostSettings, portalNode, modulesNodes, tabs);

        if (parentNode == null)
        {
            this.PortalTabsAndModulesTree.Nodes.Add(portalNode);
        }
        else
        {
            parentNode.ChildNodes.Add(portalNode);
        }
    }
}
