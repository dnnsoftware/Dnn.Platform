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

        private string addImageUrl = IconController.IconURL("Add");
        private bool allowTagging = true;

        private string cancelImageUrl = IconController.IconURL("Lt");
        private string objectType = "Page";
        private string repeatDirection = "Horizontal";

        private string saveImageUrl = IconController.IconURL("Save");
        private string separator = ",&nbsp;";
        private bool showCategories = true;
        private bool showTags = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tags"/> class.
        /// </summary>
        public Tags()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string AddImageUrl
        {
            get
            {
                return this.AddImageUrl;
            }

            set
            {
                this.AddImageUrl = value;
            }
        }

        public bool AllowTagging
        {
            get
            {
                return this.AllowTagging;
            }

            set
            {
                this.AllowTagging = value;
            }
        }

        public string CancelImageUrl
        {
            get
            {
                return this.CancelImageUrl;
            }

            set
            {
                this.CancelImageUrl = value;
            }
        }

        public string CssClass { get; set; }

        public string ObjectType
        {
            get
            {
                return this.ObjectType;
            }

            set
            {
                this.ObjectType = value;
            }
        }

        public string RepeatDirection
        {
            get
            {
                return this.RepeatDirection;
            }

            set
            {
                this.RepeatDirection = value;
            }
        }

        public string SaveImageUrl
        {
            get
            {
                return this.SaveImageUrl;
            }

            set
            {
                this.SaveImageUrl = value;
            }
        }

        public string Separator
        {
            get
            {
                return this.Separator;
            }

            set
            {
                this.Separator = value;
            }
        }

        public bool ShowCategories
        {
            get
            {
                return this.ShowCategories;
            }

            set
            {
                this.ShowCategories = value;
            }
        }

        public bool ShowTags
        {
            get
            {
                return this.ShowTags;
            }

            set
            {
                this.ShowTags = value;
            }
        }

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
