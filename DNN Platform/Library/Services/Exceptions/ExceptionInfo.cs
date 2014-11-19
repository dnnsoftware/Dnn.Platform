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
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Services.Exceptions
{
	[Serializable]
	public class ExceptionInfo
	{
		public string Method { get; set; }

		public int FileColumnNumber { get; set; }

		public string FileName { get; set; }

		public int FileLineNumber { get; set; }

		public string AssemblyVersion { get; set; }

		public int PortalId { get; set; }

		public int UserId { get; set; }

		public int TabId { get; set; }

		public string RawUrl { get; set; }

		public string Referrer { get; set; }

		public string UserAgent { get; set; }

		public string ExceptionHash { get; set; }

		public string Message { get; set; }

		public string StackTrace { get; set; }

		public string InnerMessage { get; set; }

		public string InnerStackTrace { get; set; }

		public string Source { get; set; }

		public ExceptionInfo() { }

		public ExceptionInfo(Exception e)
		{
			Message = e.Message;
			StackTrace = e.StackTrace;
			Source = e.Source;
			if (e.InnerException != null)
			{
				InnerMessage = e.InnerException.Message;
				InnerStackTrace = e.InnerException.StackTrace;
			}
			ExceptionHash = e.Hash();
		}

	}
}