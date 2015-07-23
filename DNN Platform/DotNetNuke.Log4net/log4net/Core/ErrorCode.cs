#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

namespace log4net.Core
{
	/// <summary>
	/// Defined error codes that can be passed to the <see cref="IErrorHandler.Error(string, Exception, ErrorCode)"/> method.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Values passed to the <see cref="IErrorHandler.Error(string, Exception, ErrorCode)"/> method.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public enum ErrorCode : int
	{
		/// <summary>
		/// A general error
		/// </summary>
		GenericFailure = 0,

		/// <summary>
		/// Error while writing output
		/// </summary>
		WriteFailure,

		/// <summary>
		/// Failed to flush file
		/// </summary>
		FlushFailure,

		/// <summary>
		/// Failed to close file
		/// </summary>
		CloseFailure,

		/// <summary>
		/// Unable to open output file
		/// </summary>
		FileOpenFailure,

		/// <summary>
		/// No layout specified
		/// </summary>
		MissingLayout,

		/// <summary>
		/// Failed to parse address
		/// </summary>
		AddressParseFailure
	}
}
