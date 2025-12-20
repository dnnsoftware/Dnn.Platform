// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using DotNetNuke.Services.Localization;

    public class FileServerHandler : IHttpHandler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileServerHandler));

        /// <inheritdoc/>
        public bool IsReusable => true;

        /// <summary>
        /// This handler handles requests for LinkClick.aspx, but only those specifc
        /// to file serving.
        /// </summary>
        /// <param name="context">System.Web.HttpContext).</param>
        public void ProcessRequest(HttpContext context)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tabId = -1;
            var moduleId = -1;
            try
            {
                // get TabId
                if (context.Request.QueryString["tabid"] != null)
                {
                    if (!int.TryParse(context.Request.QueryString["tabid"], out tabId))
                    {
                        tabId = -1;
                    }
                }

                // get ModuleId
                if (context.Request.QueryString["mid"] != null)
                {
                    if (!int.TryParse(context.Request.QueryString["mid"], out moduleId))
                    {
                        moduleId = -1;
                    }
                }
            }
            catch (Exception)
            {
                // The TabId or ModuleId are incorrectly formatted (potential DOS)
                Handle404Exception(context, context.Request.RawUrl);
            }

            // get Language
            string language = portalSettings.DefaultLanguage;
            if (context.Request.QueryString["language"] != null)
            {
                language = context.Request.QueryString["language"];
            }
            else
            {
                if (context.Request.Cookies["language"] != null)
                {
                    language = context.Request.Cookies["language"].Value;
                }
            }

            if (LocaleController.Instance.IsEnabled(ref language, portalSettings.PortalId))
            {
                Localization.SetThreadCultures(new CultureInfo(language), portalSettings);
                Localization.SetLanguage(language);
            }

            // get the URL
            string url = string.Empty;
            if (context.Request.QueryString["fileticket"] != null)
            {
                url = "FileID=" + FileLinkClickController.Instance.GetFileIdFromLinkClick(context.Request.QueryString);
            }

            if (context.Request.QueryString["userticket"] != null)
            {
                url = "UserId=" + UrlUtils.DecryptParameter(context.Request.QueryString["userticket"]);
            }

            if (context.Request.QueryString["link"] != null)
            {
                url = context.Request.QueryString["link"];
                if (url.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase))
                {
                    url = string.Empty; // restrict direct access by FileID
                }
            }

            if (!string.IsNullOrEmpty(url))
            {
                url = url.Replace(@"\", @"/");

                // update clicks, this must be done first, because the url tracker works with unmodified urls, like tabid, fileid etc
                var objUrls = new UrlController();
                objUrls.UpdateUrlTracking(portalSettings.PortalId, url, moduleId, -1);
                TabType urlType = Globals.GetURLType(url);
                if (urlType == TabType.Tab)
                {
                    // verify whether the tab is exist, otherwise throw out 404.
                    if (TabController.Instance.GetTab(int.Parse(url), portalSettings.PortalId, false) == null)
                    {
                        Handle404Exception(context, context.Request.RawUrl);
                    }
                }

                if (urlType != TabType.File)
                {
                    url = Globals.LinkClick(url, tabId, moduleId, false);
                }

                if (urlType == TabType.File && url.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    // to handle legacy scenarios before the introduction of the FileServerHandler
                    var fileName = Path.GetFileName(url);

                    var folderPath = url.Substring(0, url.LastIndexOf(fileName, StringComparison.InvariantCulture));
                    var folder = FolderManager.Instance.GetFolder(portalSettings.PortalId, folderPath);

                    var file = FileManager.Instance.GetFile(folder, fileName);

                    url = "FileID=" + file.FileId;
                }

                // get optional parameters
                bool blnForceDownload = false;
                if ((context.Request.QueryString["forcedownload"] != null) || (context.Request.QueryString["contenttype"] != null))
                {
                    if (!bool.TryParse(context.Request.QueryString["forcedownload"], out blnForceDownload))
                    {
                        blnForceDownload = false;
                    }
                }

                var contentDisposition = blnForceDownload ? ContentDisposition.Attachment : ContentDisposition.Inline;

                // clear the current response
                context.Response.Clear();
                var fileManager = FileManager.Instance;
                try
                {
                    switch (urlType)
                    {
                        case TabType.File:
                            var download = false;
                            var file = fileManager.GetFile(int.Parse(UrlUtils.GetParameterValue(url)));
                            if (file != null)
                            {
                                if (!file.IsEnabled || !HasAPublishedVersion(file))
                                {
                                    if (context.Request.IsAuthenticated)
                                    {
                                        context.Response.Redirect(Globals.AccessDeniedURL(Localization.GetString("FileAccess.Error")), true);
                                    }
                                    else
                                    {
                                        context.Response.Redirect(Globals.AccessDeniedURL(), true);
                                    }
                                }

                                try
                                {
                                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
                                    var directUrl = fileManager.GetUrl(file);

                                    EventManager.Instance.OnFileDownloaded(new FileDownloadedEventArgs()
                                    {
                                        FileInfo = file,
                                        UserId = UserController.Instance.GetCurrentUserInfo().UserID,
                                    });

                                    if (directUrl.Contains("LinkClick") || (blnForceDownload && folderMapping.FolderProviderType == "StandardFolderProvider"))
                                    {
                                        fileManager.WriteFileToResponse(file, contentDisposition);
                                        download = true;
                                    }
                                    else
                                    {
                                        context.Response.Redirect(directUrl, /*endResponse*/ true);
                                    }
                                }
                                catch (PermissionsNotMetException)
                                {
                                    if (context.Request.IsAuthenticated)
                                    {
                                        context.Response.Redirect(Globals.AccessDeniedURL(Localization.GetString("FileAccess.Error")), true);
                                    }
                                    else
                                    {
                                        context.Response.Redirect(Globals.AccessDeniedURL(), true);
                                    }
                                }
                                catch (ThreadAbortException)
                                {
                                    // if call fileManager.WriteFileToResponse ThreadAbortException will shown, should catch it and do nothing.
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);
                                }
                            }

                            if (!download)
                            {
                                Handle404Exception(context, url);
                            }

                            break;
                        case TabType.Url:
                            // prevent phishing by verifying that URL exists in URLs table for Portal
                            if (objUrls.GetUrl(portalSettings.PortalId, url) != null)
                            {
                                context.Response.Redirect(url, true);
                            }

                            break;
                        default:
                            // redirect to URL
                            context.Response.Redirect(url, true);
                            break;
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception)
                {
                    Handle404Exception(context, url);
                }
            }
            else
            {
                Handle404Exception(context, url);
            }
        }

        private static bool HasAPublishedVersion(IFileInfo file)
        {
            if (file.HasBeenPublished)
            {
                return true;
            }

            // We should allow creator to see the file that is pending to be approved
            var user = UserController.Instance.GetCurrentUserInfo();
            return user != null && user.UserID == file.CreatedByUserID;
        }

        private static void Handle404Exception(HttpContext context, string url)
        {
            try
            {
                Exceptions.Exceptions.ProcessHttpException(url);
            }
            catch (Exception)
            {
                UrlUtils.Handle404Exception(context.Response, PortalController.Instance.GetCurrentPortalSettings());
            }
        }
    }
}
