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

        public bool IsReusable => true;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This handler handles requests for LinkClick.aspx, but only those specifc
        /// to file serving.
        /// </summary>
        /// <param name="context">System.Web.HttpContext).</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void ProcessRequest(HttpContext context)
        {
            var _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var TabId = -1;
            var ModuleId = -1;
            try
            {
                // get TabId
                if (context.Request.QueryString["tabid"] != null)
                {
                    int.TryParse(context.Request.QueryString["tabid"], out TabId);
                }

                // get ModuleId
                if (context.Request.QueryString["mid"] != null)
                {
                    int.TryParse(context.Request.QueryString["mid"], out ModuleId);
                }
            }
            catch (Exception)
            {
                // The TabId or ModuleId are incorrectly formatted (potential DOS)
                this.Handle404Exception(context, context.Request.RawUrl);
            }

            // get Language
            string Language = _portalSettings.DefaultLanguage;
            if (context.Request.QueryString["language"] != null)
            {
                Language = context.Request.QueryString["language"];
            }
            else
            {
                if (context.Request.Cookies["language"] != null)
                {
                    Language = context.Request.Cookies["language"].Value;
                }
            }

            if (LocaleController.Instance.IsEnabled(ref Language, _portalSettings.PortalId))
            {
                Localization.SetThreadCultures(new CultureInfo(Language), _portalSettings);
                Localization.SetLanguage(Language);
            }

            // get the URL
            string URL = string.Empty;
            if (context.Request.QueryString["fileticket"] != null)
            {
                URL = "FileID=" + FileLinkClickController.Instance.GetFileIdFromLinkClick(context.Request.QueryString);
            }

            if (context.Request.QueryString["userticket"] != null)
            {
                URL = "UserId=" + UrlUtils.DecryptParameter(context.Request.QueryString["userticket"]);
            }

            if (context.Request.QueryString["link"] != null)
            {
                URL = context.Request.QueryString["link"];
                if (URL.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase))
                {
                    URL = string.Empty; // restrict direct access by FileID
                }
            }

            if (!string.IsNullOrEmpty(URL))
            {
                URL = URL.Replace(@"\", @"/");

                // update clicks, this must be done first, because the url tracker works with unmodified urls, like tabid, fileid etc
                var objUrls = new UrlController();
                objUrls.UpdateUrlTracking(_portalSettings.PortalId, URL, ModuleId, -1);
                TabType UrlType = Globals.GetURLType(URL);
                if (UrlType == TabType.Tab)
                {
                    // verify whether the tab is exist, otherwise throw out 404.
                    if (TabController.Instance.GetTab(int.Parse(URL), _portalSettings.PortalId, false) == null)
                    {
                        this.Handle404Exception(context, context.Request.RawUrl);
                    }
                }

                if (UrlType != TabType.File)
                {
                    URL = Globals.LinkClick(URL, TabId, ModuleId, false);
                }

                if (UrlType == TabType.File && URL.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    // to handle legacy scenarios before the introduction of the FileServerHandler
                    var fileName = Path.GetFileName(URL);

                    var folderPath = URL.Substring(0, URL.LastIndexOf(fileName, StringComparison.InvariantCulture));
                    var folder = FolderManager.Instance.GetFolder(_portalSettings.PortalId, folderPath);

                    var file = FileManager.Instance.GetFile(folder, fileName);

                    URL = "FileID=" + file.FileId;
                }

                // get optional parameters
                bool blnForceDownload = false;
                if ((context.Request.QueryString["forcedownload"] != null) || (context.Request.QueryString["contenttype"] != null))
                {
                    bool.TryParse(context.Request.QueryString["forcedownload"], out blnForceDownload);
                }

                var contentDisposition = blnForceDownload ? ContentDisposition.Attachment : ContentDisposition.Inline;

                // clear the current response
                context.Response.Clear();
                var fileManager = FileManager.Instance;
                try
                {
                    switch (UrlType)
                    {
                        case TabType.File:
                            var download = false;
                            var file = fileManager.GetFile(int.Parse(UrlUtils.GetParameterValue(URL)));
                            if (file != null)
                            {
                                if (!file.IsEnabled || !this.HasAPublishedVersion(file))
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
                                catch (ThreadAbortException) // if call fileManager.WriteFileToResponse ThreadAbortException will shown, should catch it and do nothing.
                                {
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);
                                }
                            }

                            if (!download)
                            {
                                this.Handle404Exception(context, URL);
                            }

                            break;
                        case TabType.Url:
                            // prevent phishing by verifying that URL exists in URLs table for Portal
                            if (objUrls.GetUrl(_portalSettings.PortalId, URL) != null)
                            {
                                context.Response.Redirect(URL, true);
                            }

                            break;
                        default:
                            // redirect to URL
                            context.Response.Redirect(URL, true);
                            break;
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception)
                {
                    this.Handle404Exception(context, URL);
                }
            }
            else
            {
                this.Handle404Exception(context, URL);
            }
        }

        private bool HasAPublishedVersion(IFileInfo file)
        {
            if (file.HasBeenPublished)
            {
                return true;
            }

            // We should allow creator to see the file that is pending to be approved
            var user = UserController.Instance.GetCurrentUserInfo();
            return user != null && user.UserID == file.CreatedByUserID;
        }

        private void Handle404Exception(HttpContext context, string url)
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
