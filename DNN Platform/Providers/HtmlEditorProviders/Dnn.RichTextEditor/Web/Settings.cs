// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;

    using Dnn.RichTextEditorProvider.Components;
    using Dnn.RichTextEditorProvider.Components.FileBrowser;
    using Dnn.RichTextEditorProvider.Components.Settings;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The editor's settings page in the HTML Editor Manager: toolbar presets and upload size limits per role,
    /// and the upload folder, for all sites or for one site.
    /// </summary>
    public partial class Settings : PortalModuleBase
    {
        private EditorSettings boundSettings;

        private int Scope => int.Parse(this.ScopeList.SelectedValue, CultureInfo.InvariantCulture);

        private IServiceProvider Services => HttpContextSource.Current.GetScope().ServiceProvider;

        private EditorSettingsRepository Repository => new EditorSettingsRepository(
            new DnnSettingsStore(this.Services.GetRequiredService<IHostSettingsService>(), this.Services.GetRequiredService<IPortalController>()));

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // The HTML Editor Manager sets this control's ID, which the default resource file name is built from.
            this.LocalResourceFile = Globals.ResolveUrl(ProviderSettings.ProviderPath + "App_LocalResources/Settings.ascx.resx");

            this.ScopeList.SelectedIndexChanged += (sender, args) => this.BindScope();
            this.SaveButton.Click += this.SaveButton_Click;
            this.RoleRows.ItemDataBound += this.RoleRows_ItemDataBound;
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // The HTML Editor Manager is a host page, but check anyway: these settings apply to every site.
            if (!this.UserInfo.IsSuperUser)
            {
                this.SettingsPanel.Visible = false;
                this.ShowMessage("AccessDenied", false);
                return;
            }

            this.LocalizeLabels();
            if (!this.IsPostBack)
            {
                this.BindScopes();
                this.BindScope();
            }
        }

        private static int? ParseLimit(string text, out bool isValid)
        {
            isValid = true;
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            isValid = int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.CurrentCulture, out var limit) && limit > 0;
            return isValid ? limit : (int?)null;
        }

        private void LocalizeLabels()
        {
            this.IntroText.Text = this.LocalizeText("Intro");
            this.ScopeLabel.Text = this.LocalizeText("Scope");
            this.DefaultToolbarLabel.Text = this.LocalizeText("DefaultToolbar");
            this.UploadFolderLabel.Text = this.LocalizeText("UploadFolder");
            this.DefaultUploadLimitLabel.Text = this.LocalizeText("DefaultUploadLimit");
            this.RolesHeading.Text = this.LocalizeText("RolesHeading");
            this.RolesHelp.Text = this.LocalizeText("RolesHelp");
            this.RoleColumn.Text = this.LocalizeText("RoleColumn");
            this.ToolbarColumn.Text = this.LocalizeText("ToolbarColumn");
            this.UploadLimitColumn.Text = this.LocalizeText("UploadLimitColumn");
            this.SaveButton.Text = this.LocalizeText("Save");
        }

        private void BindScopes()
        {
            this.ScopeList.Items.Clear();
            this.ScopeList.Items.Add(new ListItem(this.LocalizeText("AllSites"), EditorSettingsRepository.AllSites.ToString(CultureInfo.InvariantCulture)));
            foreach (var portal in PortalController.Instance.GetPortals().Cast<IPortalInfo>().OrderBy(p => p.PortalName, StringComparer.CurrentCultureIgnoreCase))
            {
                this.ScopeList.Items.Add(new ListItem(portal.PortalName, portal.PortalId.ToString(CultureInfo.InvariantCulture)));
            }

            // The Site Settings persona bar opens the manager with the current site's ID.
            var requested = this.ScopeList.Items.FindByValue(this.Request.QueryString["portalid"] ?? string.Empty);
            if (requested != null)
            {
                this.ScopeList.ClearSelection();
                requested.Selected = true;
            }
        }

        private void BindScope()
        {
            var scope = this.Scope;
            this.boundSettings = this.Repository.Load(scope);
            this.ScopeHelp.Text = this.LocalizeText(scope == EditorSettingsRepository.AllSites ? "AllSitesHelp" : "SiteHelp");

            this.FillToolbarList(this.DefaultToolbarList, this.boundSettings.DefaultToolbar);
            this.UploadFolderText.Text = this.boundSettings.UploadFolder ?? string.Empty;
            this.DefaultUploadLimitText.Text = this.boundSettings.DefaultUploadLimitKb?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;

            this.RoleRows.DataSource = this.GetRoleNames(scope, this.boundSettings);
            this.RoleRows.DataBind();
        }

        private IList<string> GetRoleNames(int scope, EditorSettings settings)
        {
            // Role names apply across sites, so the all-sites scope lists the host site's roles
            // plus any role already configured.
            var portalId = scope == EditorSettingsRepository.AllSites ? this.Services.GetRequiredService<IHostSettings>().HostPortalId : scope;
            var names = RoleController.Instance.GetRoles(portalId)
                .Where(role => role.SecurityMode != SecurityMode.SocialGroup)
                .Select(role => role.RoleName)
                .Concat(settings.ToolbarRoles?.Keys ?? Enumerable.Empty<string>())
                .Concat(settings.UploadLimitRoles?.Keys ?? Enumerable.Empty<string>())
                .Where(name => name != Globals.glbRoleAllUsersName && name != Globals.glbRoleUnauthUserName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase);
            return new[] { Globals.glbRoleAllUsersName, Globals.glbRoleUnauthUserName, }.Concat(names).ToList();
        }

        private void FillToolbarList(DropDownList list, string selected)
        {
            list.Items.Clear();
            list.Items.Add(new ListItem(this.LocalizeText(this.Scope == EditorSettingsRepository.AllSites ? "NotSet" : "Inherit"), string.Empty));
            foreach (var preset in ToolbarPresets.All)
            {
                list.Items.Add(new ListItem(this.LocalizeText("Toolbar_" + preset), preset));
            }

            list.SelectedValue = selected ?? string.Empty;
        }

        private void RoleRows_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            var roleName = (string)e.Item.DataItem;
            string toolbar = null;
            int limit = 0;
            this.boundSettings.ToolbarRoles?.TryGetValue(roleName, out toolbar);
            this.boundSettings.UploadLimitRoles?.TryGetValue(roleName, out limit);

            this.FillToolbarList((DropDownList)e.Item.FindControl("RoleToolbar"), toolbar);
            ((TextBox)e.Item.FindControl("RoleUploadLimit")).Text = limit > 0 ? limit.ToString(CultureInfo.CurrentCulture) : string.Empty;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var scope = this.Scope;
            var settings = new EditorSettings
            {
                DefaultToolbar = ToolbarPresets.Normalize(this.DefaultToolbarList.SelectedValue),
                DefaultUploadLimitKb = ParseLimit(this.DefaultUploadLimitText.Text, out var isValid),
            };
            if (!isValid)
            {
                this.ShowMessage("InvalidLimit", false);
                return;
            }

            var toolbarRoles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var uploadLimitRoles = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (RepeaterItem row in this.RoleRows.Items)
            {
                var roleName = ((HiddenField)row.FindControl("RoleName")).Value;
                var toolbar = ToolbarPresets.Normalize(((DropDownList)row.FindControl("RoleToolbar")).SelectedValue);
                var limit = ParseLimit(((TextBox)row.FindControl("RoleUploadLimit")).Text, out isValid);
                if (!isValid)
                {
                    this.ShowMessage("InvalidLimit", false);
                    return;
                }

                if (toolbar != null)
                {
                    toolbarRoles[roleName] = toolbar;
                }

                if (limit.HasValue)
                {
                    uploadLimitRoles[roleName] = limit.Value;
                }
            }

            settings.ToolbarRoles = toolbarRoles.Count > 0 ? toolbarRoles : null;
            settings.UploadLimitRoles = uploadLimitRoles.Count > 0 ? uploadLimitRoles : null;

            if (!string.IsNullOrWhiteSpace(this.UploadFolderText.Text))
            {
                settings.UploadFolder = FileBrowserService.NormalizePath(this.UploadFolderText.Text);

                // Folders belong to a site, so a site's upload folder must exist there; for all sites it is checked when used.
                if (scope != EditorSettingsRepository.AllSites && FolderManager.Instance.GetFolder(scope, settings.UploadFolder) == null)
                {
                    this.ShowMessage("FolderNotFound", false);
                    return;
                }
            }

            this.Repository.Save(scope, settings);
            this.BindScope();
            this.ShowMessage("Saved", true);
        }

        private void ShowMessage(string key, bool success)
        {
            this.MessageLabel.Text = this.LocalizeText(key);
            this.MessageLabel.CssClass = success ? "dnnFormMessage dnnFormSuccess" : "dnnFormMessage dnnFormValidationSummary";
        }
    }
}
