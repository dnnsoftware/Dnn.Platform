#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
            var contentTypes = new Dictionary<string, string>
            {
                { "txt", "text/plain" },
                { "htm", "text/html" },
                { "html", "text/html" },
                { "rtf", "text/richtext" },
                { "jpg", "image/jpeg" },
                { "jpeg", "image/jpeg" },
                { "gif", "image/gif" },
                { "bmp", "image/bmp" },
                { "png", "image/png" },
                { "ico", "image/x-icon" },
                { "svg", "image/svg+xml" },
                { "ttf", "font/ttf" },
                { "eot", "application/vnd.ms-fontobject" },
                { "woff", "application/font-woff" },
                { "mp3", "audio/mpeg" },
                { "wma", "audio/x-ms-wma" },
                { "mpg", "video/mpeg" },
                { "mpeg", "video/mpeg" },
                { "avi", "video/avi" },
                { "mp4", "video/mp4" },
                { "wmv", "video/x-ms-wmv" },
                { "pdf", "application/pdf" },
                { "doc", "application/msword" },
                { "dot", "application/msword" },
                { "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { "dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
                { "csv", "text/csv" },
                { "xls", "application/x-msexcel" },
                { "xlt", "application/x-msexcel" },
                { "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { "xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
                { "ppt", "application/vnd.ms-powerpoint" },
                { "pps", "application/vnd.ms-powerpoint" },
                { "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { "ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" }
            };

            return contentTypes;
		}

		#endregion

	}
}
