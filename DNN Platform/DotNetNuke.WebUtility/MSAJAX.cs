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
using System.Reflection;
using System.Web;
using System.Web.UI;


namespace DotNetNuke.UI.Utilities
{

	public class MSAJAX
	{

		#region "Member Variables"
		private static Type m_ScriptManagerType;
		private static Type m_ScriptReferenceType;
		private static Type m_ScriptReferenceCollectionType;
		private static Type m_JavaScriptSerializerType;
		private static Type m_AlternateJavaScriptSerializerType;
			#endregion
		private static int m_Installed = -1;

		#region "Private Properties"

		private static Type ScriptManagerType {
			get {
				if (m_ScriptManagerType == null) {
					m_ScriptManagerType = Reflection.CreateType("System.Web.UI.ScriptManager", false);
				}
				return m_ScriptManagerType;
			}
		}

		private static Type ScriptReferenceCollectionType {
			get {
				if (m_ScriptReferenceCollectionType == null) {
					m_ScriptReferenceCollectionType = Reflection.CreateType("System.Web.UI.ScriptReferenceCollection", true);
				}
				return m_ScriptReferenceCollectionType;
			}
		}

		private static Type ScriptReferenceType {
			get {
				if (m_ScriptReferenceType == null) {
					m_ScriptReferenceType = Reflection.CreateType("System.Web.UI.ScriptReference", true);
				}
				return m_ScriptReferenceType;
			}
		}

		private static Type JavaScriptSerializerType {
			get {
				if (m_JavaScriptSerializerType == null) {
					m_JavaScriptSerializerType = Reflection.CreateType("System.Web.Script.Serialization.JavaScriptSerializer", true);
				}
				return m_JavaScriptSerializerType;
			}
		}

		private static Type AlternateJavaScriptSerializerType {
			get {
				if (m_AlternateJavaScriptSerializerType == null) {
					m_AlternateJavaScriptSerializerType = Reflection.CreateType("Newtonsoft.Json.JavaScriptConvert", true);
				}
				return m_AlternateJavaScriptSerializerType;
			}
		}

		public static bool IsInstalled {
			get {
				if (m_Installed == -1) {
					//Dim capiPath As String = System.IO.Path.GetDirectoryName(GetType(ClientAPI.ClientFunctionality).Assembly.CodeBase)
					//Dim msajaxPath As String = System.IO.Path.GetDirectoryName(ScriptManagerType.Assembly.CodeBase)
					if (ScriptManagerType.Assembly.GlobalAssemblyCache == false) {
						//If capiPath = msajaxPath Then
						//if msajax loaded from same path as clientapi, then we need to be in full trust

						try {
							//demand a high level permission
							AspNetHostingPermission perm = new AspNetHostingPermission(AspNetHostingPermissionLevel.High);
							perm.Demand();
							m_Installed = 1;
						} catch (Exception ex) {
							m_Installed = 0;
						}
					} else {
						m_Installed = 1;
					}

				}
				return m_Installed == 1;
				//Return Not ScriptManagerType() Is Nothing
			}
		}

		#endregion

		#region "Public Methods"

		private static Control ScriptManagerControl(Page objPage)
		{
			MethodInfo method = ScriptManagerType.GetMethod("GetCurrent");
			return (Control)method.Invoke(ScriptManagerType, new object[] { objPage });
		}

		public static void Register(Page objPage)
		{
			if (IsInstalled) {
				if (ScriptManagerControl(objPage) == null) {
					Control sm = ScriptManagerControl(objPage);
					if (sm == null) {
						sm = (Control)Reflection.CreateInstance(ScriptManagerType);
					}
					if ((sm != null)) {
						sm.ID = "ScriptManager";
						//objPage.Form.Controls.AddAt(0, sm)
						objPage.Form.Controls.Add(sm);
						HttpContext.Current.Items["System.Web.UI.ScriptManager"] = true;
						//Let DNN know we added it
					}
				}
			} else {
				//TEST!!!
				if (ClientAPI.UseExternalScripts) {
					MSAJAX.RegisterClientScript(objPage, ClientAPI.ScriptPath + "MicrosoftAjax.js");
					MSAJAX.RegisterClientScript(objPage, ClientAPI.ScriptPath + "MicrosoftAjaxWebForms.js");
				} else {
					MSAJAX.RegisterClientScript(objPage, "MicrosoftAjax.js", "DotNetNuke.WebUtility");
					MSAJAX.RegisterClientScript(objPage, "MicrosoftAjaxWebForms.js", "DotNetNuke.WebUtility");
				}
				objPage.ClientScript.RegisterStartupScript(objPage.GetType(), "MSAJAXInit", "Sys.Application.initialize();", true);
			}
		}

		public static void SetScriptManagerProperty(Page objPage, string PropertyName, object[] Args)
		{
			if (IsInstalled) {
				Register(objPage);
				if ((ScriptManagerControl(objPage) != null)) {
					Reflection.SetProperty(ScriptManagerType, PropertyName, ScriptManagerControl(objPage), Args);
				}
			}
		}

