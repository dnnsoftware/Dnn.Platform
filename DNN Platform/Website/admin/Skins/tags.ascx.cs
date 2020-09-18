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
        private readonly INavigationManager _navigationManager;
        private string _AddImageUrl = IconController.IconURL("Add");
        private bool _AllowTagging = true;
        private string _CancelImageUrl = IconController.IconURL("Lt");
        private string _ObjectType = "Page";
        private string _RepeatDirection = "Horizontal";
        private string _SaveImageUrl = IconController.IconURL("Save");
        private string _Separator = ",&nbsp;";
        private bool _ShowCategories = true;
        private bool _ShowTags = true;

        public Tags()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string AddImageUrl
        {
            get
            {
                return this._AddImageUrl;
            }

            set
            {
                this._AddImageUrl = value;
            }
        }

        public bool AllowTagging
        {
            get
            {
                return this._AllowTagging;
            }

            set
            {
                this._AllowTagging = value;
            }
        }

        public string CancelImageUrl
        {
            get
            {
                return this._CancelImageUrl;
            }

            set
            {
                this._CancelImageUrl = value;
            }
        }

        public string CssClass { get; set; }

        public string ObjectType
        {
            get
            {
                return this._ObjectType;
            }

            set
            {
                this._ObjectType = value;
            }
        }

        public string RepeatDirection
        {
            get
            {
                return this._RepeatDirection;
            }

            set
            {
                this._RepeatDirection = value;
            }
        }

        public string SaveImageUrl
        {
            get
            {
                return this._SaveImageUrl;
            }

            set
            {
                this._SaveImageUrl = value;
            }
        }

        public string Separator
        {
            get
            {
                return this._Separator;
            }

            set
            {
                this._Separator = value;
            }
        }

        public bool ShowCategories
        {
            get
            {
                return this._ShowCategories;
            }

            set
            {
                this._ShowCategories = value;
            }
        }

        public bool ShowTags
        {
            get
            {
                return this._ShowTags;
            }

            set
            {
                this._ShowTags = value;
            }
        }

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
            this.tagsControl.NavigateUrlFormatString = this._navigationManager.NavigateURL(this.PortalSettings.SearchTabId, string.Empty, "Tag={0}");
            this.tagsControl.RepeatDirection = this.RepeatDirection;
            this.tagsControl.Separator = this.Separator;
            this.tagsControl.ShowCategories = this.ShowCategories;
            this.tagsControl.ShowTags = this.ShowTags;
        }
    }
}
