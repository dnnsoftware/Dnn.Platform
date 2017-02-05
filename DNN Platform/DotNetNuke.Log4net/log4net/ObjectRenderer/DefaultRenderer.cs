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
using System.Text;
using System.IO;
using System.Collections;

using log4net.Util;

namespace log4net.ObjectRenderer
{
	/// <summary>
	/// The default object Renderer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The default renderer supports rendering objects and collections to strings.
	/// </para>
	/// <para>
	/// See the <see cref="RenderObject"/> method for details of the output.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class DefaultRenderer : IObjectRenderer
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor
		/// </para>
		/// </remarks>
		public DefaultRenderer()
		{
		}

		#endregion

		#region Implementation of IObjectRenderer

		/// <summary>
		/// Render the object <paramref name="obj"/> to a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="obj">The object to render</param>
		/// <param name="writer">The writer to render to</param>
		/// <remarks>
		/// <para>
		/// Render the object <paramref name="obj"/> to a string.
		/// </para>
		/// <para>
		/// The <paramref name="rendererMap"/> parameter is
		/// provided to lookup and render other objects. This is
		/// very useful where <paramref name="obj"/> contains
		/// nested objects of unknown type. The <see cref="M:RendererMap.FindAndRender(object)"/>
		/// method can be used to render these objects.
		/// </para>
		/// <para>
		/// The default renderer supports rendering objects to strings as follows:
		/// </para>
		/// <list type="table">
		///		<listheader>
		///			<term>Value</term>
		///			<description>Rendered String</description>
		///		</listheader>
		///		<item>
		///			<term><c>null</c></term>
		///			<description>
		///			<para>"(null)"</para>
		///			</description>
		///		</item>
		///		<item>
		///			<term><see cref="Array"/></term>
		///			<description>
		///			<para>
		///			For a one dimensional array this is the
		///			array type name, an open brace, followed by a comma
		///			separated list of the elements (using the appropriate
		///			renderer), followed by a close brace. 
		///			</para>
		///			<para>
		///			For example: <c>int[] {1, 2, 3}</c>.
		///			</para>
		///			<para>
		///			If the array is not one dimensional the 
		///			<c>Array.ToString()</c> is returned.
		///			</para>
		///			</description>
		///		</item>
		///		<item>
		///			<term><see cref="IEnumerable"/>, <see cref="ICollection"/> &amp; <see cref="IEnumerator"/></term>
		///			<description>
		///			<para>
		///			Rendered as an open brace, followed by a comma
		///			separated list of the elements (using the appropriate
		///			renderer), followed by a close brace.
		///			</para>
		///			<para>
		///			For example: <c>{a, b, c}</c>.
		///			</para>
		///			<para>
		///			All collection classes that implement <see cref="ICollection"/> its subclasses, 
		///			or generic equivalents all implement the <see cref="IEnumerable"/> interface.
		///			</para>
		///			</description>
		///		</item>		
		///		<item>
		///			<term><see cref="DictionaryEntry"/></term>
		///			<description>
		///			<para>
		///			Rendered as the key, an equals sign ('='), and the value (using the appropriate
		///			renderer). 
		///			</para>
		///			<para>
		///			For example: <c>key=value</c>.
		///			</para>
		///			</description>
		///		</item>		
		///		<item>
		///			<term>other</term>
		///			<description>
		///			<para><c>Object.ToString()</c></para>
		///			</description>
		///		</item>
		/// </list>
		/// </remarks>
		public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
		{
			if (rendererMap == null)
			{
				throw new ArgumentNullException("rendererMap");
			}

			if (obj == null)
			{
				writer.Write(SystemInfo.NullText);
				return;
			}
			
			Array objArray = obj as Array;
			if (objArray != null)
			{
				RenderArray(rendererMap, objArray, writer);
				return;
			}

			// Test if we are dealing with some form of collection object
			IEnumerable objEnumerable = obj as IEnumerable;
			if (objEnumerable != null)
			{
				// Get a collection interface if we can as its .Count property may be more
				// performant than getting the IEnumerator object and trying to advance it.
				ICollection objCollection = obj as ICollection;
				if (objCollection != null && objCollection.Count == 0)
				{
					writer.Write("{}");
					return;
				}
				
				// This is a special check to allow us to get the enumerator from the IDictionary
				// interface as this guarantees us DictionaryEntry objects. Note that in .NET 2.0
				// the generic IDictionary<> interface enumerates KeyValuePair objects rather than
				// DictionaryEntry ones. However the implementation of the plain IDictionary 
				// interface on the generic Dictionary<> still returns DictionaryEntry objects.
				IDictionary objDictionary = obj as IDictionary;
				if (objDictionary != null)
				{
					RenderEnumerator(rendererMap, objDictionary.GetEnumerator(), writer);
					return;
				}

				RenderEnumerator(rendererMap, objEnumerable.GetEnumerator(), writer);
				return;
			}

			IEnumerator objEnumerator = obj as IEnumerator;
			if (objEnumerator != null)
			{
				RenderEnumerator(rendererMap, objEnumerator, writer);
				return;
			}
			
			if (obj is DictionaryEntry)
			{
				RenderDictionaryEntry(rendererMap, (DictionaryEntry)obj, writer);
				return;
			}

			string str = obj.ToString();
			writer.Write( (str==null) ? SystemInfo.NullText : str );
		}

