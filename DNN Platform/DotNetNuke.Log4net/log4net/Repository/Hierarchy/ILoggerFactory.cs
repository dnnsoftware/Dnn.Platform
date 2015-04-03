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

namespace log4net.Repository.Hierarchy
{
	/// <summary>
	/// Interface abstracts creation of <see cref="Logger"/> instances
	/// </summary>
	/// <remarks>
	/// <para>
	/// This interface is used by the <see cref="Hierarchy"/> to 
	/// create new <see cref="Logger"/> objects.
	/// </para>
	/// <para>
	/// The <see cref="CreateLogger"/> method is called
	/// to create a named <see cref="Logger" />.
	/// </para>
	/// <para>
	/// Implement this interface to create new subclasses of <see cref="Logger" />.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface ILoggerFactory
	{
		/// <summary>
		/// Create a new <see cref="Logger" /> instance
		/// </summary>
		/// <param name="repository">The <see cref="ILoggerRepository" /> that will own the <see cref="Logger" />.</param>
		/// <param name="name">The name of the <see cref="Logger" />.</param>
		/// <returns>The <see cref="Logger" /> instance for the specified name.</returns>
		/// <remarks>
		/// <para>
		/// Create a new <see cref="Logger" /> instance with the 
		/// specified name.
		/// </para>
		/// <para>
		/// Called by the <see cref="Hierarchy"/> to create
		/// new named <see cref="Logger"/> instances.
		/// </para>
		/// <para>
		/// If the <paramref name="name"/> is <c>null</c> then the root logger
		/// must be returned.
		/// </para>
		/// </remarks>
		Logger CreateLogger(ILoggerRepository repository, string name);
	}
}
