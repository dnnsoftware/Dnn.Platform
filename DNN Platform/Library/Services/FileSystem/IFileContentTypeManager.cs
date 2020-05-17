// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Services.FileSystem
{
	public interface IFileContentTypeManager
	{
		/// <summary>
		/// Gets the Content Type for the specified file extension.
		/// </summary>
		/// <param name="extension">The file extension.</param>
		/// <returns>The Content Type for the specified extension.</returns>
		string GetContentType(string extension);

		/// <summary>
		/// Get all content types dictionary.
		/// </summary>
		IDictionary<string, string> ContentTypes { get; }
	}
}