		public static void RegisterClientScript(Page objPage, string Path)
		{
			if (IsInstalled) {
				Register(objPage);
				object @ref = GetScriptReference();
				MSAJAX.SetScriptReferenceProperty(@ref, "Path", Path);
				MSAJAX.SetScriptReferenceProperty(@ref, "IgnoreScriptPath", false);
				RegisterClientScript(objPage, @ref);
			} else {
				if (objPage.ClientScript.IsClientScriptBlockRegistered(Path) == false) {
					objPage.ClientScript.RegisterClientScriptInclude(Path, Path);
				}
			}
		}

		public static void RegisterStartupScript(Page objPage, string Key, string Script)
		{
			if (IsInstalled) {
				MethodInfo[] methods = ScriptManagerType.GetMethods();
				foreach (MethodInfo method in methods) {
					if ((method.Name == "RegisterStartupScript")) {
						method.Invoke(null, new object[] {
							objPage,
							objPage.GetType(),
							Key,
							Script,
							false
						});
						break; // TODO: might not be correct. Was : Exit For
					}
				}
			} else {
				objPage.ClientScript.RegisterStartupScript(objPage.GetType(), Key, Script);
			}
		}

		public static void RegisterClientScript(Page objPage, string Name, string Assembly)
		{
			if (IsInstalled) {
				Register(objPage);
				object @ref = GetScriptReference();
				MSAJAX.SetScriptReferenceProperty(@ref, "Name", Name);
				MSAJAX.SetScriptReferenceProperty(@ref, "Assembly", Assembly);
				RegisterClientScript(objPage, @ref);
			} else {
				if (objPage.ClientScript.IsClientScriptBlockRegistered(Name) == false) {
					objPage.ClientScript.RegisterClientScriptResource(Reflection.CreateType(Assembly, true), Name);
				}
			}
		}

		private static void RegisterClientScript(Page objPage, object Ref)
		{
			if (IsInstalled) {
				Register(objPage);
				object sm = ScriptManagerControl(objPage);
				object scripts = ScriptManagerScripts(sm);
				if (Convert.ToBoolean(Reflection.InvokeMethod(ScriptReferenceCollectionType, "Contains", scripts, new object[] { Ref })) == false) {
					Reflection.InvokeMethod(ScriptReferenceCollectionType, "Add", ScriptManagerScripts(sm), new object[] { Ref });
				}
			}
		}

		private static object GetScriptReference()
		{
			return Reflection.CreateInstance(ScriptReferenceType);
		}

		private static void SetScriptReferenceProperty(object Ref, string Name, object Value)
		{
			Reflection.SetProperty(ScriptReferenceType, Name, Ref, new object[] { Value });
		}

		private static object ScriptManagerScripts(object objScriptManager)
		{
			return Reflection.GetProperty(ScriptManagerType, "Scripts", objScriptManager);
		}

		public static T Deserialize<T>(string Data)
		{
			if (IsInstalled) {
				object ser = Reflection.CreateInstance(JavaScriptSerializerType);
				return Reflection.InvokeGenericMethod<T>(JavaScriptSerializerType, "Deserialize", ser, new object[] { Data });
			} else if (AlternateJavaScriptSerializerType != null) {
				try {
					return (T)Reflection.InvokeMethod(AlternateJavaScriptSerializerType, "DeserializeObject", null, new object[] {
						Data,
						typeof(T)
					});
				} catch (Exception ex) {
					//if initial attempt did not work, try one more time, without specifying a type
					return (T)Reflection.InvokeMethod(AlternateJavaScriptSerializerType, "DeserializeObject", null, new object[] { Data });
				}
			}

			return default(T);
		}

		public static object DeserializeObject(string Data)
		{
			if (IsInstalled) {
				object ser = Reflection.CreateInstance(JavaScriptSerializerType);
				return Reflection.InvokeMethod(JavaScriptSerializerType, "DeserializeObject", ser, new object[] { Data });
			} else if (AlternateJavaScriptSerializerType != null) {
				return Reflection.InvokeMethod(AlternateJavaScriptSerializerType, "DeserializeObject", null, new object[] { Data });
			}
			return null;
		}

		public static string Serialize(object Obj)
		{
			object ser = null;
			if (IsInstalled) {
				ser = Reflection.CreateInstance(JavaScriptSerializerType);
				return Convert.ToString(Reflection.InvokeMethod(JavaScriptSerializerType, "Serialize", ser, new object[] { Obj }));
			} else if (AlternateJavaScriptSerializerType != null) {
				string json = Convert.ToString(Reflection.InvokeMethod(AlternateJavaScriptSerializerType, "SerializeObject", null, new object[] { Obj }));
				//HACK
				return json.Replace("'", "\\u0027");
				//make same as MSAJAX - escape single quote
			}
			return "";
		}

		#endregion

	}
}
