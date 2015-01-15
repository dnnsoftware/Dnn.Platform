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
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class PurchasedExtensions : ModuleUserControlBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Request["fileid"] != null && Request["fileAction"] != null)
            {
                GetFile(Request["fileAction"], Request["fileid"]);
            }
            fetchExtensions.Click += FetchExtensionsClick;

            setupCredentials.Visible = false;
            updateCredentials.Visible = false;
            fetchExtensions.Visible = false;

            setupCredentials.NavigateUrl = ModuleContext.EditUrl("Store");
            updateCredentials.NavigateUrl = ModuleContext.EditUrl("Store");
            Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(ModuleContext.PortalId);
            if (settings.ContainsKey("Store_Username"))
            {
                fetchExtensions.Visible = true;
                updateCredentials.Visible = true;
            }
            else
            {
                setupCredentials.Visible = true;
            }
        }

        private void GetFile(string fileAction, string fileId)
        {
            string fileCheck = Localization.GetString("StoreFile", LocalResourceFile);
            string postData = "";
            Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(ModuleContext.PortalId);
            var ps = new PortalSecurity();
            string username = ps.DecryptString(settings["Store_Username"], Config.GetDecryptionkey());
            string password = ps.DecryptString(settings["Store_Password"], Config.GetDecryptionkey());
            postData = postData + "username=" + username + "&password=" + password + "&fileid=" + fileId;

            WebRequest request = WebRequest.Create(fileCheck);

            request.Method = "POST";
            // Create POST data and convert it to a byte array.

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse wr = request.GetResponse();
            string myfile = "";
            string cd = wr.Headers["Content-Disposition"];
            if (cd != null && cd.Trim() != "" && cd.StartsWith("inline;filename="))
            {
                myfile = cd.Replace("inline;filename=", "");
            }

            var objResponse = HttpContext.Current.Response;

            if (fileAction == "download")
            {
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
        }

        protected void FetchExtensionsClick(object sender, EventArgs e)
        {
            GetExtensions();
        }

        protected void GetExtensions()
        {
            try
            {
                string fileCheck = Localization.GetString("StoreFile", LocalResourceFile);
                string postData = "";
                Stream oStream;
                Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(ModuleContext.PortalId);
                var ps = new PortalSecurity();
                string username = ps.DecryptString(settings["Store_Username"], Config.GetDecryptionkey());
                string password = ps.DecryptString(settings["Store_Password"], Config.GetDecryptionkey());
                postData = postData + "username=" + username + "&password=" + password;

                WebRequest request = WebRequest.Create(fileCheck);

                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse response = request.GetResponse();
                oStream = response.GetResponseStream();
                XmlTextReader oReader;
                XPathDocument oXMLDocument;
                oReader = new XmlTextReader((oStream));


                var dt = new DataTable();
                //instance of a datarow  
                DataRow drow;
                //creating two datacolums Column1 and Column2   
                var dcol1 = new DataColumn("Package", typeof (string));
                var dcol2 = new DataColumn("Filename", typeof (string));
                var dcol3 = new DataColumn("Download", typeof (string));

                var dcol4 = new DataColumn("Deploy", typeof (string));
                //adding datacolumn to datatable  
                dt.Columns.Add(dcol1);
                dt.Columns.Add(dcol2);
                dt.Columns.Add(dcol3);
                dt.Columns.Add(dcol4);
                oReader.XmlResolver = null;
                try
                {
                    oXMLDocument = new XPathDocument(oReader);
                }
                catch (Exception)
                {
                    grdSnow.EmptyDataText = LocalizeString("NoData");
                    grdSnow.DataBind();
                    return;
                }

                var nav = oXMLDocument.CreateNavigator();
                var orderDetailIterator = nav.Select("orders/order/orderdetails/orderdetail");
                var i = 0;
                while (orderDetailIterator.MoveNext())
                {
                    var packageName = orderDetailIterator.Current.GetAttribute("packagename", "").Replace("'", "''").Trim();

                    var filesIterator = orderDetailIterator.Current.Select("files/file");
                    while (filesIterator.MoveNext())
                    {

                        //instance of a datarow  
                        drow = dt.NewRow();
                        //add rows to datatable  
                        dt.Rows.Add(drow);
                        var fileName = filesIterator.Current.GetAttribute("filename", "");
                        var fileId = filesIterator.Current.GetAttribute("fileid", "");
                        var deploy = filesIterator.Current.GetAttribute("deploy", "");
                        //add Column values  
                        dt.Rows[i][dcol1] = packageName;
                        dt.Rows[i][dcol2] = fileName;

                        var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                        dt.Rows[i][dcol3] = "<a class='dnnPrimaryAction' href='" +
                                            Globals.NavigateURL(portalSettings.ActiveTab.TabID, Null.NullString,
                                                                "fileAction",
                                                                "download", "fileid", fileId) + "'>" +
                                            LocalizeString("download") + "</a>";


                        if (deploy == "true")
                        {
                            dt.Rows[i][dcol4] = "<a class='dnnPrimaryAction' href=" + "\"" +
                                                ModuleContext.EditUrl("fileID", fileId, "Download", "package",
                                                                      Server.UrlPathEncode(packageName)) + "\"" + ">" +
                                                LocalizeString("deploy") + "</a>";
                        }
                        else
                        {
                            dt.Rows[i][dcol4] = "N/A";
                        }
                        i = i + 1;
                    }
                }

                grdSnow.DataSource = dt;
                grdSnow.DataBind();
            }
            catch (Exception)
            {
                grdSnow.EmptyDataText = LocalizeString("NoData");
                grdSnow.DataBind();
            }
        }
    }
}