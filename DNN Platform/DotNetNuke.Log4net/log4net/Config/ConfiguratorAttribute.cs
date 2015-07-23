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

// .NET Compact Framework 1.0 has no support for reading assembly attributes
#if !NETCF

using System;
using System.Reflection;

using log4net.Repository;

namespace log4net.Config
{
	/// <summary>
	/// Base class for all log4net configuration attributes.
	/// </summary>
	/// <remarks>
	/// This is an abstract class that must be extended by 
	/// specific configurators. This attribute allows the
	/// configurator to be parameterized by an assembly level
	/// attribute.
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	[AttributeUsage(AttributeTargets.Assembly)]
	public abstract class ConfiguratorAttribute : Attribute, IComparable
	{
		private int m_priority = 0;

		/// <summary>
		/// Constructor used by subclasses.
		/// </summary>
		/// <param name="priority">the ordering priority for this configurator</param>
		/// <remarks>
		/// <para>
		/// The <paramref name="priority"/> is used to order the configurator
		/// attributes before they are invoked. Higher priority configurators are executed
		/// before lower priority ones.
		/// </para>
		/// </remarks>
		protected ConfiguratorAttribute(int priority)
		{
			m_priority = priority;
		}

		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> for the specified assembly.
		/// </summary>
		/// <param name="sourceAssembly">The assembly that this attribute was defined on.</param>
		/// <param name="targetRepository">The repository to configure.</param>
		/// <remarks>
		/// <para>
		/// Abstract method implemented by a subclass. When this method is called
		/// the subclass should configure the <paramref name="targetRepository"/>.
		/// </para>
		/// </remarks>
		public abstract void Configure(Assembly sourceAssembly, ILoggerRepository targetRepository);

		/// <summary>
		/// Compare this instance to another ConfiguratorAttribute
		/// </summary>
		/// <param name="obj">the object to compare to</param>
		/// <returns>see <see cref="IComparable.CompareTo"/></returns>
		/// <remarks>
		/// <para>
		/// Compares the priorities of the two <see cref="ConfiguratorAttribute"/> instances.
		/// Sorts by priority in descending order. Objects with the same priority are
		/// randomly ordered.
		/// </para>
		/// </remarks>
		public int CompareTo(object obj)
		{
			// Reference equals
			if ((object)this == obj)
			{
				return 0;
			}

			int result = -1;

			ConfiguratorAttribute target = obj as ConfiguratorAttribute;
			if (target != null)
			{
				// Compare the priorities
				result = target.m_priority.CompareTo(m_priority);
				if (result == 0)
				{
					// Same priority, so have to provide some ordering
					result = -1;
				}
			}
			return result;
		}
	}
}

#endif //!NETCF