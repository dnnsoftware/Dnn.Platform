// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Icons;
    using Microsoft.Extensions.DependencyInjection;

    public partial class Tags : SkinObjectBase
    {
        private const string MyFileName = "Tags.ascx";
        private readonly INavigationManager navigationManager;

        /// <summary>Initializes a new instance of the <see cref="Tags"/> class.</summary>
        public Tags()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string AddImageUrl { get; set; } = IconController.IconURL("Add");

        public bool AllowTagging { get; set; } = true;

        public string CancelImageUrl { get; set; } = IconController.IconURL("Lt");

        public string CssClass { get; set; }

        public string ObjectType { get; set; } = "Page";

        public string RepeatDirection { get; set; } = "Horizontal";

        public string SaveImageUrl { get; set; } = IconController.IconURL("Save");

        public string Separator { get; set; } = ",&nbsp;";

        public bool ShowCategories { get; set; } = true;

        public bool ShowTags { get; set; } = true;

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.ObjectType == "Page")
            {
                this.tagsControl.ContentItem = this.PortalSettings.ActiveTab;
            }
            else
            {
                this.tagsControl.ContentItem = this.ModuleControl.ModuleContext.Configuration;
            }

            this.tagsControl.AddImageUrl = this.AddImageUrl;
            this.tagsControl.CancelImageUrl = this.CancelImageUrl;
            this.tagsControl.SaveImageUrl = this.SaveImageUrl;

            this.tagsControl.CssClass = this.CssClass;

            this.tagsControl.AllowTagging = this.AllowTagging && this.Request.IsAuthenticated;
            this.tagsControl.NavigateUrlFormatString = this.navigationManager.NavigateURL(this.PortalSettings.SearchTabId, string.Empty, "Tag={0}");
            this.tagsControl.RepeatDirection = this.RepeatDirection;
            this.tagsControl.Separator = this.Separator;
            this.tagsControl.ShowCategories = this.ShowCategories;
            this.tagsControl.ShowTags = this.ShowTags;
        }
    }
}
