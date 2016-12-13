#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Common.Lists;
using DotNetNuke.ComponentModel;

namespace DotNetNuke.Services.FileSystem
{
	public class FileContentTypeManager : ComponentBase<IFileContentTypeManager, FileContentTypeManager>, IFileContentTypeManager
	{
		#region Fields

		private IDictionary<string, string> _contentTypes;
		private static readonly object _threadLocker = new object();

		#endregion

		#region Implement IFileContentTypeManager

		public virtual IDictionary<string, string> ContentTypes
		{
			get
			{
				if (_contentTypes == null)
				{
					lock (_threadLocker)
					{
						if (_contentTypes == null)
						{
							var listController = new ListController();
							var listEntries = listController.GetListEntryInfoItems("ContentTypes");
							if (listEntries == null || !listEntries.Any())
							{
								_contentTypes = GetDefaultContentTypes();
							}
							else
							{
								_contentTypes = new Dictionary<string, string>();
								if (listEntries != null)
								{
									foreach (var contentTypeEntry in listEntries)
									{
										_contentTypes.Add(contentTypeEntry.Value, contentTypeEntry.Text);
									}
								}
							}
						}
					}
				}

				return _contentTypes;
			}
		}

		public virtual string GetContentType(string extension)
		{

			if (string.IsNullOrEmpty(extension)) return "application/octet-stream";

			var key = extension.TrimStart('.').ToLowerInvariant();
			return ContentTypes.ContainsKey(key) ? ContentTypes[key] : "application/octet-stream";
		}

		#endregion

		#region Private Methods

		private Dictionary<string, string> GetDefaultContentTypes()
		{
			var contentTypes = new Dictionary<string, string>();
			contentTypes.Add("txt", "text/plain");
			contentTypes.Add("htm", "text/html");
			contentTypes.Add("html", "text/html");
			contentTypes.Add("rtf", "text/richtext");
			contentTypes.Add("jpg", "image/jpeg");
			contentTypes.Add("jpeg", "image/jpeg");
			contentTypes.Add("gif", "image/gif");
			contentTypes.Add("bmp", "image/bmp");
			contentTypes.Add("png", "image/png");
			contentTypes.Add("ico", "image/x-icon");
			contentTypes.Add("svg", "image/svg+xml");
			contentTypes.Add("ttf", "font/ttf");
			contentTypes.Add("eot", "application/vnd.ms-fontobject");
			contentTypes.Add("woff", "application/font-woff");
			contentTypes.Add("mp3", "audio/mpeg");
			contentTypes.Add("wma", "audio/x-ms-wma");
			contentTypes.Add("mpg", "video/mpeg");
			contentTypes.Add("mpeg", "video/mpeg");
			contentTypes.Add("avi", "video/avi");
			contentTypes.Add("mp4", "video/mp4");
			contentTypes.Add("wmv", "video/x-ms-wmv");
			contentTypes.Add("pdf", "application/pdf");
			contentTypes.Add("doc", "application/msword");
			contentTypes.Add("dot", "application/msword");
			contentTypes.Add("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
			contentTypes.Add("dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
			contentTypes.Add("csv", "text/csv");
			contentTypes.Add("xls", "application/x-msexcel");
			contentTypes.Add("xlt", "application/x-msexcel");
			contentTypes.Add("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			contentTypes.Add("xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template");
			contentTypes.Add("ppt", "application/vnd.ms-powerpoint");
			contentTypes.Add("pps", "application/vnd.ms-powerpoint");
			contentTypes.Add("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
			contentTypes.Add("ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");

			return contentTypes;
		}

		#endregion

	}
}