		#endregion

		/// <summary>
		/// Render the array argument into a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="array">the array to render</param>
		/// <param name="writer">The writer to render to</param>
		/// <remarks>
		/// <para>
		/// For a one dimensional array this is the
		///	array type name, an open brace, followed by a comma
		///	separated list of the elements (using the appropriate
		///	renderer), followed by a close brace. For example:
		///	<c>int[] {1, 2, 3}</c>.
		///	</para>
		///	<para>
		///	If the array is not one dimensional the 
		///	<c>Array.ToString()</c> is returned.
		///	</para>
		/// </remarks>
		private void RenderArray(RendererMap rendererMap, Array array, TextWriter writer)
		{
			if (array.Rank != 1)
			{
				writer.Write(array.ToString());
			}
			else
			{
				writer.Write(array.GetType().Name + " {");
				int len = array.Length;

				if (len > 0)
				{
					rendererMap.FindAndRender(array.GetValue(0), writer);
					for(int i=1; i<len; i++)
					{
						writer.Write(", ");
						rendererMap.FindAndRender(array.GetValue(i), writer);
					}
				}
				writer.Write("}");
			}
		}

		/// <summary>
		/// Render the enumerator argument into a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="enumerator">the enumerator to render</param>
		/// <param name="writer">The writer to render to</param>
		/// <remarks>
		/// <para>
		/// Rendered as an open brace, followed by a comma
		///	separated list of the elements (using the appropriate
		///	renderer), followed by a close brace. For example:
		///	<c>{a, b, c}</c>.
		///	</para>
		/// </remarks>
		private void RenderEnumerator(RendererMap rendererMap, IEnumerator enumerator, TextWriter writer)
		{
			writer.Write("{");

			if (enumerator != null && enumerator.MoveNext())
			{
				rendererMap.FindAndRender(enumerator.Current, writer);

				while (enumerator.MoveNext())
				{
					writer.Write(", ");
					rendererMap.FindAndRender(enumerator.Current, writer);
				}
			}

			writer.Write("}");
		}

		/// <summary>
		/// Render the DictionaryEntry argument into a string
		/// </summary>
		/// <param name="rendererMap">The map used to lookup renderers</param>
		/// <param name="entry">the DictionaryEntry to render</param>
		/// <param name="writer">The writer to render to</param>
		/// <remarks>
		/// <para>
		/// Render the key, an equals sign ('='), and the value (using the appropriate
		///	renderer). For example: <c>key=value</c>.
		///	</para>
		/// </remarks>
		private void RenderDictionaryEntry(RendererMap rendererMap, DictionaryEntry entry, TextWriter writer)
		{
			rendererMap.FindAndRender(entry.Key, writer);
			writer.Write("=");
			rendererMap.FindAndRender(entry.Value, writer);
		}	
	}
}
