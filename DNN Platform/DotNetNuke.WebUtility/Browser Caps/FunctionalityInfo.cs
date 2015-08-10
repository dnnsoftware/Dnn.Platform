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
using System.Xml.Serialization;

namespace DotNetNuke.UI.Utilities
{

	public class FunctionalityInfo
	{

		private string _desc;
		private ClientAPI.ClientFunctionality _functionality;
		private BrowserCollection _excludes;

		private BrowserCollection _supports;
		[XmlAttribute("nm")]
		public ClientAPI.ClientFunctionality Functionality {
			get { return _functionality; }
			set { _functionality = value; }
		}

		[XmlAttribute("desc")]
		public string Desc {
			get { return _desc; }
			set { _desc = value; }
		}

		[XmlElement("excludes")]
		public BrowserCollection Excludes {
			get { return _excludes; }
			set { _excludes = value; }
		}

		[XmlElement("supports")]
		public BrowserCollection Supports {
			get { return _supports; }
			set { _supports = value; }
		}

		public bool HasMatch(string strAgent, string strBrowser, double dblVersion)
		{
			bool _hasMatch = false;

			//Parse through the supported browsers to find a match
			_hasMatch = HasMatch(Supports, strAgent, strBrowser, dblVersion);

			//If has Match check the excluded browsers to find a match
			if (_hasMatch) {
				_hasMatch = !HasMatch(Excludes, strAgent, strBrowser, dblVersion);
			}

			return _hasMatch;
		}

		private bool HasMatch(BrowserCollection browsers, string strAgent, string strBrowser, double dblVersion)
		{
			bool _hasMatch = false;

			//Parse through the browsers to find a match based on name/minversion
			foreach (Browser browser in browsers) {
				//Check by browser name and min version
				if ((!string.IsNullOrEmpty(browser.Name) && browser.Name.ToLower().Equals(strBrowser.ToLower()) && browser.MinVersion <= dblVersion)) {
					_hasMatch = true;
					break; // TODO: might not be correct. Was : Exit For
				}

				//Check for special browser name of "*"
				if ((browser.Name == "*")) {
					_hasMatch = true;
					break; // TODO: might not be correct. Was : Exit For
				}
			}

			if (!_hasMatch) {
				//Parse through the browsers to find a match based on contains (more expensive so only try if NoMatch
				foreach (Browser browser in browsers) {
					//Check if UserAgent contains the string (Contains)
					if (!string.IsNullOrEmpty(browser.Contains)) {
						if (strAgent.ToLower().IndexOf(browser.Contains.ToLower()) > -1) {
							_hasMatch = true;
							break; // TODO: might not be correct. Was : Exit For
						}
					}
				}
			}

			return _hasMatch;
		}

	}

}

