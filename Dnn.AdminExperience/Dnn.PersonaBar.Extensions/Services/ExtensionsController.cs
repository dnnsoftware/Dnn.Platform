// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Xml;
    using System.Xml.XPath;

    using Dnn.PersonaBar.Extensions.Components;
    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Editors;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Installer.Writers;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    using Constants = Dnn.PersonaBar.Extensions.Components.Constants;
    using Util = DotNetNuke.Entities.Content.Common.Util;

    [MenuPermission(Scope = ServiceScope.Admin)]
    public class ExtensionsController : PersonaBarApiController
    {
        private const string AuthFailureMessage = "Authorization has been denied for this request.";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExtensionsController));
        private static readonly Regex ManifestExensionsRegex = new Regex(@"dnn\d*$");
        private static readonly string[] SpecialModuleFolders = new[] { "mvc" };
        private readonly Components.ExtensionsController _controller = new Components.ExtensionsController();

        /// GET: api/Extensions/GetPackageTypes
        /// <summary>
        /// Get installed package types.
        /// </summary>
        /// <returns>List of package types.</returns>
        [HttpGet]
        public HttpResponseMessage GetPackageTypes()
        {
            try
            {
                var packageTypes = this._controller.GetPackageTypes()
                    .OrderBy(t => t.Value.PackageType != "Module")
                    .Select(t =>
                    {
                        var packageType = t.Value.PackageType;
                        string rootPath;
                        return new
                        {
                            Type = packageType,
                            HasAvailablePackages = this._controller.HasAvailablePackage(packageType, out rootPath),
                            DisplayName = Localization.GetString(packageType + ".Type", Constants.SharedResources),
                        };
                    });
                var response = new
                {
                    Success = true,
                    Results = packageTypes
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetAllPackagesListExceptLangPacks
        /// <summary>
        /// Get installed packages list except language packs.
        /// </summary>
        /// <returns>List of [Id,Name] pairs of all system packages.</returns>
        [HttpGet]
        public HttpResponseMessage GetAllPackagesListExceptLangPacks()
        {
            try
            {
                var packages = Utility.GetAllPackagesListExceptLangPacks();
                return this.Request.CreateResponse(HttpStatusCode.OK, packages);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetInstalledPackages
        /// <summary>
        /// Gets installed packages.
        /// </summary>
        /// <param name="packageType"></param>
        /// <returns>List of installed packages.</returns>
        [HttpGet]
        public HttpResponseMessage GetInstalledPackages(string packageType)
        {
            try
            {
                var packages = this._controller.GetInstalledPackages(this.UserInfo.IsSuperUser ? -1 : this.PortalSettings.PortalId, packageType);
                var response = new
                {
                    Success = true,
                    Results = packages,
                    TotalResults = packages.Count
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetAvailablePackages
        /// <summary>
        /// Gets available packages.
        /// </summary>
        /// <param name="packageType"></param>
        /// <returns>List of available packages.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAvailablePackages(string packageType)
        {
            try
            {
                var packages = this._controller.GetAvailablePackages(packageType);
                var response = new
                {
                    Success = true,
                    Results = packages,
                    TotalResults = packages.Count
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //        /// GET: api/Extensions/GetPackageUsage
        //        /// <summary>
        //        /// Gets package usage
        //        /// </summary>
        //        /// <param name="portalId"></param>
        //        /// <param name="packageId"></param>
        //        /// <returns>List of tabs using a specific package</returns>
        //        [HttpGet]
        //        public HttpResponseMessage GetPackageUsage(int portalId, int packageId)
        //        {
        //            try
        //            {
        //                var packages = _controller.GetPackageUsage(portalId, packageId).Select(t => new
        //                {
        //                    TabLink = _controller.GetFormattedTabLink(portalId, t)
        //                }).ToList();
        //
        //                var response = new
        //                {
        //                    Success = true,
        //                    Results = packages,
        //                    TotalResults = packages.Count
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, response);
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Error(ex);
        //                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //            }
        //        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSourceFolders(/*int moduleControlId*/)
        {
            var path = Path.Combine(Globals.ApplicationMapPath, "DesktopModules");
            var controlfolders = (
                from subdirectory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                select subdirectory).ToList();

            controlfolders.Insert(0, Path.Combine(Globals.ApplicationMapPath, "Admin\\Skins"));

            //var moduleControl = ModuleControlController.GetModuleControl(moduleControlId);
            //var currentControlFolder = moduleControl == null ? "" :
            //    (Path.GetDirectoryName(moduleControl.ControlSrc.ToLower()) ?? "").Replace('\\', '/');

            var response = new List<KeyValuePair<string, string>>();
            var appPathLen = Globals.ApplicationMapPath.Length + 1;
            foreach (var folder in controlfolders)
            {
                var moduleControls = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .Count(s => s.EndsWith(".ascx") || s.EndsWith(".cshtml") ||
                                s.EndsWith(".vbhtml") || s.EndsWith(".html") || s.EndsWith(".htm"));
                if (moduleControls > 0)
                {
                    var shortFolder = folder.Substring(appPathLen).Replace('\\', '/');
                    var item = new KeyValuePair<string, string>(shortFolder.ToLower(), shortFolder);
                    response.Add(item);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSourceFiles(string root)
        {
            var response = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("", "<" + Localization.GetString("None_Specified") + ">")
            };

            if (!string.IsNullOrEmpty(root))
            {
                var path = Path.Combine(Globals.ApplicationMapPath, root.Replace('/', '\\'));
                if (Directory.Exists(path))
                {
                    AddFiles(response, path, root, "*.ascx");
                    AddFiles(response, path, root, "*.cshtml");
                    AddFiles(response, path, root, "*.vbhtml");
                    AddFiles(response, path, root, "*.html");
                    AddFiles(response, path, root, "*.htm");
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage LoadIcons(string controlPath)
        {
            var response = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("", "<" + Localization.GetString("None_Specified") + ">")
            };

            if (!string.IsNullOrEmpty(controlPath))
            {
                var idx = controlPath.LastIndexOf("/", StringComparison.Ordinal);
                var root = controlPath.Substring(0, Math.Max(0, idx));
                var path = Path.Combine(Globals.ApplicationMapPath, root.Replace('/', '\\'));
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path);
                    if (files.Length > 0)
                    {
                        var extensions = Globals.glbImageFileTypes.ToLowerInvariant().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (var file in files)
                        {
                            var ext = Path.GetExtension(file) ?? "";
                            var extension = ext.Length <= 1 ? "" : ext.Substring(1).ToLowerInvariant();
                            if (extensions.Contains(extension))
                            {
                                path = Path.GetFileName(file);
                                if (path != null)
                                {
                                    var item = new KeyValuePair<string, string>(path.ToLower(), Path.GetFileName(file));
                                    response.Add(item);
                                }
                            }
                        }

                    }
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetLanguagesList()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, Utility.GetAllLanguagesList());
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleCategories()
        {
            var termController = Util.GetTermController();
            var categories = termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Select(t => t.Name);

            return this.Request.CreateResponse(HttpStatusCode.OK, categories);
        }

        [HttpGet]
        public HttpResponseMessage GetPackageSettings(int siteId, int packageId)
        {
            var portalId = siteId;
            if (portalId == Null.NullInteger && !this.UserInfo.IsSuperUser)
            {
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId);
            if (package == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
            }

            try
            {
                var packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
                var packageDetail = packageEditor?.GetPackageDetail(portalId, package) ?? new PackageInfoDto(portalId, package);
                return this.Request.CreateResponse(HttpStatusCode.OK, packageDetail);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SavePackageSettings(PackageSettingsDto packageSettings)
        {
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageSettings.PackageId);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        Localization.GetString("SavePackageSettings.PackageNotFound", Constants.SharedResources));
                }

                if (this.UserInfo.IsSuperUser)
                {
                    var authService = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
                    var isReadOnly = authService != null && authService.AuthenticationType == Constants.DnnAuthTypeName;
                    if (isReadOnly)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            Localization.GetString("ReadOnlyPackage.SaveErrorMessage", Constants.SharedResources));
                    }

                    var type = package.GetType();
                    var needUpdate = false;
                    foreach (var kvp in packageSettings.Settings)
                    {
                        var property = type.GetProperty(kvp.Key, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                        if (property != null && property.CanWrite)
                        {
                            var value = kvp.Value;
                            var propValue = property.GetValue(package);
                            if (propValue == null || propValue.ToString() != value)
                            {
                                var nativeValue = property.PropertyType == typeof(Version)
                                    ? new Version(value) : Convert.ChangeType(value, property.PropertyType);
                                property.SetValue(package, nativeValue);
                                needUpdate = true;
                            }
                        }
                    }

                    if (needUpdate)
                    {
                        PackageController.Instance.SaveExtensionPackage(package);
                    }
                }

                var packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
                if (packageEditor != null)
                {
                    string error;
                    packageEditor.SavePackageSettings(packageSettings, out error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Error = error });
                    }
                }

                var packageDetail = packageEditor?.GetPackageDetail(packageSettings.PortalId, package) ?? new PackageInfoDto(packageSettings.PortalId, package);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, PackageDetail = packageDetail });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAvailableControls(int packageId)
        {
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId);
            if (package == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
            }

            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(packageId);
                var rootFolder = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", desktopModule.FolderName);
                var controls = Directory.GetFiles(rootFolder, "*.ascx", SearchOption.AllDirectories)
                    .Select(f => f.Replace(Globals.ApplicationMapPath, "~").Replace("\\", "/"))
                    .ToList();

                return this.Request.CreateResponse(HttpStatusCode.OK, controls);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage DeletePackage(DeletePackageDto deletePackage)
        {
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == deletePackage.Id);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
                }

                var installer = new Installer(package, Globals.ApplicationMapPath);
                installer.UnInstall(deletePackage.DeleteFiles);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        [RequireHost]
        public Task<HttpResponseMessage> InstallPackage([FromUri] string legacySkin = null, [FromUri] bool isPortalPackage = false)
        {
            try
            {
                return
                    this.UploadFileAction((portalSettings, userInfo, filePath, stream) =>
                        InstallController.Instance.InstallPackage(portalSettings, userInfo, legacySkin, filePath, stream, isPortalPackage));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Task.FromResult(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        [RequireHost]
        public Task<HttpResponseMessage> ParsePackage()
        {
            try
            {
                return
                    this.UploadFileAction((portalSettings, userInfo, filePath, stream) =>
                        InstallController.Instance.ParsePackage(portalSettings, userInfo, filePath, stream));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Task.FromResult(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ParsePackageFile(DownloadPackageDto package)
        {
            try
            {
                var installFolder = this.GetPackageInstallFolder(package.PackageType);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(package.FileName))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, package.FileName);
                if (!File.Exists(packagePath))
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }

                var result = this.ParsePackageFile(packagePath);
                return this.Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ParseLanguagePackage([FromUri] string cultureCode)
        {
            try
            {
                DotNetNuke.Services.Upgrade.Internals.InstallController.Instance.IsAvailableLanguagePack(cultureCode);
                const string packageFileName = "installlanguage.resources";
                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install/Language/" + packageFileName);
                var result = this.ParsePackageFile(packagePath);
                return this.Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Inatall a package that is already included under one of the installation folders.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage InstallAvailablePackage(DownloadPackageDto package)
        {
            try
            {
                var installFolder = this.GetPackageInstallFolder(package.PackageType);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(package.FileName))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, package.FileName);
                if (!File.Exists(packagePath))
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }

                var installResult = this.InstallPackageFile(packagePath);
                return this.Request.CreateResponse(installResult.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, installResult);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Download install package.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage DownloadPackage(string packageType, string fileName)
        {
            try
            {
                var installFolder = this.GetPackageInstallFolder(packageType);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(fileName))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, fileName);
                if (!File.Exists(packagePath))
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }

                if (fileName.EndsWith(".resources"))
                {
                    fileName = fileName.Replace(".resources", ".zip");
                }

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                var stream = new FileStream(packagePath, FileMode.Open);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentLength = stream.Length;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage DownloadLanguagePackage([FromUri] string cultureCode)
        {
            try
            {
                const string packageFileName = "installlanguage.resources";
                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install/Language/" + packageFileName);

                var parsePackage = this.ParsePackageFile(packagePath);
                var invalidPackage = !parsePackage.Success
                                        || !parsePackage.PackageType.Equals("CoreLanguagePack")
                                        || !parsePackage.Name.EndsWith(cultureCode, StringComparison.InvariantCultureIgnoreCase);

                if (invalidPackage)
                {
                    DotNetNuke.Services.Upgrade.Internals.InstallController.Instance.IsAvailableLanguagePack(cultureCode);
                }

                return this.DownLoadFile(packagePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetPackageUsageFilter()
        {
            try
            {
                var portals = this.UserInfo.IsSuperUser ? PortalController.Instance.GetPortals().OfType<PortalInfo>() : PortalController.Instance.GetPortals().OfType<PortalInfo>().Where(p => p.PortalID == this.PortalId);
                var availablePortals = portals.Select(v => new
                {
                    v.PortalID,
                    v.PortalName,
                    IsCurrentPortal = this.PortalId == v.PortalID
                }).ToList();

                if (this.UserInfo.IsSuperUser)
                {
                    availablePortals.Insert(0, new
                    {
                        PortalID = -2,
                        PortalName = "Host",
                        IsCurrentPortal = false
                    });
                }

                var response = new
                {
                    Success = true,
                    Results = availablePortals,
                    TotalResults = availablePortals.Count
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetPackageUsage(int portalId, int packageId)
        {
            try
            {
                var pid = portalId == -2 ? Null.NullInteger : portalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var tabsWithModule = TabController.Instance.GetTabsByPackageID(pid, packageId, false);
                var allPortalTabs = TabController.Instance.GetTabsByPortal(pid);
                IDictionary<int, TabInfo> tabsInOrder = new Dictionary<int, TabInfo>();

                foreach (var tab in allPortalTabs.Values)
                {
                    AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
                }
                var response = new
                {
                    Success = true,
                    Results = tabsInOrder.Select(t => new
                    {
                        TabUrl = this.GetFormattedLink(t.Value)
                    }).ToList(),
                    TotalResults = tabsInOrder.Count
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateExtension(PackageSettingsDto packageSettings)
        {
            try
            {
                var package = new PackageInfo { PortalID = packageSettings.PortalId };
                var type = package.GetType();
                foreach (var kvp in packageSettings.Settings.Where(kpv => kpv.Value != null))
                {
                    var property = type.GetProperty(kvp.Key, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                    if (property != null && property.CanWrite)
                    {
                        var value = kvp.Value;
                        var propValue = property.GetValue(package);
                        if (propValue == null || propValue.ToString() != value)
                        {
                            var nativeValue = property.PropertyType == typeof(Version)
                                ? new Version(value) : Convert.ChangeType(value, property.PropertyType);
                            property.SetValue(package, nativeValue);
                        }
                    }
                }

                var tmpPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == package.Name);
                if (tmpPackage != null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Error = "DuplicateName" });
                }

                PackageController.Instance.SaveExtensionPackage(package);
                packageSettings.PackageId = package.PackageID;

                Locale locale;
                LanguagePackInfo languagePack;
                PackageTypes pkgType;
                Enum.TryParse(package.PackageType, true, out pkgType);

                switch (pkgType)
                {
                    case PackageTypes.AuthSystem:
                        //Create a new Auth System
                        var authSystem = new AuthenticationInfo
                        {
                            AuthenticationType = package.Name,
                            IsEnabled = Null.NullBoolean,
                            PackageID = package.PackageID
                        };
                        AuthenticationController.AddAuthentication(authSystem);
                        break;
                    case PackageTypes.Container:
                    case PackageTypes.Skin:
                        var skinPackage = new SkinPackageInfo
                        {
                            SkinName = package.Name,
                            PackageID = package.PackageID,
                            SkinType = package.PackageType
                        };
                        SkinController.AddSkinPackage(skinPackage);
                        break;
                    case PackageTypes.CoreLanguagePack:
                        locale = LocaleController.Instance.GetLocale(PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage);
                        languagePack = new LanguagePackInfo
                        {
                            PackageID = package.PackageID,
                            LanguageID = locale.LanguageId,
                            DependentPackageID = -2
                        };
                        LanguagePackController.SaveLanguagePack(languagePack);
                        break;
                    case PackageTypes.ExtensionLanguagePack:
                        locale = LocaleController.Instance.GetLocale(PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage);
                        languagePack = new LanguagePackInfo
                        {
                            PackageID = package.PackageID,
                            LanguageID = locale.LanguageId,
                            DependentPackageID = Null.NullInteger
                        };
                        LanguagePackController.SaveLanguagePack(languagePack);
                        break;
                    case PackageTypes.Module:
                        //Create a new DesktopModule
                        var desktopModule = new DesktopModuleInfo
                        {
                            PackageID = package.PackageID,
                            ModuleName = package.Name,
                            FriendlyName = package.FriendlyName,
                            FolderName = package.Name,
                            Description = package.Description,
                            Version = package.Version.ToString(3),
                            SupportedFeatures = 0
                        };
                        var desktopModuleId = DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
                        if (desktopModuleId > Null.NullInteger)
                        {
                            DesktopModuleController.AddDesktopModuleToPortals(desktopModuleId);
                        }
                        break;
                    case PackageTypes.SkinObject:
                        var skinControl = new SkinControlInfo { PackageID = package.PackageID, ControlKey = package.Name };
                        SkinControlController.SaveSkinControl(skinControl);
                        break;
                }

                var packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
                if (packageEditor != null)
                {
                    string error;
                    packageEditor.SavePackageSettings(packageSettings, out error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Error = error });
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, PackageId = package.PackageID });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetOwnerFolders()
        {
            try
            {
                var folders = new List<string>();
                foreach (var folder in this.GetRootModuleDefinitionFolders())
                {
                    var files = Directory.GetFiles(folder.Path, "*.ascx");
                    //exclude module folders
                    if (files.Length == 0 || folder.Path.ToLowerInvariant() == "admin")
                    {
                        var path = this.GetFolderPath(folder);
                        folders.Add(path);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, folders);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleFolders(string ownerFolder)
        {
            if (!string.IsNullOrEmpty(ownerFolder) &&
                (ownerFolder.Replace("\\", "/").Contains("/")
                 || ownerFolder.StartsWith(".")))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var folders = new List<string>();
                foreach (var moduleFolder in this.GetModulesFolders(ownerFolder))
                {
                    var path = this.GetFolderPath(moduleFolder);
                    folders.Add(path);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, folders);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleFiles(string ownerFolder, string moduleFolder, FileType type)
        {
            if ((!string.IsNullOrEmpty(ownerFolder) && (ownerFolder.Replace("\\", "/").Contains("/") || ownerFolder.StartsWith("."))) || string.IsNullOrEmpty(moduleFolder) || moduleFolder.Replace("\\", "/").Contains("/") || moduleFolder.StartsWith("."))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var files = new List<string>();
                var folder = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", ownerFolder ?? string.Empty, moduleFolder);
                switch (type)
                {
                    case FileType.Control:
                        files.AddRange(this.GetFiles(folder, "*.ascx"));
                        files.AddRange(this.GetFiles(folder, "*.html"));
                        files.AddRange(this.GetFiles(folder, "*.htm"));
                        files.AddRange(this.GetFiles(folder, "*.cshtml"));
                        files.AddRange(this.GetFiles(folder, "*.vbhtml"));
                        break;
                    case FileType.Template:
                        files.AddRange(this.GetFiles(Globals.HostMapPath + "Templates\\", ".module.template"));
                        break;
                    case FileType.Manifest:
                        files.AddRange(this.GetFiles(folder, "*.dnn*").Where(file => ManifestExensionsRegex.IsMatch(file)));
                        break;
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, files);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateFolder([FromUri] string ownerFolder, [FromUri] string moduleFolder)
        {
            if ((!string.IsNullOrEmpty(ownerFolder) && (ownerFolder.Replace("\\", "/").Contains("/") || ownerFolder.StartsWith(".")))
                || (!string.IsNullOrEmpty(moduleFolder) && (moduleFolder.Replace("\\", "/").Contains("/") || moduleFolder.StartsWith("."))))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var parentFolderPath = Globals.ApplicationMapPath + "\\DesktopModules";
                if (!string.IsNullOrEmpty(ownerFolder))
                {
                    parentFolderPath += "\\" + ownerFolder;

                    if (!Directory.Exists(parentFolderPath))
                    {
                        Directory.CreateDirectory(parentFolderPath);
                    }
                }

                if (!string.IsNullOrEmpty(moduleFolder))
                {
                    parentFolderPath += "\\" + moduleFolder;

                    if (!Directory.Exists(parentFolderPath))
                    {
                        Directory.CreateDirectory(parentFolderPath);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateModule(CreateModuleDto createModuleDto)
        {
            try
            {
                string errorMessage;
                string newPageUrl;

                var packageId = CreateModuleController.Instance.CreateModule(createModuleDto, out newPageUrl, out errorMessage);
                var newPackage = packageId > Null.NullInteger
                    ? PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId)
                    : null;

                var result = new
                {
                    Success = packageId > Null.NullInteger,
                    PackageInfo = newPackage != null ? new PackageInfoDto(Null.NullInteger, newPackage) : null,
                    NewPageUrl = newPageUrl,
                    Error = errorMessage
                };

                return this.Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetPackageManifest(int packageId)
        {
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "PackageNotFound");
                }

                switch (package.PackageType.ToLowerInvariant())
                {
                    case "corelanguagepack":
                        package.IconFile = "N\\A";
                        break;
                    default:
                        package.IconFile = DotNetNuke.Services.Installer.Util.ParsePackageIconFileName(package);
                        break;
                }

                var packageManifestDto = new PackageManifestDto(Null.NullInteger, package);

                var writer = PackageWriterFactory.GetWriter(package);

                packageManifestDto.BasePath = writer.BasePath;

                //Load Manifests
                if (!string.IsNullOrEmpty(package.Manifest))
                {
                    //Use Database
                    var sb = new StringBuilder();
                    var settings = new XmlWriterSettings();
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                    settings.OmitXmlDeclaration = true;
                    settings.Indent = true;

                    writer.WriteManifest(XmlWriter.Create(sb, settings), package.Manifest);

                    packageManifestDto.Manifests.Add("Database version", sb.ToString());
                }
                var filePath = Path.Combine(Globals.ApplicationMapPath, writer.BasePath);
                if (!string.IsNullOrEmpty(filePath))
                {
                    if (Directory.Exists(filePath))
                    {
                        foreach (var file in Directory.GetFiles(filePath, "*.dnn"))
                        {
                            var fileName = file.Replace(filePath + "\\", "");
                            packageManifestDto.Manifests.Add(fileName, this.GetFileContent(writer.BasePath, fileName));
                        }
                        foreach (var file in Directory.GetFiles(filePath, "*.dnn.resources"))
                        {
                            var fileName = file.Replace(filePath + "\\", "");
                            packageManifestDto.Manifests.Add(fileName, this.GetFileContent(writer.BasePath, fileName));
                        }
                    }
                }

                //get assemblies
                foreach (var file in writer.Assemblies.Values)
                {
                    packageManifestDto.Assemblies.Add(file.FullName);
                }

                //get files
                writer.GetFiles(true);

                //Display App Code files
                foreach (var file in writer.AppCodeFiles.Values)
                {
                    packageManifestDto.Files.Add("[app_code]" + file.FullName);
                }

                //Display Script files
                foreach (var file in writer.Scripts.Values)
                {
                    packageManifestDto.Files.Add(file.FullName);
                }

                //Display regular files
                foreach (var file in writer.Files.Values)
                {
                    if (file.Path.StartsWith(".git"))
                        continue;
                    if (!file.Name.StartsWith(".git"))
                    {
                        packageManifestDto.Files.Add(file.FullName);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, packageManifestDto);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateManifest(PackageManifestDto packageManifestDto)
        {
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(
                    Null.NullInteger, p => p.PackageID == packageManifestDto.PackageId);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "PackageNotFound");
                }

                return this.CreateManifestInternal(package, packageManifestDto);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateNewManifest(PackageManifestDto packageManifestDto)
        {
            try
            {
                var package = packageManifestDto.ToPackageInfo();
                return this.CreateManifestInternal(package, packageManifestDto);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreatePackage(PackageManifestDto packageManifestDto)
        {
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageManifestDto.PackageId);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "PackageNotFound");
                }

                //Save package manifest
                if (packageManifestDto.Manifests.Any())
                {
                    var doc = new XPathDocument(new StringReader(packageManifestDto.Manifests.Values.FirstOrDefault()));
                    XPathNavigator nav = doc.CreateNavigator();
                    XPathNavigator packageNav = nav.SelectSingleNode("dotnetnuke/packages");
                    package.Manifest = packageNav.InnerXml;
                    var pkgIconFile = DotNetNuke.Services.Installer.Util.ParsePackageIconFileName(package);
                    package.IconFile = (pkgIconFile.Trim().Length > 0) ? DotNetNuke.Services.Installer.Util.ParsePackageIconFile(package) : null;

                    PackageController.Instance.SaveExtensionPackage(package);
                }

                var writer = PackageWriterFactory.GetWriter(package);

                var manifestName = packageManifestDto.ManifestName;
                if (string.IsNullOrEmpty(manifestName))
                {
                    manifestName = packageManifestDto.ArchiveName.ToLowerInvariant().Replace("zip", "dnn");
                }
                //Use the installer to parse the manifest and load the files that need to be packaged
                var installer = new Installer(package, Globals.ApplicationMapPath);
                foreach (var file in installer.InstallerInfo.Files.Values)
                {
                    writer.AddFile(file);
                }
                string basePath;
                switch (package.PackageType.ToLowerInvariant())
                {
                    case "auth_system":
                        basePath = Globals.InstallMapPath + "AuthSystem";
                        break;
                    case "container":
                        basePath = Globals.InstallMapPath + "Container";
                        break;
                    case "corelanguagepack":
                    case "extensionlanguagepack":
                        basePath = Globals.InstallMapPath + "Language";
                        break;
                    case "module":
                        basePath = Globals.InstallMapPath + "Module";
                        break;
                    case "provider":
                        basePath = Globals.InstallMapPath + "Provider";
                        break;
                    case "skin":
                        basePath = Globals.InstallMapPath + "Skin";
                        break;
                    default:
                        basePath = Globals.HostMapPath;
                        break;
                }
                if (!manifestName.EndsWith(".dnn"))
                {
                    manifestName += ".dnn";
                }
                if (!packageManifestDto.ArchiveName.EndsWith(".zip"))
                {
                    packageManifestDto.ArchiveName += ".zip";
                }
                writer.CreatePackage(Path.Combine(basePath, packageManifestDto.ArchiveName), manifestName, package.Manifest, true);

                var logs = writer.Log.Logs.Select(l => l.ToString()).ToList();

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Logs = logs });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage RefreshPackageFiles(PackageFilesQueryDto packageData)
        {
            var baseFolder = Path.Combine(Globals.ApplicationMapPath, packageData.PackageFolder.Replace('/', '\\'));
            if (!Directory.Exists(baseFolder))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var package = packageData.ToPackageInfo();
                var writer = PackageWriterFactory.GetWriter(package);
                writer.BasePath = packageData.PackageFolder;
                writer.GetFiles(packageData.IncludeSource);

                var files = new List<string>();
                if (packageData.IncludeAppCode)
                {
                    // add app-code files
                    files.AddRange(writer.AppCodeFiles.Values.Select(file => "[app_code]" + file.FullName));
                }

                // add script files
                files.AddRange(writer.Scripts.Values.Select(f => f.FullName));

                // add code files
                files.AddRange(writer.Files.Values.Where(
                    f => !f.Path.StartsWith(".") && !f.Name.StartsWith(".")).Select(file => file.FullName));

                return this.Request.CreateResponse(HttpStatusCode.OK, files);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        protected string GetFormattedLink(object dataItem)
        {
            var returnValue = new StringBuilder();
            if ((dataItem is TabInfo))
            {
                var tab = (TabInfo)dataItem;
                {
                    var index = 0;
                    TabController.Instance.PopulateBreadCrumbs(ref tab);
                    foreach (TabInfo t in tab.BreadCrumbs)
                    {
                        if (index > 0)
                        {
                            returnValue.Append(" > ");
                        }
                        if ((tab.BreadCrumbs.Count - 1 == index))
                        {
                            var url = Globals.AddHTTP(t.PortalID == Null.NullInteger ? this.PortalSettings.PortalAlias.HTTPAlias : PortalAliasController.Instance.GetPortalAliasesByPortalId(t.PortalID).ToList().OrderByDescending(a => a.IsPrimary).FirstOrDefault().HTTPAlias) + "/Default.aspx?tabId=" + t.TabID;
                            returnValue.AppendFormat("<a target=\"_blank\" href=\"{0}\">{1}</a>", url, t.LocalizedTabName);
                        }
                        else
                        {
                            returnValue.AppendFormat("{0}", t.LocalizedTabName);
                        }
                        index = index + 1;
                    }
                }
            }
            return returnValue.ToString();
        }

        private static void AddFiles(ICollection<KeyValuePair<string, string>> collection, string path, string root, string filter)
        {
            var files = Directory.GetFiles(path, filter);
            foreach (var strFile in files)
            {
                var file = root.Replace('\\', '/') + "/" + Path.GetFileName(strFile);
                var item = new KeyValuePair<string, string>(file.ToLower(), file);
                collection.Add(item);
            }
        }

        private static void AddChildTabsToList(TabInfo currentTab, ref TabCollection allPortalTabs, ref IDictionary<int, TabInfo> tabsWithModule, ref IDictionary<int, TabInfo> tabsInOrder)
        {
            if (!tabsWithModule.ContainsKey(currentTab.TabID) || tabsInOrder.ContainsKey(currentTab.TabID)) return;
            tabsInOrder.Add(currentTab.TabID, currentTab);
            foreach (var tab in allPortalTabs.WithParentId(currentTab.TabID))
            {
                AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
            }
        }

        private HttpResponseMessage CreateManifestInternal(PackageInfo package, PackageManifestDto packageManifestDto)
        {
            var writer = PackageWriterFactory.GetWriter(package);

            foreach (var fileName in packageManifestDto.Files)
            {
                var name = fileName.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    writer.AddFile(new InstallFile(name));
                }
            }

            foreach (var fileName in packageManifestDto.Assemblies)
            {
                var name = fileName.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    writer.AddFile(new InstallFile(name));
                }
            }

            string manifestContent;
            if (!string.IsNullOrEmpty(packageManifestDto.ManifestName))
            {
                writer.WriteManifest(packageManifestDto.ManifestName, package.Manifest);
                manifestContent = package.Manifest;
            }
            else
            {
                manifestContent = writer.WriteManifest(false);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Content = manifestContent });
        }

        private Task<HttpResponseMessage> UploadFileAction(Func<PortalSettings, UserInfo, string, Stream, object> action)
        {
            var request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            // local references for use in closure
            var portalSettings = this.PortalSettings;
            var currentSynchronizationContext = SynchronizationContext.Current;
            var userInfo = this.UserInfo;

            var task = request.Content.ReadAsMultipartAsync(provider)
                .ContinueWith(o =>
                {
                    object result = null;

                    var fileName = string.Empty;
                    Stream stream = null;

                    foreach (var item in provider.Contents)
                    {
                        var name = item.Headers.ContentDisposition.Name;
                        switch (name.ToUpper())
                        {
                            case "\"POSTFILE\"":
                                fileName = item.Headers.ContentDisposition.FileName.Replace("\"", "");
                                if (fileName.IndexOf("\\", StringComparison.Ordinal) != -1)
                                {
                                    fileName = Path.GetFileName(fileName);
                                }
                                if (Globals.FileEscapingRegex.Match(fileName).Success == false)
                                {
                                    stream = item.ReadAsStreamAsync().Result;
                                }
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(fileName) && stream != null)
                    {
                        // The SynchronizationContext keeps the main thread context. Send method is synchronous
                        currentSynchronizationContext.Send(
                            delegate
                            {
                                result = action(portalSettings, userInfo, fileName, stream);
                            },
                            null
                            );
                    }

                    var mediaTypeFormatter = new JsonMediaTypeFormatter();
                    mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                    // Response Content Type cannot be application/json
                    // because IE9 with iframe-transport manages the response
                    // as a file download
                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        result,
                        mediaTypeFormatter,
                        "text/plain");
                });

            return task;
        }

        private string GetFolderPath(ModuleFolderDto folder)
        {
            var path = folder.Path.Replace(Path.GetDirectoryName(folder.Path) + "\\", "");
            if (folder.IsSpecial)
            {
                path = folder.SpecialType + "\\" + path;
            }

            return path;
        }

        private IList<ModuleFolderDto> GetRootModuleDefinitionFolders()
        {
            var moduleFolders = new List<ModuleFolderDto>();
            var rootFolders = Directory.GetDirectories(Globals.ApplicationMapPath + "\\DesktopModules\\").ToList();

            foreach (var folderPath in rootFolders)
            {
                var folderName = folderPath.Replace(Path.GetDirectoryName(folderPath) + "\\", "");
                if (this.IsSpecialFolder(folderName))
                {
                    Directory.GetDirectories(folderPath).ToList()
                        .ForEach(specialFolderChild =>
                            moduleFolders.Add(new ModuleFolderDto
                            {
                                Path = specialFolderChild,
                                IsSpecial = true,
                                SpecialType = folderName
                            })
                        );
                }
                else
                {
                    moduleFolders.Add(new ModuleFolderDto
                    {
                        Path = folderPath,
                        IsSpecial = false
                    });
                }
            }

            return moduleFolders;
        }

        private bool IsSpecialFolder(string folderName)
        {
            return SpecialModuleFolders.Any(specialFolder => specialFolder.ToLower().Equals(folderName.ToLower()));
        }

        private IList<ModuleFolderDto> GetModulesFolders(string ownerFolder)
        {
            if (!string.IsNullOrEmpty(ownerFolder))
            {
                return Directory.GetDirectories(Globals.ApplicationMapPath + "\\DesktopModules\\" + ownerFolder)
                    .Select(folder => new ModuleFolderDto { Path = folder, IsSpecial = false })
                    .ToList();
            }

            return this.GetRootModuleDefinitionFolders();
        }

        private IList<string> GetFiles(string rootFolder, string extension)
        {
            return Directory.GetFiles(rootFolder, extension).Select(Path.GetFileName).ToList();
        }

        private ParseResultDto ParsePackageFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new ParseResultDto() { Success = false, Message = "FileNotFound" };
            }

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                return InstallController.Instance.ParsePackage(this.PortalSettings, this.UserInfo, filePath, stream);
            }
        }

        private InstallResultDto InstallPackageFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new InstallResultDto() { Success = false, Message = "FileNotFound" };
            }

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                return InstallController.Instance.InstallPackage(this.PortalSettings, this.UserInfo, null, filePath, stream);
            }
        }

        private string GetPackageInstallFolder(string packageType)
        {
            switch ((packageType ?? "").ToLowerInvariant())
            {
                case "authsystem":
                case "auth_system":
                    return "AuthSystem";
                case "corelanguagepack":
                case "extensionlanguagepack":
                    return "Language";
                case "javascriptlibrary":
                case "javascript_library":
                    return "JavaScriptLibrary";
                case "module":
                case "skin":
                case "container":
                case "provider":
                case "library":
                    return packageType;
                default:
                    return string.Empty;
            }
        }

        private HttpResponseMessage DownLoadFile(string packagePath)
        {
            if (!File.Exists(packagePath))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "FileNotFound");
            }

            var stream = FileWrapper.Instance.OpenRead(packagePath);
            var fileName = Path.GetFileNameWithoutExtension(packagePath) + ".zip";

            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = fileName;
            return result;
        }

        private string GetFileContent(string basePath, string fileName)
        {
            var filename = Path.Combine(Globals.ApplicationMapPath, basePath, fileName);
            using (var objStreamReader = File.OpenText(filename))
            {
                return objStreamReader.ReadToEnd();
            }
        }
    }
}
