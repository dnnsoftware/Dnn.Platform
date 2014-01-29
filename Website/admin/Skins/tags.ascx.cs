#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class Tags : SkinObjectBase
    {
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
            tagsControl.NavigateUrlFormatString = Globals.NavigateURL(PortalSettings.SearchTabId, "", "Tag={0}");
            tagsControl.RepeatDirection = RepeatDirection;
            tagsControl.Separator = Separator;
            tagsControl.ShowCategories = ShowCategories;
            tagsControl.ShowTags = ShowTags;
        }
    }
}