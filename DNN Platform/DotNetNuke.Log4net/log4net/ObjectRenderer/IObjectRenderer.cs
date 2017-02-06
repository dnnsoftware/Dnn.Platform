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
using System.IO;

namespace log4net.ObjectRenderer
{
	/// <summary>
	/// Implement this interface in order to render objects as strings
	/// </summary>
	/// <remarks>
	/// <para>
	/// Certain types require special case conversion to
	/// string form. This conversion is done by an object renderer.
	/// Object renderers implement the <see cref="IObjectRenderer"/>
	/// interface.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IObjectRenderer
	{
		/// <summary>
		/// Render the object <paramref name="obj"/> to a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="obj">The object to render</param>
		/// <param name="writer">The writer to render to</param>
		/// <remarks>
		/// <para>
		/// Render the object <paramref name="obj"/> to a 
		/// string.
		/// </para>
		/// <para>
		/// The <paramref name="rendererMap"/> parameter is
		/// provided to lookup and render other objects. This is
		/// very useful where <paramref name="obj"/> contains
		/// nested objects of unknown type. The <see cref="M:RendererMap.FindAndRender(object, TextWriter)"/>
		/// method can be used to render these objects.
		/// </para>
		/// </remarks>
		void RenderObject(RendererMap rendererMap, object obj, TextWriter writer);
	}
}
