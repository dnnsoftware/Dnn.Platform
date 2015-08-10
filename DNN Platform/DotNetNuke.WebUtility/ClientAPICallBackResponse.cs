#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
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

using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace DotNetNuke.UI.Utilities
{

	public class ClientAPICallBackResponse
	{
		public enum CallBackResponseStatusCode
		{
			OK = 200,
			GenericFailure = 400,
			ControlNotFound = 404,
			InterfaceNotSupported = 501
		}

		public enum CallBackTypeCode
		{
			Simple = 0,
			ProcessPage = 1,
			CallbackMethod = 2,
			ProcessPageCallbackMethod = 3
			//XMLRPC
		}

		public enum TransportTypeCode
		{
			XMLHTTP,
			IFRAMEPost
		}

		public string Response = "";
		public CallBackResponseStatusCode StatusCode;
		public string StatusDesc = "";
		private Page m_objPage;

		public CallBackTypeCode CallBackType;
		public TransportTypeCode TransportType {
			get {
				if (!string.IsNullOrEmpty(m_objPage.Request.Form["ctx"])) {
					return TransportTypeCode.IFRAMEPost;
				} else {
					return TransportTypeCode.XMLHTTP;
				}
			}
		}

		public ClientAPICallBackResponse(Page objPage, CallBackTypeCode eCallBackType)
		{
			m_objPage = objPage;
			CallBackType = eCallBackType;
		}

		public void Write()
		{
			switch (this.TransportType) {
				case TransportTypeCode.IFRAMEPost:
					string strContextID = m_objPage.Request.Form["ctx"];
					//if context passed in then we are using IFRAME Implementation
					if (Regex.IsMatch(strContextID, "^\\d+$")) {
						m_objPage.Response.Write("<html><head></head><body onload=\"window.parent.dnn.xmlhttp.requests['" + strContextID + "'].complete(window.parent.dnn.dom.getById('txt', document).value);\"><form>");
						m_objPage.Response.Write("<input type=\"hidden\" id=\"" + ClientAPI.SCRIPT_CALLBACKSTATUSID + "\" value=\"" + Convert.ToInt32(this.StatusCode).ToString() + "\">");
						m_objPage.Response.Write("<input type=\"hidden\" id=\"" + ClientAPI.SCRIPT_CALLBACKSTATUSDESCID + "\" value=\"" + this.StatusDesc + "\">");
						m_objPage.Response.Write("<textarea id=\"txt\">");
						m_objPage.Response.Write(HttpUtility.HtmlEncode(MSAJAX.Serialize(new { d = Response })));
						m_objPage.Response.Write("</textarea></body></html>");
					}
					break;
				case TransportTypeCode.XMLHTTP:
					m_objPage.Response.AppendHeader(ClientAPI.SCRIPT_CALLBACKSTATUSID, Convert.ToInt32(this.StatusCode).ToString());
					m_objPage.Response.AppendHeader(ClientAPI.SCRIPT_CALLBACKSTATUSDESCID, this.StatusDesc);

					m_objPage.Response.Write(MSAJAX.Serialize(new { d = Response }));
					////don't serialize straight html
					break;
			}

		}
	}

}
