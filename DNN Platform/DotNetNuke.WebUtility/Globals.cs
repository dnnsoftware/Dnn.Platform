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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace DotNetNuke.UI.Utilities
{
	public class Globals
	{

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Searches control hierarchy from top down to find a control matching the passed in name
		/// </summary>
		/// <param name="objParent">Root control to begin searching</param>
		/// <param name="strControlName">Name of control to look for</param>
		/// <returns></returns>
		/// <remarks>
		/// This differs from FindControlRecursive in that it looks down the control hierarchy, whereas, the 
		/// FindControlRecursive starts at the passed in control and walks the tree up.  Therefore, this function is 
		/// more a expensive task.
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	9/17/2004	Created
		///     [Jon Henning]   12/3/2004   Now checking if the control HasControls before calling FindControl.
		///                                 Using FindControl or accessing the controls collection on controls like
		///                                 the DataList can cause problems with ViewState
		/// </history>
		/// -----------------------------------------------------------------------------
		public static Control FindControlRecursive(Control objParent, string strControlName)
		{
			return FindControlRecursive(objParent, strControlName, "");
		}

		public static Control FindControlRecursive(Control objParent, string strControlName, string strClientID)
		{
			Control objCtl = null;
			Control objChild = null;
			objCtl = objParent.FindControl(strControlName);
			if (objCtl == null) {
				foreach (Control objChild_loopVariable in objParent.Controls) {
					objChild = objChild_loopVariable;
					if (objChild.HasControls())
						objCtl = FindControlRecursive(objChild, strControlName, strClientID);
					if ((objCtl != null) && !string.IsNullOrEmpty(strClientID) && objCtl.ClientID != strClientID)
						objCtl = null;
					if ((objCtl != null))
						break; // TODO: might not be correct. Was : Exit For
				}
			}
			return objCtl;
		}

		public static string GetAttribute(Control objControl, string strAttr)
		{
			if (objControl is WebControl)
			{
				return ((WebControl) objControl).Attributes[strAttr];
			}
			else if (objControl is HtmlControl)
			{
				return ((HtmlControl) objControl).Attributes[strAttr];
			}
			else
			{	//throw error?
					return null;
			}
		}

		public static void SetAttribute(Control objControl, string strAttr, string strValue)
		{
			string strOrigVal = GetAttribute(objControl, strAttr);
			if (!string.IsNullOrEmpty(strOrigVal))
				strValue = strOrigVal + strValue;

			if (objControl is WebControl)
			{
				WebControl objCtl = (WebControl)objControl;
				if (objCtl.Attributes[strAttr] == null)
				{
					objCtl.Attributes.Add(strAttr, strValue);
				}
				else
				{
					objCtl.Attributes[strAttr] = strValue;
				}
			}
			else if (objControl is HtmlControl)
			{
				HtmlControl objCtl = (HtmlControl)objControl;
				if (objCtl.Attributes[strAttr] == null)
				{
					objCtl.Attributes.Add(strAttr, strValue);
				}
				else
				{
					objCtl.Attributes[strAttr] = strValue;
				}
			}
			else
			{	//throw error?
			}
		}

		//hack... can we use a method to determine this
		private static Hashtable m_aryScripts;
		public static bool IsEmbeddedScript(string key)
		{
			if (m_aryScripts == null) {
				m_aryScripts = new Hashtable();
				m_aryScripts.Add("dnn.js", "");
				m_aryScripts.Add("dnn.dom.positioning.js", "");
				m_aryScripts.Add("dnn.diagnostics.js", "");
				m_aryScripts.Add("dnn.scripts.js", "");
				m_aryScripts.Add("dnn.util.tablereorder.js", "");
				m_aryScripts.Add("dnn.xml.js", "");
				m_aryScripts.Add("dnn.xml.jsparser.js", "");
				m_aryScripts.Add("dnn.xmlhttp.js", "");
				m_aryScripts.Add("dnn.xmlhttp.jsxmlhttprequest.js", "");
				m_aryScripts.Add("dnn.motion.js", "");
			}
			return m_aryScripts.ContainsKey(key);
		}

	}
}
