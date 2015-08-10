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

namespace DotNetNuke.UI.Utilities
{

	/// -----------------------------------------------------------------------------
	/// Project	 : DotNetNuke
	/// Class	 : ClientAPIPostBackEventArgs
	/// 
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// Event arguments passed to a delegate associated to a client postback event 
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// 	[Jon Henning]	9/15/2004	Created
	/// </history>
	/// -----------------------------------------------------------------------------
	public class ClientAPIPostBackEventArgs
	{
		public string EventName;

		public Hashtable EventArguments = new Hashtable();

		public ClientAPIPostBackEventArgs()
		{
		}

		public ClientAPIPostBackEventArgs(string strEventArgument)
		{
			string[] aryArgs = Regex.Split(strEventArgument, ClientAPI.COLUMN_DELIMITER);
			int i = 0;

			if ((aryArgs.Length > 0))
				this.EventName = aryArgs[0];
			for (i = 1; i <= aryArgs.Length - 1; i += 2) {
				this.EventArguments.Add(aryArgs[i], aryArgs[i + 1]);
			}

		}
	}

}
