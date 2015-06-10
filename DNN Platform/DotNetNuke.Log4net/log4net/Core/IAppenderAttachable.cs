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

using log4net.Appender;

namespace log4net.Core
{
	/// <summary>
	/// Interface for attaching appenders to objects.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Interface for attaching, removing and retrieving appenders.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IAppenderAttachable
	{
		/// <summary>
		/// Attaches an appender.
		/// </summary>
		/// <param name="appender">The appender to add.</param>
		/// <remarks>
		/// <para>
		/// Add the specified appender. The implementation may
		/// choose to allow or deny duplicate appenders.
		/// </para>
		/// </remarks>
		void AddAppender(IAppender appender);

		/// <summary>
		/// Gets all attached appenders.
		/// </summary>
		/// <value>
		/// A collection of attached appenders.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets a collection of attached appenders.
		/// If there are no attached appenders the
		/// implementation should return an empty 
		/// collection rather than <c>null</c>.
		/// </para>
		/// </remarks>
		AppenderCollection Appenders {get;}

		/// <summary>
		/// Gets an attached appender with the specified name.
		/// </summary>
		/// <param name="name">The name of the appender to get.</param>
		/// <returns>
		/// The appender with the name specified, or <c>null</c> if no appender with the
		/// specified name is found.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Returns an attached appender with the <paramref name="name"/> specified.
		/// If no appender with the specified name is found <c>null</c> will be
		/// returned.
		/// </para>
		/// </remarks>
		IAppender GetAppender(string name);

		/// <summary>
		/// Removes all attached appenders.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Removes and closes all attached appenders
		/// </para>
		/// </remarks>
		void RemoveAllAppenders();

		/// <summary>
		/// Removes the specified appender from the list of attached appenders.
		/// </summary>
		/// <param name="appender">The appender to remove.</param>
		/// <returns>The appender removed from the list</returns>
		/// <remarks>
		/// <para>
		/// The appender removed is not closed.
		/// If you are discarding the appender you must call
		/// <see cref="IAppender.Close"/> on the appender removed.
		/// </para>
		/// </remarks>
		IAppender RemoveAppender(IAppender appender);

		/// <summary>
		/// Removes the appender with the specified name from the list of appenders.
		/// </summary>
		/// <param name="name">The name of the appender to remove.</param>
		/// <returns>The appender removed from the list</returns>
		/// <remarks>
		/// <para>
		/// The appender removed is not closed.
		/// If you are discarding the appender you must call
		/// <see cref="IAppender.Close"/> on the appender removed.
		/// </para>
		/// </remarks>
		IAppender RemoveAppender(string name);   	
	}
}
