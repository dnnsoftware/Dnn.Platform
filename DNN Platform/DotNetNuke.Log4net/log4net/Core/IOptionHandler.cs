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
	/// Interface used to delay activate a configured object.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This allows an object to defer activation of its options until all
	/// options have been set. This is required for components which have
	/// related options that remain ambiguous until all are set.
	/// </para>
	/// <para>
	/// If a component implements this interface then the <see cref="ActivateOptions"/> method 
	/// must be called by the container after its all the configured properties have been set 
	/// and before the component can be used.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public interface IOptionHandler
	{
		/// <summary>
		/// Activate the options that were previously set with calls to properties.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This allows an object to defer activation of its options until all
		/// options have been set. This is required for components which have
		/// related options that remain ambiguous until all are set.
		/// </para>
		/// <para>
		/// If a component implements this interface then this method must be called
		/// after its properties have been set before the component can be used.
		/// </para>
		/// </remarks>
		void ActivateOptions();
	}
}
