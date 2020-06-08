// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Icons;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class Tags : SkinObjectBase
    {
        private readonly INavigationManager _navigationManager;
        private const string MyFileName = "Tags.ascx";
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
            _navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string AddImageUrl
        {
            get
            {
                return _AddImageUrl;
            }
            set
            {
                _AddImageUrl = value;
            }
        }

        public bool AllowTagging
        {
            get
            {
                return _AllowTagging;
            }
            set
            {
                _AllowTagging = value;
            }
        }

        public string CancelImageUrl
        {
            get
            {
                return _CancelImageUrl;
            }
            set
            {
                _CancelImageUrl = value;
            }
        }

        public string CssClass { get; set; }

        public string ObjectType
        {
            get
            {
                return _ObjectType;
            }
            set
            {
                _ObjectType = value;
            }
        }

        public string RepeatDirection
        {
            get
            {
                return _RepeatDirection;
            }
            set
            {
                _RepeatDirection = value;
            }
        }

        public string SaveImageUrl
        {
            get
            {
                return _SaveImageUrl;
            }
            set
            {
                _SaveImageUrl = value;
            }
        }

        public string Separator
        {
            get
            {
                return _Separator;
            }
            set
            {
                _Separator = value;
            }
        }

        public bool ShowCategories
        {
            get
            {
                return _ShowCategories;
            }
            set
            {
                _ShowCategories = value;
            }
        }

        public bool ShowTags
        {
            get
            {
                return _ShowTags;
            }
            set
            {
                _ShowTags = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (ObjectType == "Page")
            {
                tagsControl.ContentItem = PortalSettings.ActiveTab;
            }
            else
            {
                tagsControl.ContentItem = ModuleControl.ModuleContext.Configuration;
            }

            tagsControl.AddImageUrl = AddImageUrl;
            tagsControl.CancelImageUrl = CancelImageUrl;
            tagsControl.SaveImageUrl = SaveImageUrl;

            tagsControl.CssClass = CssClass;

            tagsControl.AllowTagging = AllowTagging && Request.IsAuthenticated;
            tagsControl.NavigateUrlFormatString = _navigationManager.NavigateURL(PortalSettings.SearchTabId, "", "Tag={0}");
            tagsControl.RepeatDirection = RepeatDirection;
            tagsControl.Separator = Separator;
            tagsControl.ShowCategories = ShowCategories;
            tagsControl.ShowTags = ShowTags;
        }
    }
}
