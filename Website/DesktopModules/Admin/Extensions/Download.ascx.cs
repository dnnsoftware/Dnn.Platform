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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class Download : ModuleUserControlBase
    {
        public static int BufferSize = 1024;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cancel.Click += cancel_Click;
            deployExtension.Click += deployExtension_Click;
            downloadExtension.Click += downloadExtension_Click;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                if (Request.QueryString["ExtensionID"]!=null)
                {
                    ExtensionRequest();
                    return;
                }
                else
                {
                    FileRequest();
                    return;
                }
            }
        }

            private void FileRequest()
            {
                if (Request["package"]!=null)
                {
                    ViewState["extName"] = Request["package"].ToString();    
                }
                else
                {
                    ViewState["extName"] = "N/A";    
                }
                
                ViewState["extType"] = "Module";
                ViewState["extURL"] = Localization.GetString("StoreFile", LocalResourceFile);
                ViewState["fileId"] = Request.QueryString["FileID"].ToString();
                extensionType.Text = ViewState["extType"].ToString();
                extensionName.Text = ViewState["extName"].ToString();
                extensionDesc.Text = "N/A";
                downloadExtension.Visible = true;
                deployExtension.Visible = true;
                
            }

        private void ExtensionRequest()
        {
            var extensionId = Request.QueryString["ExtensionID"];
            var extensionRequest = "http://" + LocalizeString("feedEndpoint") + "/AppGalleryService.svc/Extensions(" + extensionId + ")";
                
            var xmlDoc = new XmlDocument();
            var xml = GetOData(extensionRequest);

            var xmlNsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            xmlNsMgr.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            xmlNsMgr.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
            xmlDoc.LoadXml(xml);

            if (xmlDoc.DocumentElement != null)
            {
                var elements = xmlDoc.DocumentElement.SelectNodes("/atom:entry", xmlNsMgr);
                string extName="";
                string extType = "";
                string extDesc = "";
                string extURL = "";
                string extCatalogID = "";
                foreach (XmlNode element in elements)
                {
                    var properties = element.SelectSingleNode("./atom:content/m:properties", xmlNsMgr).ChildNodes;
                
                    foreach (XmlNode property in properties)
                    {
                        string propertyName = property.LocalName;
                        switch (propertyName)
                        {
                            case "ExtensionName":
                                extName = property.InnerText;
                                ViewState["extName"] = extName;
                                break;
                            case "ExtensionType":
                                extType = property.InnerText;
                                ViewState["extType"] = extType;
                                break;
                            case "Description":
                                extDesc = property.InnerText;
                                break;
                            case "DownloadURL":
                                extURL = property.InnerText;
                                ViewState["extURL"] = extURL;
                                break;
                            case "CatalogID":
                                extCatalogID = property.InnerText;
                                break;
                            default:
                                break;   
                        }
                    }
                }

                extensionType.Text = extType;
                extensionName.Text = extName;
                extensionDesc.Text = extDesc;

                if (extURL == "")
                {
                    UI.Skins.Skin.AddModuleMessage(this, GetString("unexpectedRequest"), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }
                downloadExtension.Visible = true;
                if (extCatalogID=="2")
                {
                    deployExtension.Visible = true;
                
                }
            }
        }

        private void ProcessRequest(string action, bool doInstall)
        {
            var downloadURL = ViewState["extURL"].ToString();
            var extensionFolder = GetInstallationFolder(ViewState["extType"].ToString());
            var installFolder = HttpContext.Current.Server.MapPath("~/Install/") + extensionFolder;

            if (downloadURL.Contains("codeplex.com"))
            {
                ProcessCodeplex(downloadURL, installFolder, action);
            }
            else if (downloadURL.Contains("snowcovered.com"))
            {
                ProcessSnowcovered(downloadURL, installFolder, action);
            }
            else
            {
                ProcessUnknown(downloadURL, installFolder, action);
            }
        }

        private static string GetOData(string extensionRequest)
        {
            var request = Globals.GetExternalRequest(extensionRequest);
            request.Method = "GET";
            request.Accept = "application/atom+xml";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var readStream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                {
                    return readStream.ReadToEnd();
                }
            }
 
        }

        private void ProcessUnknown(string downloadURL, string installFolder, string catalogAction)
        {
            string myfile = "";
            var wr = HttpAsWebResponse(downloadURL,
                                   null,
                                   null,
                                   null,
                                   null,
                                   null,
                                   -1,
                                   false,
                                   "DotNetNuke-Appgallery/1.0.0.0(Microsoft Windows NT 6.1.7600.0",
                                   "wpi://2.1.0.0/Microsoft Windows NT 6.1.7600.0",
                                   out myfile);
            DownloadDeploy(wr, myfile, installFolder, catalogAction);
        }

        private void ProcessSnowcovered(string downloadURL, string installFolder, string catalogAction)
        {
            string fileCheck = downloadURL;
            string postData = "";
            Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(ModuleContext.PortalId);
            PortalSecurity ps = new PortalSecurity();
            string username = ps.DecryptString(settings["Store_Username"], Config.GetDecryptionkey());
            string password = ps.DecryptString(settings["Store_Password"], Config.GetDecryptionkey());
            postData = postData + "username=" + username + "&password=" + password + "&fileid=" + ViewState["fileId"].ToString();

            WebRequest request = Globals.GetExternalRequest(fileCheck);

            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();

            string myfile = "";

            var cd = response.Headers["Content-Disposition"];
            if (cd != null && cd.Trim() != "" && cd.StartsWith("inline;filename="))
            {
                myfile =cd.Replace("inline;filename=", "");
            }

            DownloadDeploy(response, myfile, installFolder, catalogAction);

            UI.Skins.Skin.AddModuleMessage(this, String.Format(GetString("deploySuccess"), ViewState["extName"]), ModuleMessage.ModuleMessageType.GreenSuccess);
            installExtension.NavigateUrl = Util.InstallURL(ModuleContext.TabId, "", ViewState["extType"].ToString(), myfile.ToLower().Replace(".zip", ".resources").ToString());
            installExtension.Visible = true;
            deployExtension.Visible = false;
        }

        private void ProcessCodeplex(string downloadURL, string installFolder, string catalogAction)
        {
            string myfile = "";
            try
            {
                var url = new Uri(downloadURL);
                var host = url.Host;

                //convert path to download version
                string directdownloadURL = "";
                if (downloadURL.Contains("#DownloadId="))
                {
                    int start = downloadURL.IndexOf("DownloadId=");
                    directdownloadURL = "http://" + host + "/Project/Download/FileDownload.aspx?" + downloadURL.Substring(start);
                }
                else
                {
                    directdownloadURL = downloadURL;
                }
                var wr = HttpAsWebResponse(directdownloadURL,
                                       null,
                                       null,
                                       null,
                                       null,
                                       null,
                                       -1,
                                       false,
                                       "DotNetNuke-Appgallery/1.0.0.0(Microsoft Windows NT 6.1.7600.0",
                                       "wpi://2.1.0.0/Microsoft Windows NT 6.1.7600.0",
                                       out myfile);
                DownloadDeploy(wr, myfile, installFolder, catalogAction);

                UI.Skins.Skin.AddModuleMessage(this, String.Format(GetString("deploySuccess"), ViewState["extName"]), ModuleMessage.ModuleMessageType.GreenSuccess);
                installExtension.NavigateUrl = Util.InstallURL(ModuleContext.TabId, "", ViewState["extType"].ToString(), myfile.ToLower().Replace(".zip", ".resources").ToString());
                installExtension.Visible = true;
                deployExtension.Visible = false;
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
 
        }

        private void DownloadDeploy(WebResponse wr, string myfile, string installFolder, string catalogAction)
        {
            if (catalogAction == "download")
            {
                var objResponse = HttpContext.Current.Response;
                var aByteArray = new byte[wr.ContentLength];
                objResponse.AppendHeader("Content-Disposition", "attachment; filename=\"" + myfile + "\"");
                objResponse.AppendHeader("Content-Length", wr.ContentLength.ToString());
                objResponse.ContentType = wr.ContentType;

                const int bufferLength = 4096;
                byte[] byteBuffer = new byte[bufferLength];
                Stream rs = wr.GetResponseStream();
                int len = 0;
                while ((len = rs.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                {
                    if (len < bufferLength)
                    { objResponse.BinaryWrite(byteBuffer.Take(len).ToArray()); }
                    else
                    { objResponse.BinaryWrite(byteBuffer); }
                    objResponse.Flush();
                }
                
            }
            else
            {
                Stream remoteStream = null;
                Stream localStream = null;

                try
                {
                    // Once the WebResponse object has been retrieved,
                    // get the stream object associated with the response's data
                    remoteStream = wr.GetResponseStream();

                    // Create the local file
                    localStream = File.Create(installFolder + "/" + myfile.ToLower().Replace(".zip",".resources"));

                    // Allocate a 1k buffer
                    var buffer = new byte[1024];
                    int bytesRead;

                    // Simple do/while loop to read from stream until
                    // no bytes are returned
                    do
                    {
                        // Read data (up to 1k) from the stream
                        bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                        // Write the data to the local file
                        localStream.Write(buffer, 0, bytesRead);

                        // Increment total bytes processed
                        //TODO fix this line bytesProcessed += bytesRead;
                    } while (bytesRead > 0);
                }
                finally
                {
                    // Close the response and streams objects here 
                    // to make sure they're closed even if an exception
                    // is thrown at some point
                    if (remoteStream != null)
                    {
                        remoteStream.Close();
                    }
                    if (localStream != null)
                    {
                        localStream.Close();
                    }
                }
            }
        }

        public static WebResponse HttpAsWebResponse(string URL, byte[] Data, string Username, string Password, string Domain, string ProxyAddress, int ProxyPort, bool DoPOST, string UserAgent, string Referer, out string Filename)
        {
            if (!DoPOST && Data != null && Data.Length > 0)
            {
                var restoftheurl = Encoding.ASCII.GetString(Data);
                if (URL.IndexOf("?") <= 0)
                {
                    URL = URL + "?";
                }
                URL = URL + restoftheurl;
            }

            var wreq = Globals.GetExternalRequest(URL);
            wreq.UserAgent = UserAgent;
            wreq.Referer = Referer;
            wreq.Method = "GET";
            if (DoPOST)
            {
                wreq.Method = "POST";
            }
           
            wreq.Timeout = Host.WebRequestTimeout;
            
            if (!string.IsNullOrEmpty(Host.ProxyServer))
            {
                var proxy = new WebProxy(Host.ProxyServer, Host.ProxyPort);
                if (!string.IsNullOrEmpty(Host.ProxyUsername))
                {
                    var proxyCredentials = new NetworkCredential(Host.ProxyUsername, Host.ProxyPassword);
                    proxy.Credentials = proxyCredentials;
                }
                wreq.Proxy = proxy;
            }

            if (Username != null && Password != null && Domain != null && Username.Trim() != "" && Password.Trim() != null && Domain.Trim() != null)
            {
                wreq.Credentials = new NetworkCredential(Username, Password, Domain);
            }
            else if (Username != null && Password != null && Username.Trim() != "" && Password.Trim() != null)
            {
                wreq.Credentials = new NetworkCredential(Username, Password);
            }

            if (DoPOST && Data != null && Data.Length > 0)
            {
                wreq.ContentType = "application/x-www-form-urlencoded";
                var request = wreq.GetRequestStream();
                request.Write(Data, 0, Data.Length);
                request.Close();
            }

            Filename = "";
            var wrsp = wreq.GetResponse();
            var cd = wrsp.Headers["Content-Disposition"];
            if (cd != null && cd.Trim() != string.Empty && cd.StartsWith("attachment"))
            {
                if (cd.IndexOf("filename") > -1 && cd.Substring(cd.IndexOf("filename")).IndexOf("=") > -1)
                {
                    var filenameParam = cd.Substring(cd.IndexOf("filename"));

                    if (filenameParam.IndexOf("\"") > -1)
                    {
                        Filename = filenameParam.Substring(filenameParam.IndexOf("\"") + 1).TrimEnd(Convert.ToChar("\"")).TrimEnd(Convert.ToChar("\\"));
                    }
                    else
                    {
                        Filename = filenameParam.Substring(filenameParam.IndexOf("=") + 1);
                    }
                }
            }
            return wrsp;
        }

        private static string GetInstallationFolder(string extensionType)
        {
            var extensionFolder = "";
            switch (extensionType)
            {
                case "Library":
                    extensionFolder = "Module";
                    break;
                case "Module":
                    extensionFolder = "Module";
                    break;
                case "Provider":
                    extensionFolder = "Provider";
                    break;
                case "Skin":
                    extensionFolder = "Skin";
                    break;
                case "Skin Object":
                    extensionFolder = "Skin";
                    break;
                case "Widget":
                    extensionFolder = "Module";
                    break;
                case "Other":
                    extensionFolder = "Module";
                    break;
            }
            return extensionFolder;
        }

        protected string GetString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        protected void downloadExtension_Click(object sender, EventArgs e)
        {
            ProcessRequest("download",false);
        }

        protected void deployExtension_Click(object sender, EventArgs e)
        {
            ProcessRequest("deploy", false);
        }

        protected void cancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(), true);
        }
}
}