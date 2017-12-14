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

#if !NETCF
using System;
using System.Collections;

namespace log4net.Util
{
	/// <summary>
	/// Implementation of Stacks collection for the <see cref="log4net.LogicalThreadContext"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Implementation of Stacks collection for the <see cref="log4net.LogicalThreadContext"/>
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class LogicalThreadContextStacks
	{
		private readonly LogicalThreadContextProperties m_properties;

		#region Public Instance Constructors

		/// <summary>
		/// Internal constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ThreadContextStacks" /> class.
		/// </para>
		/// </remarks>
		internal LogicalThreadContextStacks(LogicalThreadContextProperties properties)
		{
			m_properties = properties;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets the named thread context stack
		/// </summary>
		/// <value>
		/// The named stack
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the named thread context stack
		/// </para>
		/// </remarks>
		public LogicalThreadContextStack this[string key]
		{
			get
			{
				LogicalThreadContextStack stack = null;

				object propertyValue = m_properties[key];
				if (propertyValue == null)
				{
					// Stack does not exist, create
#if NET_2_0 || MONO_2_0
					stack = new LogicalThreadContextStack(key, registerNew);
#else
					stack = new LogicalThreadContextStack(key, new TwoArgAction(registerNew));
#endif
					m_properties[key] = stack;
				}
				else
				{
					// Look for existing stack
					stack = propertyValue as LogicalThreadContextStack;
					if (stack == null)
					{
						// Property is not set to a stack!
						string propertyValueString = SystemInfo.NullText;

						try
						{
							propertyValueString = propertyValue.ToString();
						}
						catch
						{
						}

						LogLog.Error(declaringType, "ThreadContextStacks: Request for stack named [" + key + "] failed because a property with the same name exists which is a [" + propertyValue.GetType().Name + "] with value [" + propertyValueString + "]");

#if NET_2_0 || MONO_2_0
						stack = new LogicalThreadContextStack(key, registerNew);
#else
						stack = new LogicalThreadContextStack(key, new TwoArgAction(registerNew));
#endif
					}
				}

				return stack;
			}
		}

		#endregion Public Instance Properties

		#region Private Instance Fields

		private void registerNew(string stackName, LogicalThreadContextStack stack)
		{
			m_properties[stackName] = stack;
		}

		#endregion Private Instance Fields

		#region Private Static Fields

		/// <summary>
		/// The fully qualified type of the ThreadContextStacks class.
		/// </summary>
		/// <remarks>
		/// Used by the internal logger to record the Type of the
		/// log message.
		/// </remarks>
		private readonly static Type declaringType = typeof(LogicalThreadContextStacks);

		#endregion Private Static Fields
	}
}

#endif
