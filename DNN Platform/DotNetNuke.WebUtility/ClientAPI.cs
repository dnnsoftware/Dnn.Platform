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
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.UI.Utilities
{

	/// -----------------------------------------------------------------------------
	/// Project	 : DotNetNuke
	/// Class	 : ClientAPI
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// Library responsible for interacting with DNN Client API.
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// 	[Jon Henning]	8/3/2004	Created
	/// </history>
	/// -----------------------------------------------------------------------------
	public class ClientAPI
	{

		#region "Public Constants"

		public const string SCRIPT_CALLBACKID = "__DNNCAPISCI";
		public const string SCRIPT_CALLBACKTYPE = "__DNNCAPISCT";
		public const string SCRIPT_CALLBACKPARAM = "__DNNCAPISCP";
		public const string SCRIPT_CALLBACKPAGEID = "__DNNCAPISCPAGEID";
		public const string SCRIPT_CALLBACKSTATUSID = "__DNNCAPISCSI";

		public const string SCRIPT_CALLBACKSTATUSDESCID = "__DNNCAPISCSDI";
			#endregion
		public const string DNNVARIABLE_CONTROLID = "__dnnVariable";

		#region "Public Enums"

		public enum ClientFunctionality : int
		{
			DHTML,
			XML,
			XSLT,
			Positioning,
			//what we would call adaquate positioning support
			XMLJS,
			XMLHTTP,
			XMLHTTPJS,
			SingleCharDelimiters,
			UseExternalScripts,
			Motion
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Enumerates each namespace with a seperate js file
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/3/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public enum ClientNamespaceReferences : int
		{
			dnn,
			dnn_dom,
			dnn_dom_positioning,
			dnn_xml,
			dnn_xmlhttp,
			dnn_motion
		}

		#endregion

		#region "Private Shared Members"

		/// -----------------------------------------------------------------------------
		/// <summary>Private variable holding location of client side js files.  Shared by entire application.</summary>
		/// -----------------------------------------------------------------------------

		private static string m_sScriptPath;

		private static string m_ClientAPIDisabled = string.Empty;
		#endregion

		#region "Private Shared Properties"

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Finds __dnnVariable control on page, if not found it attempts to add its own.
		/// </summary>
		/// <param name="objPage">Current page rendering content</param>
		/// <value></value>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/3/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		private static HtmlInputHidden ClientVariableControl(Page page) {
			return RegisterDNNVariableControl(page);
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Loop up parent controls to find form
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	2/2/2006	Commented
		/// </history>
		/// -----------------------------------------------------------------------------
		private static Control FindForm(Control oCtl)
		{
			return oCtl.Page.Form;
			//Do While Not TypeOf oCtl Is HtmlControls.HtmlForm
			//    If oCtl Is Nothing OrElse TypeOf oCtl Is Page Then Return Nothing
			//    oCtl = oCtl.Parent
			//Loop
			//Return oCtl
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Returns __dnnVariable control if present
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	4/6/2005	Commented
		/// </history>
		/// -----------------------------------------------------------------------------
		private static HtmlInputHidden GetDNNVariableControl(Control objParent)
		{
			return (System.Web.UI.HtmlControls.HtmlInputHidden)DotNetNuke.UI.Utilities.Globals.FindControlRecursive(objParent.Page, DNNVARIABLE_CONTROLID);
		}

		#endregion

		#region "Public Shared Properties"

		/// -----------------------------------------------------------------------------
		/// <summary>Character used for delimiting name from value</summary>
		/// -----------------------------------------------------------------------------
		public static string COLUMN_DELIMITER {
			get {
				if (BrowserSupportsFunctionality(ClientFunctionality.SingleCharDelimiters)) {
					return ((char)18).ToString();
				} else {
					return "~|~";
				}
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>Character used for delimiting name from value</summary>
		/// -----------------------------------------------------------------------------
		public static string CUSTOM_COLUMN_DELIMITER {
			get {
				if (BrowserSupportsFunctionality(ClientFunctionality.SingleCharDelimiters)) {
					return ((char)16).ToString();
				} else {
					return "~.~";
				}
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>Character used for delimiting name/value pairs</summary>
		/// -----------------------------------------------------------------------------
		public static string CUSTOM_ROW_DELIMITER {
			get {
				if (BrowserSupportsFunctionality(ClientFunctionality.SingleCharDelimiters)) {
					return ((char)15).ToString();
				} else {
					return "~,~";
				}
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>In order to reduce payload, substitute out " with different char, since when put in a hidden control it uses &quot;</summary>
		/// -----------------------------------------------------------------------------
		public static string QUOTE_REPLACEMENT {
			get {
				if (BrowserSupportsFunctionality(ClientFunctionality.SingleCharDelimiters)) {
					return ((char)19).ToString();
				} else {
					return "~!~";
				}
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>Character used for delimiting name/value pairs</summary>
		/// -----------------------------------------------------------------------------
		public static string ROW_DELIMITER {
			get {
				if (BrowserSupportsFunctionality(ClientFunctionality.SingleCharDelimiters)) {
					return ((char)17).ToString();
				} else {
					return "~`~";
				}
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Path where js files are placed
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/19/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static string ScriptPath {
			get {
				string script = "";
				if (!string.IsNullOrEmpty(m_sScriptPath)) {
					script = m_sScriptPath;
				} else if ((System.Web.HttpContext.Current != null)) {
					if (System.Web.HttpContext.Current.Request.ApplicationPath.EndsWith("/")) {
						script = System.Web.HttpContext.Current.Request.ApplicationPath + "js/";
					} else {
						script = System.Web.HttpContext.Current.Request.ApplicationPath + "/js/";
					}
				}
				return script;
			}
			set { m_sScriptPath = value; }
		}

		public static bool UseExternalScripts {
			get { return BrowserSupportsFunctionality(ClientFunctionality.UseExternalScripts); }
		}

		public static string EscapeForJavascript(string s)
		{
			return s.Replace("\\", "\\\\").Replace("'", "\\'");
		}

		#endregion

		#region "Private Shared Methods"

		private static void AddAttribute(Control objControl, string strName, string strValue)
		{
			if (objControl is HtmlControl) {
				((HtmlControl)objControl).Attributes.Add(strName, strValue);
			} else if (objControl is WebControl) {
				((WebControl)objControl).Attributes.Add(strName, strValue);
			}
		}

		public static Dictionary<string, string> GetClientVariableList(Page objPage)
		{
			HtmlInputHidden ctlVar = ClientVariableControl(objPage);
			string strValue = "";
			if ((ctlVar != null))
				strValue = ctlVar.Value;
			if (string.IsNullOrEmpty(strValue))
				strValue = System.Web.HttpContext.Current.Request[DNNVARIABLE_CONTROLID];
			//using request object in case we are loading before controls have values set
			if (strValue == null)
				strValue = "";

			Dictionary<string, string> objDict = (Dictionary<string, string>)HttpContext.Current.Items["CAPIVariableList"];
			if (objDict == null) {
				//Dim objJSON As Script.Serialization.JavaScriptSerializer = New Script.Serialization.JavaScriptSerializer()
				if (string.IsNullOrEmpty(strValue) == false) {
					try {
						//fix serialization issues with invalid json objects
						if (strValue.IndexOf("`") == 0) {
							strValue = strValue.Substring(1).Replace("`", "\"");
						}

						objDict = MSAJAX.Deserialize<Dictionary<string, string>>(strValue);
						//objJSON.Deserialize(Of Generic.Dictionary(Of String, String))(strValue)
					} catch {
						//ignore error
					}
				}
				if (objDict == null) {
					objDict = new System.Collections.Generic.Dictionary<string, string>();
				}
				HttpContext.Current.Items["CAPIVariableList"] = objDict;
			}

			return objDict;
		}

		public static void SerializeClientVariableDictionary(Page objPage, System.Collections.Generic.Dictionary<string, string> objDict)
		{
			HtmlInputHidden ctlVar = ClientVariableControl(objPage);
			ctlVar.Value = MSAJAX.Serialize(objDict);
			//minimize payload by using ` for ", which serializes to &quot;
			if (ctlVar.Value.IndexOf("`") == -1) {
				//prefix the value with ` to denote that we escaped it (it was safe)
				ctlVar.Value = "`" + ctlVar.Value.Replace("\"", "`");
			}

		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Parses DNN Variable control contents and returns out the delimited name/value pair
		/// </summary>
		/// <param name="objPage">Current page rendering content</param>
		/// <param name="strVar">Name to retrieve</param>
		/// <returns>Delimited name/value pair string</returns>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/3/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		private static string GetClientVariableNameValuePair(Page objPage, string strVar)
		{
			IDictionary<string, string> objDict = GetClientVariableList(objPage);
			if (objDict.ContainsKey(strVar)) {
				return strVar + COLUMN_DELIMITER + objDict[strVar];
			}
			return "";
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Returns javascript to call dnncore.js key handler logic
		/// </summary>
		/// <param name="intKeyAscii">ASCII value to trap</param>
		/// <param name="strJavascript">Javascript to execute</param>
		/// <returns>Javascript to handle key press</returns>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	2/17/2005	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		private static string GetKeyDownHandler(int intKeyAscii, string strJavascript)
		{
			return "return __dnn_KeyDown('" + intKeyAscii + "', '" + strJavascript.Replace("'", "%27") + "', event);";
		}

		#endregion

		#region "Public Shared Methods"

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Common way to handle confirmation prompts on client
		/// </summary>
		/// <param name="objButton">Button to trap click event</param>
		/// <param name="strText">Text to display in confirmation</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	2/17/2005	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static void AddButtonConfirm(WebControl objButton, string strText)
		{
			objButton.Attributes.Add("onClick", "javascript:return confirm('" + GetSafeJSString(strText) + "');");
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Determines of browser currently requesting page adaquately supports passed un client-side functionality
		/// </summary>
		/// <param name="eFunctionality">Desired Functionality</param>
		/// <returns>True when browser supports it</returns>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/3/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static bool BrowserSupportsFunctionality(ClientFunctionality eFunctionality)
		{
			try {
				if (System.Web.HttpContext.Current == null)
					return true;
				bool blnSupports = false;

				if (ClientAPIDisabled() == false) {
					BrowserCaps objCaps = BrowserCaps.GetBrowserCaps();
					if ((objCaps != null)) {
						HttpRequest objRequest = System.Web.HttpContext.Current.Request;
						string strUserAgent = objRequest.UserAgent;
						if (!string.IsNullOrEmpty(strUserAgent))
						{
							//First check whether we have checked this browser before
							if (objCaps.FunctionalityDictionary.ContainsKey(strUserAgent) == false) {
								string strBrowser = objRequest.Browser.Browser;
								//if no version present, Framework dies, hence need for try 
								double dblVersion = Convert.ToDouble(objRequest.Browser.MajorVersion + objRequest.Browser.MinorVersion);
								int iBitValue = 0;
								FunctionalityInfo objFuncInfo = null;
								//loop through all functionalities for this UserAgent and determine the bitvalue 
								foreach (ClientFunctionality eFunc in System.Enum.GetValues(typeof(ClientFunctionality))) {
									objFuncInfo = objCaps.Functionality[eFunc];
									if ((objFuncInfo != null) && objFuncInfo.HasMatch(strUserAgent, strBrowser, dblVersion)) {
										iBitValue += (int)eFunc;
									}
								}
								objCaps.FunctionalityDictionary[strUserAgent] = iBitValue;
							}
							blnSupports = ((int)objCaps.FunctionalityDictionary[strUserAgent] + eFunctionality) != 0;
						}
					}
				}
				return blnSupports;
			} catch {
				//bad user agent (CAP-7321)
			}

			return false;

		}

		public static string GetCallbackEventReference(Control objControl, string strArgument, string strClientCallBack, string strContext, string srtClientErrorCallBack)
		{
			return GetCallbackEventReference(objControl, strArgument, strClientCallBack, strContext, srtClientErrorCallBack, null, "");
		}

		public static string GetCallbackEventReference(Control objControl, string strArgument, string strClientCallBack, string strContext, string srtClientErrorCallBack, ClientAPICallBackResponse.CallBackTypeCode eCallbackType)
		{
			return GetCallbackEventReference(objControl, strArgument, strClientCallBack, strContext, srtClientErrorCallBack, null, null, eCallbackType);
		}

		public static string GetCallbackEventReference(Control objControl, string strArgument, string strClientCallBack, string strContext, string srtClientErrorCallBack, Control objPostChildrenOf)
		{
			return GetCallbackEventReference(objControl, strArgument, strClientCallBack, strContext, srtClientErrorCallBack, null, objPostChildrenOf.ClientID, ClientAPICallBackResponse.CallBackTypeCode.Simple);
		}

		public static string GetCallbackEventReference(Control objControl, string strArgument, string strClientCallBack, string strContext, string srtClientErrorCallBack, string strClientStatusCallBack)
		{
			return GetCallbackEventReference(objControl, strArgument, strClientCallBack, strContext, srtClientErrorCallBack, strClientStatusCallBack, null, ClientAPICallBackResponse.CallBackTypeCode.Simple);
		}

		public static string GetCallbackEventReference(Control objControl, string strArgument, string strClientCallBack, string strContext, string srtClientErrorCallBack, string strClientStatusCallBack, ClientAPICallBackResponse.CallBackTypeCode eCallbackType)
		{
			return GetCallbackEventReference(objControl, strArgument, strClientCallBack, strContext, srtClientErrorCallBack, strClientStatusCallBack, null, eCallbackType);
		}

		public static string GetCallbackEventReference(Control objControl, string strArgument, string strClientCallBack, string strContext, string srtClientErrorCallBack, string strClientStatusCallBack, Control objPostChildrenOf)
		{
			return GetCallbackEventReference(objControl, strArgument, strClientCallBack, strContext, srtClientErrorCallBack, strClientStatusCallBack, objPostChildrenOf.ClientID, ClientAPICallBackResponse.CallBackTypeCode.Simple);
		}

		public static string GetCallbackEventReference(Control objControl, string strArgument, string strClientCallBack, string strContext, string srtClientErrorCallBack, string strClientStatusCallBack, string strPostChildrenOfId)
		{
			return GetCallbackEventReference(objControl, strArgument, strClientCallBack, strContext, srtClientErrorCallBack, strClientStatusCallBack, strPostChildrenOfId, ClientAPICallBackResponse.CallBackTypeCode.Simple);
		}
		public static string GetCallbackEventReference(Control objControl, string strArgument, string strClientCallBack, string strContext, string srtClientErrorCallBack, string strClientStatusCallBack, string strPostChildrenOfId, ClientAPICallBackResponse.CallBackTypeCode eCallbackType)
		{
			string strCallbackType = Convert.ToInt32(eCallbackType).ToString();
			if (strArgument == null)
				strArgument = "null";
			if (strContext == null)
				strContext = "null";
			if (srtClientErrorCallBack == null)
				srtClientErrorCallBack = "null";
			if (strClientStatusCallBack == null)
				strClientStatusCallBack = "null";
			if (string.IsNullOrEmpty(strPostChildrenOfId))
			{
				strPostChildrenOfId = "null";
			} else if (strPostChildrenOfId.StartsWith("'") == false) {
				strPostChildrenOfId = "'" + strPostChildrenOfId + "'";
			}
			string strControlID = objControl.ID;
			if (BrowserSupportsFunctionality(ClientFunctionality.XMLHTTP) && BrowserSupportsFunctionality(ClientFunctionality.XML)) {
				DotNetNuke.UI.Utilities.ClientAPI.RegisterClientReference(objControl.Page, DotNetNuke.UI.Utilities.ClientAPI.ClientNamespaceReferences.dnn_xml);
				DotNetNuke.UI.Utilities.ClientAPI.RegisterClientReference(objControl.Page, DotNetNuke.UI.Utilities.ClientAPI.ClientNamespaceReferences.dnn_xmlhttp);

				//page doesn't usually have an ID so we need to make one up
				if ((objControl) is Page && string.IsNullOrEmpty(strControlID))
				{
					strControlID = SCRIPT_CALLBACKPAGEID;
				}

				if ((objControl) is Page == false) {
					strControlID = strControlID + " " + objControl.ClientID;
					//ID is not unique (obviously)
				}

				return string.Format("dnn.xmlhttp.doCallBack('{0}',{1},{2},{3},{4},{5},{6},{7},{8});", strControlID, strArgument, strClientCallBack, strContext, srtClientErrorCallBack, strClientStatusCallBack, "null", strPostChildrenOfId, strCallbackType);
			} else {
				return "";
			}

		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Retrieves DNN Client Variable value
		/// </summary>
		/// <param name="objPage">Current page rendering content</param>
		/// <param name="strVar">Variable name to retrieve value for</param>
		/// <returns>Value of variable</returns>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/3/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static string GetClientVariable(Page objPage, string strVar)
		{
			string strPair = GetClientVariableNameValuePair(objPage, strVar);
			if (strPair.IndexOf(COLUMN_DELIMITER) > -1) {
				return Regex.Split(strPair, COLUMN_DELIMITER)[1];
			} else {
				return "";
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Retrieves DNN Client Variable value
		/// </summary>
		/// <param name="objPage">Current page rendering content</param>
		/// <param name="strVar">Variable name to retrieve value for</param>
		/// <param name="strDefaultValue">Default value if variable not found</param>
		/// <returns>Value of variable</returns>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/3/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static string GetClientVariable(Page objPage, string strVar, string strDefaultValue)
		{
			string strPair = GetClientVariableNameValuePair(objPage, strVar);
			if (strPair.IndexOf(COLUMN_DELIMITER) > -1) {
				return Regex.Split(strPair, COLUMN_DELIMITER)[1];
			} else {
				return strDefaultValue;
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Escapes string to be safely used in client side javascript.  
		/// </summary>
		/// <param name="strString">String to escape</param>
		/// <returns>Escaped string</returns>
		/// <remarks>
		/// Currently this only escapes out quotes and apostrophes
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	2/17/2005	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static string GetSafeJSString(string strString)
		{
			if (string.IsNullOrEmpty(strString))
			{
				//Return System.Text.RegularExpressions.Regex.Replace(strString, "(['""])", "\$1")
				return System.Text.RegularExpressions.Regex.Replace(strString, "(['\"\\\\])", "\\$1");
			} else {
				return strString;
			}
		}

		public static bool IsInCallback(Page objPage)
		{
			return !string.IsNullOrEmpty(objPage.Request[SCRIPT_CALLBACKID]) && objPage.Request.HttpMethod.ToUpper() == "POST";
		}

		public static void HandleClientAPICallbackEvent(Page objPage)
		{
			HandleClientAPICallbackEvent(objPage, GetCallbackType(objPage));
		}

		private static ClientAPICallBackResponse.CallBackTypeCode GetCallbackType(Page objPage)
		{
			ClientAPICallBackResponse.CallBackTypeCode eType = ClientAPICallBackResponse.CallBackTypeCode.Simple;
			if (!string.IsNullOrEmpty(objPage.Request[SCRIPT_CALLBACKTYPE]))
			{
				eType = (ClientAPICallBackResponse.CallBackTypeCode)Enum.Parse(typeof(ClientAPICallBackResponse.CallBackTypeCode), objPage.Request[SCRIPT_CALLBACKTYPE]);
			}
			return eType;
		}

		public static void HandleClientAPICallbackEvent(Page objPage, ClientAPICallBackResponse.CallBackTypeCode eType)
		{
			if (IsInCallback(objPage)) {
				switch (eType) {
					case ClientAPICallBackResponse.CallBackTypeCode.Simple:
					case ClientAPICallBackResponse.CallBackTypeCode.CallbackMethod:
						string[] arrIDs = objPage.Request[SCRIPT_CALLBACKID].Split(Convert.ToChar(" "));
						string strControlID = arrIDs[0];
						string strClientID = "";
						if (arrIDs.Length > 1) {
							strClientID = arrIDs[1];
						}

						string strParam = objPage.Server.UrlDecode(objPage.Request[SCRIPT_CALLBACKPARAM]);
						Control objControl = null;
						IClientAPICallbackEventHandler objInterface = null;
						ClientAPICallBackResponse objResponse = new ClientAPICallBackResponse(objPage, ClientAPICallBackResponse.CallBackTypeCode.Simple);

						try {
							objPage.Response.Clear();
							//clear response stream
							if (strControlID == SCRIPT_CALLBACKPAGEID) {
								objControl = objPage;
							} else {
								objControl = Globals.FindControlRecursive(objPage, strControlID, strClientID);
							}
							if ((objControl != null)) {
								if (eType == ClientAPICallBackResponse.CallBackTypeCode.CallbackMethod) {
									try {
										objResponse.Response = ExecuteControlMethod(objControl, strParam);
										objResponse.StatusCode = ClientAPICallBackResponse.CallBackResponseStatusCode.OK;
									} catch (Exception ex) {
										objResponse.StatusCode = ClientAPICallBackResponse.CallBackResponseStatusCode.GenericFailure;
										objResponse.StatusDesc = ex.Message;
									}
								} else {
									//form doesn't implement interface, so use page instead
									if ((objControl) is System.Web.UI.HtmlControls.HtmlForm) {
										objInterface = (IClientAPICallbackEventHandler)objPage;
									} else {
										objInterface = (IClientAPICallbackEventHandler)objControl;
									}

									if ((objInterface != null)) {
										try {
											objResponse.Response = objInterface.RaiseClientAPICallbackEvent(strParam);
											objResponse.StatusCode = ClientAPICallBackResponse.CallBackResponseStatusCode.OK;
										} catch (Exception ex) {
											objResponse.StatusCode = ClientAPICallBackResponse.CallBackResponseStatusCode.GenericFailure;
											objResponse.StatusDesc = ex.Message;
										}
									} else {
										objResponse.StatusCode = ClientAPICallBackResponse.CallBackResponseStatusCode.InterfaceNotSupported;
										objResponse.StatusDesc = "Interface Not Supported";
									}
								}
							} else {
								objResponse.StatusCode = ClientAPICallBackResponse.CallBackResponseStatusCode.ControlNotFound;
								objResponse.StatusDesc = "Control Not Found";
							}
						} catch (Exception ex) {
							objResponse.StatusCode = ClientAPICallBackResponse.CallBackResponseStatusCode.GenericFailure;
							objResponse.StatusDesc = "Generic failure";
							//ex.Message
						} finally {
							objResponse.Write();
							//objPage.Response.Flush()
							objPage.Response.End();
						}

						break;
					case ClientAPICallBackResponse.CallBackTypeCode.ProcessPage:
					case ClientAPICallBackResponse.CallBackTypeCode.ProcessPageCallbackMethod:
						objPage.SetRenderMethodDelegate(CallbackRenderMethod);
						break;
				}
			}
		}

		private static void CallbackRenderMethod(System.Web.UI.HtmlTextWriter output, System.Web.UI.Control container)
		{
			Page objPage = (Page)container;
			ClientAPICallBackResponse.CallBackTypeCode eType = GetCallbackType(objPage);
			if (eType == ClientAPICallBackResponse.CallBackTypeCode.ProcessPage) {
				eType = ClientAPICallBackResponse.CallBackTypeCode.Simple;
			} else {
				eType = ClientAPICallBackResponse.CallBackTypeCode.CallbackMethod;
			}
			HandleClientAPICallbackEvent(objPage, eType);
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Determines if DNNVariable control is present in page's control collection
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	4/6/2005	Commented
		/// </history>
		/// -----------------------------------------------------------------------------
		public static bool NeedsDNNVariable(Control objParent)
		{
			return GetDNNVariableControl(objParent) == null;
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Responsible for registering client side js libraries and its dependecies.
		/// </summary>
		/// <param name="objPage">Current page rendering content</param>
		/// <param name="eRef">Enumerator of library to reference</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/3/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static void RegisterClientReference(Page objPage, ClientNamespaceReferences eRef)
		{
			switch (eRef) {
				case ClientNamespaceReferences.dnn:
					if (!IsClientScriptBlockRegistered(objPage, "dnn.js")) {
						RegisterClientScriptBlock(objPage, "dnn.js", "<script src=\"" + ClientAPI.ScriptPath + "dnn.js\"></script>");
						if (BrowserSupportsFunctionality(ClientFunctionality.SingleCharDelimiters) == false) {
							RegisterClientVariable(objPage, "__scdoff", "1", true);
							//SingleCharDelimiters Off!!!
						}

						//allow scripts to be dynamically loaded when use external false
						if (ClientAPI.UseExternalScripts == false) {
							ClientAPI.RegisterEmbeddedResource(objPage, "dnn.scripts.js", typeof(DotNetNuke.UI.Utilities.ClientAPI));
						}
					}
					break;
				case ClientNamespaceReferences.dnn_dom:
					RegisterClientReference(objPage, ClientNamespaceReferences.dnn);
					break;
				case ClientNamespaceReferences.dnn_dom_positioning:
					RegisterClientReference(objPage, ClientNamespaceReferences.dnn);
					ClientResourceManager.RegisterScript(objPage, ScriptPath + "dnn.dom.positioning.js");

					break;
				case ClientNamespaceReferences.dnn_xml:
					RegisterClientReference(objPage, ClientNamespaceReferences.dnn);
					ClientResourceManager.RegisterScript(objPage, ScriptPath + "dnn.xml.js", FileOrder.Js.DnnXml);

					if (BrowserSupportsFunctionality(ClientFunctionality.XMLJS)) {
						ClientResourceManager.RegisterScript(objPage, ScriptPath + "dnn.xml.jsparser.js", FileOrder.Js.DnnXmlJsParser);
					}
					break;
				case ClientNamespaceReferences.dnn_xmlhttp:
					RegisterClientReference(objPage, ClientNamespaceReferences.dnn);
					ClientResourceManager.RegisterScript(objPage, ScriptPath + "dnn.xmlhttp.js", FileOrder.Js.DnnXmlHttp);

					if (BrowserSupportsFunctionality(ClientFunctionality.XMLHTTPJS)) {
						ClientResourceManager.RegisterScript(objPage, ScriptPath + "dnn.xmlhttp.jsxmlhttprequest.js", FileOrder.Js.DnnXmlHttpJsXmlHttpRequest);
					}
					break;
				case ClientNamespaceReferences.dnn_motion:
					RegisterClientReference(objPage, ClientNamespaceReferences.dnn_dom_positioning);
					ClientResourceManager.RegisterScript(objPage, ScriptPath + "dnn.motion.js");

					break;
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Registers a client side variable (name/value) pair
		/// </summary>
		/// <param name="objPage">Current page rendering content</param>
		/// <param name="strVar">Variable name</param>
		/// <param name="strValue">Value</param>
		/// <param name="blnOverwrite">Determins if a replace or append is applied when variable already exists</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	8/3/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static void RegisterClientVariable(Page objPage, string strVar, string strValue, bool blnOverwrite)
		{
			//only add once
			Dictionary<string, string> objDict = GetClientVariableList(objPage);
			if (objDict.ContainsKey(strVar)) {
				if (blnOverwrite) {
					objDict[strVar] = strValue;
				} else {
					//appending value
					objDict[strVar] += strValue;
				}
			} else {
				objDict.Add(strVar, strValue);
			}

			//TODO: JON to serialize this each time is quite a perf hit.  better to somehow sync the prerender event and do it once
			//SerializeClientVariableDictionary(objPage, objDict)

			//instead of serializing each time, do it once
			if (HttpContext.Current.Items["CAPIPreRender"] == null) {
				//AddHandler objPage.PreRender, AddressOf CAPIPreRender
				GetDNNVariableControl(objPage).PreRender += CAPIPreRender;
				HttpContext.Current.Items["CAPIPreRender"] = true;
			}

			//if we are past prerender event then we need to keep serializing
			if ((HttpContext.Current.Items["CAPIPostPreRender"] != null)) {
				SerializeClientVariableDictionary(objPage, objDict);
			}

		}

		private static void CAPIPreRender(object Sender, System.EventArgs Args)
		{
			Control ctl = (Control)Sender;
			System.Collections.Generic.Dictionary<string, string> objDict = GetClientVariableList(ctl.Page);
			SerializeClientVariableDictionary(ctl.Page, objDict);
			HttpContext.Current.Items["CAPIPostPreRender"] = true;
		}



		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Responsible for inputting the hidden field necessary for the ClientAPI to pass variables back in forth
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	4/6/2005	Commented
		/// </history>
		/// -----------------------------------------------------------------------------
		public static HtmlInputHidden RegisterDNNVariableControl(System.Web.UI.Control objParent)
		{
			System.Web.UI.HtmlControls.HtmlInputHidden ctlVar = GetDNNVariableControl(objParent);

			if (ctlVar == null) {
				Control oForm = FindForm(objParent);
				if ((oForm != null)) {
					//objParent.Page.ClientScript.RegisterHiddenField(DNNVARIABLE_CONTROLID, "")
					ctlVar = new NonNamingHiddenInput();
					//New System.Web.UI.HtmlControls.HtmlInputHidden
					ctlVar.ID = DNNVARIABLE_CONTROLID;
					//oForm.Controls.AddAt(0, ctlVar)
					oForm.Controls.Add(ctlVar);
				}
			}
			return ctlVar;
		}


		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Traps client side keydown event looking for passed in key press (ASCII) and hooks it up with server side postback handler
		/// </summary>
		/// <param name="objControl">Control that should trap the keydown</param>
		/// <param name="objPostbackControl">Server-side control that has its onclick event handled server-side</param>
		/// <param name="intKeyAscii">ASCII value of key to trap</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	2/17/2005	Commented
		/// </history>
		/// -----------------------------------------------------------------------------
		public static void RegisterKeyCapture(Control objControl, Control objPostbackControl, int intKeyAscii)
		{
			Globals.SetAttribute(objControl, "onkeydown", GetKeyDownHandler(intKeyAscii, GetPostBackClientHyperlink(objPostbackControl, "")));
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Traps client side keydown event looking for passed in key press (ASCII) and hooks it up with client-side javascript
		/// </summary>
		/// <param name="objControl">Control that should trap the keydown</param>
		/// <param name="strJavascript">Javascript to execute when event fires</param>
		/// <param name="intKeyAscii">ASCII value of key to trap</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	2/17/2005	Commented
		/// </history>
		/// -----------------------------------------------------------------------------
		public static void RegisterKeyCapture(Control objControl, string strJavascript, int intKeyAscii)
		{
			Globals.SetAttribute(objControl, "onkeydown", GetKeyDownHandler(intKeyAscii, strJavascript));
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Allows a listener to be associated to a client side post back
		/// </summary>
		/// <param name="objParent">The current control on the page or the page itself.  Depending on where the page is in its lifecycle it may not be possible to add a control directly to the page object, therefore we will use the current control being rendered to append the postback control.</param>
		/// <param name="strEventName">Name of the event to sync.  If a page contains more than a single client side event only the events associated with the passed in name will be raised.</param>
		/// <param name="objDelegate">Server side AddressOf the function to handle the event</param>
		/// <param name="blnMultipleHandlers">Boolean flag to determine if multiple event handlers can be associated to an event.</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	9/15/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static void RegisterPostBackEventHandler(Control objParent, string strEventName, ClientAPIPostBackControl.PostBackEvent objDelegate, bool blnMultipleHandlers)
		{
			const string CLIENTAPI_POSTBACKCTL_ID = "ClientAPIPostBackCtl";
			Control objCtl = Globals.FindControlRecursive(objParent.Page, CLIENTAPI_POSTBACKCTL_ID);
			//DotNetNuke.Globals.FindControlRecursive(objParent, CLIENTAPI_POSTBACKCTL_ID)
			if (objCtl == null) {
				objCtl = new ClientAPIPostBackControl(objParent.Page, strEventName, objDelegate);
				objCtl.ID = CLIENTAPI_POSTBACKCTL_ID;
				objParent.Controls.Add(objCtl);
				ClientAPI.RegisterClientVariable(objParent.Page, "__dnn_postBack", GetPostBackClientHyperlink(objCtl, "[DATA]"), true);
			} else if (blnMultipleHandlers) {
				((ClientAPIPostBackControl)objCtl).AddEventHandler(strEventName, objDelegate);
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Registers a button inside a table for the ability to perform client-side reordering
		/// </summary>
		/// <param name="objButton">Button responsible for moving the row up or down.</param>
		/// <param name="objPage">Page the table belongs to.  Can't just use objButton.Page because inside ItemCreated event of grid the button has no page yet.</param>
		/// <param name="blnUp">Determines if the button is responsible for moving the row up or down</param>
		/// <param name="strKey">Unique key for the table/grid to be used to obtain the new order on postback.  Needed when calling GetClientSideReOrder</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	3/10/2006	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static void EnableClientSideReorder(Control objButton, Page objPage, bool blnUp, string strKey)
		{

			if (BrowserSupportsFunctionality(ClientFunctionality.DHTML)) {
				RegisterClientReference(objPage, ClientNamespaceReferences.dnn_dom);
				ClientResourceManager.RegisterScript(objPage, ScriptPath + "dnn.util.tablereorder.js");

				AddAttribute(objButton, "onclick", "if (dnn.util.tableReorderMove(this," + Convert.ToInt32(blnUp) + ",'" + strKey + "')) return false;");
				Control objParent = objButton.Parent;
				while ((objParent != null)) {
					if (objParent is TableRow) {
						AddAttribute(objParent, "origidx", "-1");
						//mark row as one that we care about, it will be numbered correctly on client
					}
					objParent = objParent.Parent;
				}
			}

		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Retrieves an array of the new order for the rows
		/// </summary>
		/// <param name="strKey">Unique key for the table/grid to be used to obtain the new order on postback.  Needed when calling GetClientSideReOrder</param>
		/// <param name="objPage">Page the table belongs to.  Can't just use objButton.Page because inside ItemCreated event of grid the button has no page yet.</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	3/10/2006	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static string[] GetClientSideReorder(string strKey, Page objPage)
		{
			if (!string.IsNullOrEmpty(ClientAPI.GetClientVariable(objPage, strKey))) {
				return ClientAPI.GetClientVariable(objPage, strKey).Split(',');
			} else {
				return new string[0];
			}
		}

		public static bool ClientAPIDisabled()
		{
			//jon
			return false;
			if (m_ClientAPIDisabled == string.Empty) {
				if (System.Configuration.ConfigurationManager.AppSettings["ClientAPI"] == null) {
					m_ClientAPIDisabled = "1";
				} else {
					m_ClientAPIDisabled = System.Configuration.ConfigurationManager.AppSettings["ClientAPI"];
				}
			}
			return m_ClientAPIDisabled == "0";
		}

		public static string GetPostBackClientEvent(Page objPage, Control objControl, string arg)
		{
			return objPage.ClientScript.GetPostBackEventReference(objControl, arg);
		}
		public static string GetPostBackClientHyperlink(Control objControl, string strArgument)
		{
			return "javascript:" + GetPostBackEventReference(objControl, strArgument);
		}
		public static string GetPostBackEventReference(Control objControl, string strArgument)
		{
			return objControl.Page.ClientScript.GetPostBackEventReference(objControl, strArgument);
		}
		public static bool IsClientScriptBlockRegistered(Page objPage, string key)
		{
			return objPage.ClientScript.IsClientScriptBlockRegistered(objPage.GetType(), key);
		}
		public static void RegisterClientScriptBlock(Page objPage, string key, string strScript)
		{
			if (Globals.IsEmbeddedScript(key) == false) {
				//JON
				//ScriptManager.RegisterClientScriptBlock(objPage, objPage.GetType(), key, strScript, False)
				objPage.ClientScript.RegisterClientScriptBlock(objPage.GetType(), key, strScript);
			} else {
				RegisterClientScriptBlock(objPage, key);
			}
		}
		public static void RegisterStartUpScript(Page objPage, string key, string script)
		{
			MSAJAX.RegisterStartupScript(objPage, key, script);
			//objPage.ClientScript.RegisterStartupScript(objPage.GetType(), key, script)
		}

		public static void RegisterClientScriptBlock(Page objPage, string key)
		{
			if (UseExternalScripts) {
				MSAJAX.RegisterClientScript(objPage, ClientAPI.ScriptPath + key);
			} else {
				MSAJAX.RegisterClientScript(objPage, key, "DotNetNuke.WebUtility");
				//client-side won't be able to get this info from dnn.js location, since its embedded.
				// so we need to use where it would have gone instead
				RegisterClientVariable(objPage, "__sp", ScriptPath, true);
			}
		}

		public static bool RegisterControlMethods(Control CallbackControl)
		{
			return RegisterControlMethods(CallbackControl, string.Empty);
		}

		public static bool RegisterControlMethods(Control CallbackControl, string FriendlyID)
		{
			ControlMethodClassAttribute classAttr = null;
			string name = null;
			bool ret = true;

			classAttr = (ControlMethodClassAttribute)Attribute.GetCustomAttribute(CallbackControl.GetType(), typeof(ControlMethodClassAttribute));

			if ((classAttr != null)) {
				if (string.IsNullOrEmpty(classAttr.FriendlyNamespace)) {
					name = CallbackControl.GetType().FullName;
				} else {
					name = classAttr.FriendlyNamespace;
				}
				string format = "{0}.{1}={2} ";
				if (string.IsNullOrEmpty(FriendlyID)) {
					format = "{0}={2} ";
				}
				ClientAPI.RegisterClientVariable(CallbackControl.Page, "__dnncbm", string.Format(format, name, FriendlyID, CallbackControl.UniqueID), false);

				if (BrowserSupportsFunctionality(ClientFunctionality.XMLHTTP) && BrowserSupportsFunctionality(ClientFunctionality.XML)) {
					DotNetNuke.UI.Utilities.ClientAPI.RegisterClientReference(CallbackControl.Page, DotNetNuke.UI.Utilities.ClientAPI.ClientNamespaceReferences.dnn_xml);
					DotNetNuke.UI.Utilities.ClientAPI.RegisterClientReference(CallbackControl.Page, DotNetNuke.UI.Utilities.ClientAPI.ClientNamespaceReferences.dnn_xmlhttp);
				} else {
					ret = false;
				}
			} else {
				throw new Exception("Control does not have CallbackMethodAttribute");
			}

			return ret;
		}

		public static string ExecuteControlMethod(Control CallbackControl, string callbackArgument)
		{
			object result = null;
			Type controlType = CallbackControl.GetType();
			//Dim serializer As JavaScriptSerializer = New JavaScriptSerializer()
			//Dim callInfo As Dictionary(Of String, Object) = TryCast(serializer.DeserializeObject(callbackArgument), Dictionary(Of String, Object))
			Dictionary<string, object> callInfo = MSAJAX.DeserializeObject(callbackArgument) as Dictionary<string, object>;
			string methodName = Convert.ToString(callInfo["method"]);
			Dictionary<string, object> args = callInfo["args"] as Dictionary<string, object>;
			MethodInfo mi = controlType.GetMethod(methodName, (BindingFlags.Public | (BindingFlags.Static | BindingFlags.Instance)));

			if ((mi == null)) {
				throw new Exception(string.Format("Class: {0} does not have the method: {1}", controlType.FullName, methodName));
			}
			ParameterInfo[] methodParams = mi.GetParameters();

			//only allow methods with attribute to be called 
			ControlMethodAttribute methAttr = (ControlMethodAttribute)Attribute.GetCustomAttribute(mi, typeof(ControlMethodAttribute));
			if (methAttr == null || args.Count != methodParams.Length) {
				throw new Exception(string.Format("Class: {0} does not have the method: {1}", controlType.FullName, methodName));
			}

			object[] targetArgs = new object[args.Count];
			//Dim ser As System.Web.Script.Serialization.JavaScriptSerializer = New System.Web.Script.Serialization.JavaScriptSerializer(New SimpleTypeResolver())
			string paramName = null;
			object arg = null;
			Type paramType = null;
			for (int i = 0; i <= methodParams.Length - 1; i++) {
				paramName = methodParams[i].Name;
				paramType = methodParams[i].ParameterType;
				if ((args.ContainsKey(paramName))) {
					arg = args[paramName];
					if (paramType.IsGenericType) {
						if ((arg as IList != null)) {
							Type[] genTypes = paramType.GetGenericArguments();
							targetArgs[i] = GetListFromType(genTypes[0], (IList)arg);
						} else {
							targetArgs[i] = arg;
						}
					} else if (paramType.IsClass && (arg as Dictionary<string, object> != null)) {
						targetArgs[i] = ConvertDictionaryToObject((Dictionary<string, object>)arg, paramType);
					} else {
						targetArgs[i] = Convert.ChangeType(arg, methodParams[i].ParameterType, CultureInfo.InvariantCulture);
					}
				}
				//Dim T As Type = methodParams(i).ParameterType
				//targetArgs(i) = ser.ConvertToType(Of String)(args(i))
				//targetArgs(i) = Convert.ChangeType(args(i), methodParams(i).ParameterType, CultureInfo.InvariantCulture)

				//End If
			}
			result = mi.Invoke(CallbackControl, targetArgs);

			Dictionary<string, object> resultInfo = new Dictionary<string, object>();
			resultInfo["result"] = result;
			//Return serializer.Serialize(resultInfo)
			return MSAJAX.Serialize(resultInfo);
		}

		public static IList GetListFromType(Type TheType, IList TheList)
		{
			Type listOf = typeof(List<>);
			Type argType = listOf.MakeGenericType(TheType);

			IList instance = (IList)Activator.CreateInstance(argType);
			foreach (Dictionary<string, object> listitem in TheList) {
				if ((listitem != null)) {
					instance.Add(ConvertDictionaryToObject(listitem, TheType));
				}
			}
			return instance;

		}

		public static object ConvertDictionaryToObject(Dictionary<string, object> dict, Type TheType)
		{
			object item = Activator.CreateInstance(TheType);
			PropertyInfo pi = null;
			foreach (KeyValuePair<string, object> pair in dict) {
				pi = TheType.GetProperty(pair.Key);
				if ((pi != null) && pi.CanWrite && (pair.Value != null)) {
					TheType.InvokeMember(pair.Key, System.Reflection.BindingFlags.SetProperty, null, item, new object[] { pair.Value });
				}
			}
			return item;
		}

		//Private Shared Function ConvertIt(Of T)(ByVal List As IList, ByVal TheType As Type) As List(Of T)
		//    Dim o As Object
		//    Dim fields() As FieldInfo = TheType.GetFields()

		//    For Each item As Object In List
		//        o = Activator.CreateInstance(TheType)
		//        For Each info As FieldInfo In fields

		//        Next
		//    Next
		//End Function



		public static void RegisterEmbeddedResource(Page ThePage, string FileName, Type AssemblyType)
		{
			RegisterClientVariable(ThePage, FileName + ".resx", ThePage.ClientScript.GetWebResourceUrl(AssemblyType, FileName), true);
		}


		#endregion

	}

	namespace Animation
	{
		public enum AnimationType
		{
			None,
			Slide,
			Expand,
			Diagonal,
			ReverseDiagonal
		}

		public enum EasingDirection
		{
			In,
			Out,
			InOut
		}

		public enum EasingType
		{
			Bounce,
			Circ,
			Cubic,
			Expo,
			Quad,
			Quint,
			Quart,
			Sine
		}
	}

}
