// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Dnn.Modules.ResourceManager.Components;
using DnnExceptions = DotNetNuke.Services.Exceptions.Exceptions;
using DotNetNuke.Common.Utilities;
using Dnn.Modules.ResourceManager.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Dnn.Modules.ResourceManager
{
    public partial class View : PortalModuleBase
    {
        private readonly string _bundleJsPath;
        private int? _gid;
        private int? _folderId;
        private int? FolderId
        {
            get
            {
                if (!_folderId.HasValue)
                {
                    if (int.TryParse(Request.QueryString["folderId"], out var id))
                    {
                        _folderId = id;
                    }
                }
                return _folderId;
            }
        }
        public int GroupId
        {
            get
            {
                if (_gid.HasValue) return _gid.Value;

                if (!int.TryParse(Request.QueryString["groupid"], out var id))
                    id = Null.NullInteger;
                _gid = id;
                return _gid.Value;
            }
        }
        private string _extensionWhitelist;
        public string ExtensionWhitelist
        {
            get
            {
                if (string.IsNullOrEmpty(_extensionWhitelist))
                {
                    _extensionWhitelist = FileManager.Instance.WhiteList.ToStorageString();
                }
                return _extensionWhitelist;
            }
        }
        private string _validationCode;
        public string ValidationCode
        {
            get
            {
                if (string.IsNullOrEmpty(_validationCode))
                {
                    var parameters = new List<object>() { ExtensionWhitelist.Split(',').Select(i => i.Trim()).ToList() };
                    parameters.Add(PortalSettings.PortalId);
                    parameters.Add(PortalSettings.UserInfo.UserID);
                    _validationCode = ValidationUtils.ComputeValidationCode(parameters);
                }
                return _validationCode;
            }
        }

        protected string ResxCulture => LocalizationController.Instance.CultureName;
        protected string ResxTimeStamp => LocalizationController.Instance.GetResxTimeStamp(Constants.ViewResourceFileName,
            Constants.ResourceManagerLocalization).ToString();
        protected int HomeFolderId => new SettingsManager(ModuleId, GroupId).HomeFolderId;
        protected bool UserLogged => UserId > 0;
        protected int FolderPanelNumItems => Constants.ItemsPerPage;
        protected int ItemWidth => Constants.ItemWidth;
        protected long MaxUploadSize => Config.GetMaxUploadSize();
        protected string SortingFields => Json.Serialize(Constants.SortingFields);
        protected string DefaultSortingField => Constants.DefaultSortingField;
        protected string MaxUploadSizeHumanReadable
            => string.Format(new FileSizeFormatProvider(), "{0:fs}", MaxUploadSize);

        protected int OpenFolderId => FolderId.GetValueOrDefault();

        protected string OpenFolderPath
        {
            get
            {
                if (!FolderId.HasValue)
                {
                    return "";
                }

                var folder = FolderManager.Instance.GetFolder(FolderId.Value);
                var folderPathElements = folder.FolderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                return Json.Serialize(folderPathElements);
            }
        }

        public View()
        {
            _bundleJsPath = Constants.ModulePath + "Scripts/resourceManager-bundle.js";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                ClientResourceManager.RegisterScript(Page, Constants.ModulePath + "Scripts/dnn.Localization.js");
                ClientResourceManager.RegisterScript(Page, _bundleJsPath);
            }
            catch (ModeValidationException exc)
            {
                DnnExceptions.ProcessModuleLoadException(LocalizeString(exc.Message), this, exc);
            }
            catch (Exception exc) //Module failed to load
            {
                DnnExceptions.ProcessModuleLoadException(exc.Message, this, exc);
            }
        }
    }
}