#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Dnn.PersonaBar.Extensions.Components;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using Dnn.PersonaBar.Extensions.Components.Editors;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using Util = DotNetNuke.Entities.Content.Common.Util;

namespace Dnn.PersonaBar.Extensions.Services
{
    [ServiceScope(Scope = ServiceScope.AdminHost)]
    public class ExtensionsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExtensionsController));
        private readonly Components.ExtensionsController _controller = new Components.ExtensionsController();
        private static readonly string[] SpecialModuleFolders = new[] { "mvc" };

        #region Extensions Lists API

        /// GET: api/Extensions/GetInstalledPackageTypes
        /// <summary>
        /// Get installed package types.
        /// </summary>
        /// <returns>List of package types</returns>
        [HttpGet]
        public HttpResponseMessage GetPackageTypes()
        {
            try
            {
                var packageTypes = _controller.GetPackageTypes()
                                        .OrderBy(t => t.Value.PackageType != "Module")
                                        .Select(t =>
                                        {
                                            var packageType = t.Value.PackageType;
                                            string rootPath;
                                            return new
                                            {
                                                Type = packageType,
                                                HasAvailablePackages = _controller.HasAvailablePackage(packageType, out rootPath)
                                            };
                                        });
                var response = new
                {
                    Success = true,
                    Results = packageTypes
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetInstalledPackages
        /// <summary>
        /// Gets installed packages
        /// </summary>
        /// <param name="packageType"></param>
        /// <returns>List of installed packages</returns>
        [HttpGet]
        public HttpResponseMessage GetInstalledPackages(string packageType)
        {
            try
            {
                var packages = _controller.GetInstalledPackages(UserInfo.IsSuperUser ? -1 : PortalSettings.PortalId, packageType);
                var response = new
                {
                    Success = true,
                    Results = packages,
                    TotalResults = packages.Count
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetAvailablePackages
        /// <summary>
        /// Gets available packages
        /// </summary>
        /// <param name="packageType"></param>
        /// <returns>List of available packages</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAvailablePackages(string packageType)
        {
            try
            {
                var packages = _controller.GetAvailablePackages(packageType);
                var response = new
                {
                    Success = true,
                    Results = packages,
                    TotalResults = packages.Count
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetPortals
        /// <summary>
        /// Gets portals list
        /// </summary>
        /// <param></param>
        /// <returns>List of portals</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetPortals()
        {
            try
            {
                var portals = PortalController.Instance.GetPortals().OfType<PortalInfo>();
                var availablePortals = portals.Select(v => new
                {
                    v.PortalID,
                    v.PortalName
                }).ToList();
                var response = new
                {
                    Success = true,
                    Results = availablePortals,
                    TotalResults = availablePortals.Count
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetPackageUsage
        /// <summary>
        /// Gets package usage
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="packageId"></param>
        /// <returns>List of tabs using a specific package</returns>
        [HttpGet]
        public HttpResponseMessage GetPackageUsage(int portalId, int packageId)
        {
            try
            {
                var packages = _controller.GetPackageUsage(portalId, packageId).Select(t => new
                {
                    TabLink = _controller.GetFormattedTabLink(portalId, t)
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = packages,
                    TotalResults = packages.Count
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Download install package.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DownloadPackage(DownloadPackageDto package)
        {
            var packageType = package.Type.ToString();
            var packageName = package.Name;
            if (string.IsNullOrEmpty(packageType) 
                    || string.IsNullOrEmpty(packageName)
                    || packageName.Contains("/")
                    || packageName.Contains("\\"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var packageFile = new FileInfo(Path.Combine(Globals.ApplicationMapPath, "Install\\" + packageType, packageName));
            if (!packageFile.Exists)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            try
            {
                var fileName = packageName;
                if (fileName.EndsWith(".resources"))
                {
                    fileName = fileName.Replace(".resources", "") + ".zip";
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
                response.Headers.Add("Content-Length", packageFile.Length.ToString());
                response.Headers.Add("ContentType", "application/zip, application/octet-stream");

                using (var stream = new FileStream(packageFile.FullName, FileMode.Open))
                {
                    response.Content = new StreamContent(stream);
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new {Error = ex.Message});
            }
        }

        [HttpGet]
        public HttpResponseMessage GetSourceFolders( /*int moduleControlId*/)
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

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
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
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        private static void AddFiles(ICollection<KeyValuePair<string, string>> collection, string path,string root, string filter)
        {
            var files = Directory.GetFiles(path, filter);
            foreach (var strFile in files)
            {
                var file = root.Replace('\\', '/') + "/" + Path.GetFileName(strFile);
                var item = new KeyValuePair<string, string>(file.ToLower(), file);
                collection.Add(item);
            }
        }

        [HttpGet]
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

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        #endregion

        #region Edit Extensions API

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleCategories()
        {
            var termController = Util.GetTermController();
            var categories = termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Select(t => t.Name);

            return Request.CreateResponse(HttpStatusCode.OK, categories);
        }

        [HttpGet]
        public HttpResponseMessage GetPackageSettings(int siteId, int packageId)
        {
            if (siteId == Null.NullInteger && !UserInfo.IsSuperUser)
            {
                throw new SecurityException();
            }

            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId);
            if (package == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
            }

            try
            {
                var packageType = (PackageTypes)Enum.Parse(typeof(PackageTypes), package.PackageType, true);
                var packageEditor = PackageEditorFactory.GetPackageEditor(packageType);

                var packageDetail = packageEditor?.GetPackageDetail(siteId, package) ?? new PackageDetailDto(siteId, package);

                return Request.CreateResponse(HttpStatusCode.OK, packageDetail);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SavePackageSettings(PackageSettingsDto packageSettings)
        { 
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageSettings.PackageId);
                if (package != null)
                {
                    if (packageSettings.PortalId == Null.NullInteger && UserInfo.IsSuperUser)
                    {
                        var type = package.GetType();
                        var needUpdate = false;
                        foreach (var kvp in packageSettings.Settings)
                        {
                            var name = kvp.Key;
                            var value = kvp.Value;

                            var property = type.GetProperty(name,
                                BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                            if (property != null)
                            {
                                if (property.GetValue(package).ToString() != value)
                                {
                                    property.SetValue(package, value);
                                    needUpdate = true;
                                }
                            }
                        }

                        if (needUpdate)
                        {
                            PackageController.Instance.SaveExtensionPackage(package);
                        }
                    }

                    var packageType = (PackageTypes)Enum.Parse(typeof(PackageTypes), package.PackageType, true);
                    var packageEditor = PackageEditorFactory.GetPackageEditor(packageType);
                    if (packageEditor != null)
                    {
                        string error = string.Empty;
                        packageEditor.SavePackageSettings(packageSettings, out error);

                        if (!string.IsNullOrEmpty(error))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { Success = false, Error = error });
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAvailableControls(int packageId)
        {
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId);
            if (package == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
            }

            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(packageId);
                var rootFolder = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", desktopModule.FolderName);
                var controls = Directory.GetFiles(rootFolder, "*.ascx", SearchOption.AllDirectories)
                    .Select(f => f.Replace(Globals.ApplicationMapPath, "~").Replace("\\", "/"))
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, controls);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage DeletePackage(DeletePackageDto deletePackage)
        {
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == deletePackage.Id);
            if (package == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
            }

            var installer = new Installer(package, Globals.ApplicationMapPath);
            installer.UnInstall(deletePackage.DeleteFiles);

            return Request.CreateResponse(HttpStatusCode.OK, new {});
        }

        #endregion

        #region Install Wizard API

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        [RequireHost]
        public Task<HttpResponseMessage> ParsePackage()
        {
            try
            {
                return
                UploadFileAction((portalSettings, userInfo, fileName, stream) =>
                    InstallController.Instance.ParsePackage(portalSettings, userInfo, fileName, stream));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Task<HttpResponseMessage>.Factory.StartNew(
                    () => Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        [RequireHost]
        public Task<HttpResponseMessage> InstallPackage()
        {
            try
            {
                return
                UploadFileAction((portalSettings, userInfo, fileName, stream) =>
                    InstallController.Instance.InstallPackage(portalSettings, userInfo, fileName, stream));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Task<HttpResponseMessage>.Factory.StartNew(
                    () => Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [RequireHost]
        public HttpResponseMessage ParseLanguagePackage([FromUri]string cultureCode)
        {
            try
            {
                DotNetNuke.Services.Upgrade.Internals.InstallController.Instance.IsAvailableLanguagePack(cultureCode);
                const string packageFileName = "installlanguage.resources";
                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install/Language/" + packageFileName);
                return Request.CreateResponse(HttpStatusCode.OK, ParsePackageFile(packagePath));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [RequireHost]
        public HttpResponseMessage InstallAvailablePackage([FromUri]string packageType, string packageName)
        {
            try
            {
                var installFolder = GetPackageInstallFolder(packageType);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(packageName))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, packageName);
                var installResult = InstallPackageFile(packagePath);

                return Request.CreateResponse(HttpStatusCode.OK, installResult);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage DownloadLanguagePackage([FromUri]string cultureCode)
        {
            try
            {
                const string packageFileName = "installlanguage.resources";
                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install/Language/" + packageFileName);

                if (!File.Exists(packagePath))
                {
                    DotNetNuke.Services.Upgrade.Internals.InstallController.Instance.IsAvailableLanguagePack(cultureCode);
                }
                return DownLoadFile(packagePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage DownloadAvailablePackage([FromUri]string packageType, string packageName)
        {
            try
            {
                var installFolder = GetPackageInstallFolder(packageType);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(packageName))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, packageName);
                return DownLoadFile(packagePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Create Extension API

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateExtension(PackageInfoDto packageInfoDto)
        {
            try
            {
                var newPackage = packageInfoDto.ToPackageInfo();
                PackageInfo tmpPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == newPackage.Name);
                if (tmpPackage != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = false, Error = "DuplicateName" });
                }
                    PackageController.Instance.SaveExtensionPackage(newPackage);
                    var packageId = newPackage.PackageID;
                    Locale locale;
                    LanguagePackInfo languagePack;
                    switch (newPackage.PackageType)
                    {
                        case "Auth_System":
                            //Create a new Auth System
                            var authSystem = new AuthenticationInfo
                            {
                                AuthenticationType = newPackage.Name,
                                IsEnabled = Null.NullBoolean,
                                PackageID = newPackage.PackageID
                            };
                            AuthenticationController.AddAuthentication(authSystem);
                            break;
                        case "Container":
                        case "Skin":
                            var skinPackage = new SkinPackageInfo
                            {
                                SkinName = newPackage.Name,
                                PackageID = newPackage.PackageID,
                                SkinType = newPackage.PackageType
                            };
                            SkinController.AddSkinPackage(skinPackage);
                            break;
                        case "CoreLanguagePack":
                            locale = LocaleController.Instance.GetLocale(PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage);
                            languagePack = new LanguagePackInfo
                            {
                                PackageID = newPackage.PackageID,
                                LanguageID = locale.LanguageId,
                                DependentPackageID = -2
                            };
                            LanguagePackController.SaveLanguagePack(languagePack);
                            break;
                        case "ExtensionLanguagePack":
                            locale = LocaleController.Instance.GetLocale(PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage);
                            languagePack = new LanguagePackInfo
                            {
                                PackageID = newPackage.PackageID,
                                LanguageID = locale.LanguageId,
                                DependentPackageID = Null.NullInteger
                            };
                            LanguagePackController.SaveLanguagePack(languagePack);
                            break;
                        case "Module":
                            //Create a new DesktopModule
                            var desktopModule = new DesktopModuleInfo
                            {
                                PackageID = newPackage.PackageID,
                                ModuleName = newPackage.Name,
                                FriendlyName = newPackage.FriendlyName,
                                FolderName = newPackage.Name,
                                Description = newPackage.Description,
                                Version = newPackage.Version.ToString(3),
                                SupportedFeatures = 0
                            };
                            int desktopModuleId = DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
                            if (desktopModuleId > Null.NullInteger)
                            {
                                DesktopModuleController.AddDesktopModuleToPortals(desktopModuleId);
                            }
                            break;
                        case "SkinObject":
                            var skinControl = new SkinControlInfo { PackageID = newPackage.PackageID, ControlKey = newPackage.Name };
                            SkinControlController.SaveSkinControl(skinControl);
                            break;
                    }                

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, PackageId = newPackage.PackageID });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Create Module API

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetOwnerFolders()
        {
            try
            {
                var folders = new List<string>();
                foreach (var folder in GetRootModuleDefinitionFolders())
                {
                    var files = Directory.GetFiles(folder.Path, "*.ascx");
                    //exclude module folders
                    if (files.Length == 0 || folder.Path.ToLowerInvariant() == "admin")
                    {
                        var path = GetFolderPath(folder);
                        folders.Add(path);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, folders);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var folders = new List<string>();
                foreach (var moduleFolder in GetModulesFolders(ownerFolder))
                {
                    var path = GetFolderPath(moduleFolder);
                    folders.Add(path);
                }

                return Request.CreateResponse(HttpStatusCode.OK, folders);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleFiles(string ownerFolder, string moduleFolder, FileType type)
        {
            if ((!string.IsNullOrEmpty(ownerFolder) && (ownerFolder.Replace("\\", "/").Contains("/") || ownerFolder.StartsWith(".")))
                    || (string.IsNullOrEmpty(moduleFolder) || moduleFolder.Replace("\\", "/").Contains("/") || moduleFolder.StartsWith(".")))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var files = new List<string>();
                var folder = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", ownerFolder ?? string.Empty, moduleFolder);
                switch (type)
                {
                    case FileType.Control:
                        files.AddRange(GetFiles(folder, "*.ascx"));
                        files.AddRange(GetFiles(folder, "*.cshtml"));
                        files.AddRange(GetFiles(folder, "*.vbhtml"));
                        break;
                    case FileType.Template:
                        files.AddRange(GetFiles(Globals.HostMapPath + "Templates\\", ".module.template"));
                        break;
                    case FileType.Manifest:
                        files.AddRange(GetFiles(folder, "*.dnn"));
                        files.AddRange(GetFiles(folder, "*.dnn5"));
                        break;
                }

                return Request.CreateResponse(HttpStatusCode.OK, files);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateFolder([FromUri]string ownerFolder, [FromUri]string moduleFolder)
        {
            if ((!string.IsNullOrEmpty(ownerFolder) && (ownerFolder.Replace("\\", "/").Contains("/") || ownerFolder.StartsWith(".")))
                    || (!string.IsNullOrEmpty(moduleFolder) && (moduleFolder.Replace("\\", "/").Contains("/") || moduleFolder.StartsWith("."))))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
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

                return Request.CreateResponse(HttpStatusCode.OK, new {Success = true});
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                    ? PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId) : null;

                var result = new
                {
                    Success = packageId > Null.NullInteger,
                    PackageInfo = newPackage != null ? new PackageInfoDto(Null.NullInteger, newPackage) : null,
                    NewPageUrl = newPageUrl,
                    Error = errorMessage
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Module Definition Actions

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddOrUpdateModuleDefinition(ModuleDefinitionDto definition)
        {
            try
            {
                var existingName = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(definition.FriendlyName);
                if (definition.Id < 0 && existingName != null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        Localization.GetString("DuplicateDefinition.ErrorMessage"));
                }

                var moduleDefinition = definition.ToModuleDefinitionInfo();
                    ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, true);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, DefinitionId = moduleDefinition.ModuleDefID });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteModuleDefinition([FromUri] int definitionId)
        {
            try
            {
                new ModuleDefinitionController().DeleteModuleDefinition(definitionId);
                return Request.CreateResponse(HttpStatusCode.OK, new {Success = true, DefinitionId = definitionId});
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Module Control Actions

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddOrUpdateModuleControl(ModuleControlDto controlDto)
        {
            try
            {
                //check whether have a same control key in the module definition
                var controlKey = controlDto.Key ?? "";
                var moduleControls = ModuleControlController.GetModuleControlsByModuleDefinitionID(controlDto.DefinitionId).Values;
                var keyExists = moduleControls.Any(c => c.ModuleControlID != controlDto.Id &&
                    c.ControlKey.Equals(controlKey, StringComparison.InvariantCultureIgnoreCase));
                if (keyExists)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        Localization.GetString("DuplicateKey.ErrorMessage"));
                }

                try
                {
                    var moduleControl = controlDto.ToModuleControlInfo();
                    ModuleControlController.SaveModuleControl(moduleControl, true);
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, ModuleControlId = controlDto.Id });
                }
                catch
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        Localization.GetString("AddControl.ErrorMessage"));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteModuleControl([FromUri] int controlId)
        {
            try
            {
                ModuleControlController.DeleteModuleControl(controlId);
                return Request.CreateResponse(HttpStatusCode.OK, new {Success = true, ControlId = controlId});
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private Task<HttpResponseMessage> UploadFileAction(Func<PortalSettings, UserInfo, string, Stream, object> action)
        {
            var request = Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            // local references for use in closure
            var portalSettings = PortalSettings;
            var currentSynchronizationContext = SynchronizationContext.Current;
            var userInfo = UserInfo;

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

                    /* Response Content Type cannot be application/json 
                     * because IE9 with iframe-transport manages the response 
                     * as a file download 
                     */
                    return Request.CreateResponse(
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
                if (IsSpecialFolder(folderName))
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

            return GetRootModuleDefinitionFolders();
        }

        private IList<string> GetFiles(string rootFolder, string extension)
        {
            return Directory.GetFiles(rootFolder, extension).Select(Path.GetFileName).ToList();
        }

        private ParseResultDto ParsePackageFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new ParseResultDto() {Success = false, Message = "FileNotFound"};
            }

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var fileName = Path.GetFileName(filePath);
                return InstallController.Instance.ParsePackage(PortalSettings, UserInfo, fileName, stream);
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
                var fileName = Path.GetFileName(filePath);
                return InstallController.Instance.InstallPackage(PortalSettings, UserInfo, fileName, stream);
            }
        }

        private string GetPackageInstallFolder(string packageType)
        {
            switch (packageType.ToLowerInvariant())
            {
                case "auth_system":
                    return "AuthSystem";
                case "corelanguagepack":
                case "extensionlanguagepack":
                    return "Language";
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
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "FileNotFound");
            }

            var stream = FileWrapper.Instance.OpenRead(packagePath);
            var fileName = Path.GetFileNameWithoutExtension(packagePath) + ".zip";

            var result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StreamContent(stream)};
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = fileName;
            return result;
        }

        #endregion
    }
}
