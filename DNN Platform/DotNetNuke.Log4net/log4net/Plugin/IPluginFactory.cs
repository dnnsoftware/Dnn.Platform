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

namespace log4net.Plugin
{
	/// <summary>
	/// Interface used to create plugins.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Interface used to create  a plugin.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IPluginFactory
	{
		/// <summary>
		/// Creates the plugin object.
		/// </summary>
		/// <returns>the new plugin instance</returns>
		/// <remarks>
		/// <para>
		/// Create and return a new plugin instance.
		/// </para>
		/// </remarks>
		IPlugin CreatePlugin();
	}
}
