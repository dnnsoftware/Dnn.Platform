#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    public class FileServerHandler : IHttpHandler
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (FileServerHandler));
        #region IHttpHandler Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This handler handles requests for LinkClick.aspx, but only those specifc
        /// to file serving
        /// </summary>
        /// <param name="context">System.Web.HttpContext)</param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cpaterra]	4/19/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void ProcessRequest(HttpContext context)
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            int TabId = -1;
            int ModuleId = -1;
            try
            {
                //get TabId
                if (context.Request.QueryString["tabid"] != null)
                {
                    Int32.TryParse(context.Request.QueryString["tabid"], out TabId);
                }

                //get ModuleId
                if (context.Request.QueryString["mid"] != null)
                {
                    Int32.TryParse(context.Request.QueryString["mid"], out ModuleId);
                }
            }
            catch (Exception)
            {
                //The TabId or ModuleId are incorrectly formatted (potential DOS)
                Exceptions.Exceptions.ProcessHttpException(context.Request);
            }

            //get Language
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
                Localization.Localization.SetThreadCultures(new CultureInfo(Language), _portalSettings);
                Localization.Localization.SetLanguage(Language);
            }

            //get the URL
            string URL = "";
            bool blnClientCache = true;
            bool blnForceDownload = false;
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
                if (URL.ToLowerInvariant().StartsWith("fileid="))
                {
                    URL = ""; //restrict direct access by FileID
                }
            }
            if (!String.IsNullOrEmpty(URL))
            {
                URL = URL.Replace(@"\", @"/");

                //update clicks, this must be done first, because the url tracker works with unmodified urls, like tabid, fileid etc
                var objUrls = new UrlController();
                objUrls.UpdateUrlTracking(_portalSettings.PortalId, URL, ModuleId, -1);
                TabType UrlType = Globals.GetURLType(URL);
                if(UrlType == TabType.Tab)
                {
                    //verify whether the tab is exist, otherwise throw out 404.
                    if(new TabController().GetTab(int.Parse(URL), _portalSettings.PortalId, false) == null)
                    {
                        Exceptions.Exceptions.ProcessHttpException();
                    }
                }
                if (UrlType != TabType.File)
                {
                    URL = Globals.LinkClick(URL, TabId, ModuleId, false);
                }

                if (UrlType == TabType.File && URL.ToLowerInvariant().StartsWith("fileid=") == false)
                {
                    //to handle legacy scenarios before the introduction of the FileServerHandler
                    var fileName = Path.GetFileName(URL);

                    var folderPath = URL.Substring(0, URL.LastIndexOf(fileName));
                    var folder = FolderManager.Instance.GetFolder(_portalSettings.PortalId, folderPath);

                    var file = FileManager.Instance.GetFile(folder, fileName);

                    URL = "FileID=" + file.FileId;
                }

                //get optional parameters
                if (context.Request.QueryString["clientcache"] != null)
                {
                    blnClientCache = bool.Parse(context.Request.QueryString["clientcache"]);
                }
                if ((context.Request.QueryString["forcedownload"] != null) || (context.Request.QueryString["contenttype"] != null))
                {
                    blnForceDownload = bool.Parse(context.Request.QueryString["forcedownload"]);
                }
                var contentDisposition = blnForceDownload ? ContentDisposition.Attachment : ContentDisposition.Inline;

                //clear the current response
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
                                if (!file.IsEnabled)
                                {
                                    if (context.Request.IsAuthenticated)
                                    {
                                        context.Response.Redirect(Globals.AccessDeniedURL(Localization.Localization.GetString("FileAccess.Error")), true);
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
                                        context.Response.Redirect(Globals.AccessDeniedURL(Localization.Localization.GetString("FileAccess.Error")), true);
                                    }
                                    else
                                    {
                                        context.Response.Redirect(Globals.AccessDeniedURL(), true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);
                                }
                            }

                            if (!download)
                            {
                                Exceptions.Exceptions.ProcessHttpException(URL);
                            }
                            break;
                        case TabType.Url:
                            //prevent phishing by verifying that URL exists in URLs table for Portal
                            if (objUrls.GetUrl(_portalSettings.PortalId, URL) != null)
                            {
                                context.Response.Redirect(URL, true);
                            }
                            break;
                        default:
                            //redirect to URL
                            context.Response.Redirect(URL, true);
                            break;
                    }
                }
                catch (ThreadAbortException exc)
                {
                    Logger.Error(exc);
                }
                catch (Exception)
                {
                    Exceptions.Exceptions.ProcessHttpException(URL);
                }
            }
            else
            {
                Exceptions.Exceptions.ProcessHttpException(URL);
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}