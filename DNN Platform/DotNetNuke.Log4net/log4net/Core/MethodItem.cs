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
using System.Collections;

using log4net.Util;

namespace log4net.Core
{
	/// <summary>
	/// provides method information without actually referencing a System.Reflection.MethodBase
	/// as that would require that the containing assembly is loaded.
	/// </summary>
	/// 
#if !NETCF
	[Serializable]
#endif
	public class MethodItem
	{
		#region Public Instance Constructors

		/// <summary>
		/// constructs a method item for an unknown method.
		/// </summary>
		public MethodItem()
		{
			m_name = NA;
			m_parameters = new string[0];
		}

		/// <summary>
		/// constructs a method item from the name of the method.
		/// </summary>
		/// <param name="name"></param>
		public MethodItem(string name)
			: this()
		{
			m_name = name;
		}

		/// <summary>
		/// constructs a method item from the name of the method and its parameters.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		public MethodItem(string name, string[] parameters)
			: this(name)
		{
			m_parameters = parameters;
		}

        /// <summary>
        /// constructs a method item from a method base by determining the method name and its parameters.
        /// </summary>
        /// <param name="methodBase"></param>
		public MethodItem(System.Reflection.MethodBase methodBase)
			: this(methodBase.Name, GetMethodParameterNames(methodBase))
        {
		}

		#endregion

		private static string[] GetMethodParameterNames(System.Reflection.MethodBase methodBase)
		{
			ArrayList methodParameterNames = new ArrayList();
			try
			{
				System.Reflection.ParameterInfo[] methodBaseGetParameters = methodBase.GetParameters();

				int methodBaseGetParametersCount = methodBaseGetParameters.GetUpperBound(0);

				for (int i = 0; i <= methodBaseGetParametersCount; i++)
				{
					methodParameterNames.Add(methodBaseGetParameters[i].ParameterType + " " + methodBaseGetParameters[i].Name);
				}
			}
			catch (Exception ex)
			{
				LogLog.Error(declaringType, "An exception ocurred while retreiving method parameters.", ex);
			}

			return (string[])methodParameterNames.ToArray(typeof(string));
		}

		#region Public Instance Properties

		/// <summary>
		/// Gets the method name of the caller making the logging 
		/// request.
		/// </summary>
		/// <value>
		/// The method name of the caller making the logging 
		/// request.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the method name of the caller making the logging 
		/// request.
		/// </para>
		/// </remarks>
		public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Gets the method parameters of the caller making
		/// the logging request.
		/// </summary>
		/// <value>
		/// The method parameters of the caller making
		/// the logging request
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the method parameters of the caller making
		/// the logging request.
		/// </para>
		/// </remarks>
		public string[] Parameters
		{
			get { return m_parameters; }
		}

		#endregion

		#region Private Instance Fields

		private readonly string m_name;
		private readonly string[] m_parameters;

		#endregion

		#region Private Static Fields

		/// <summary>
		/// The fully qualified type of the StackFrameItem class.
		/// </summary>
		/// <remarks>
		/// Used by the internal logger to record the Type of the
		/// log message.
		/// </remarks>
		private readonly static Type declaringType = typeof(MethodItem);

		/// <summary>
		/// When location information is not available the constant
		/// <c>NA</c> is returned. Current value of this string
		/// constant is <b>?</b>.
		/// </summary>
		private const string NA = "?";

		#endregion Private Static Fields
	}
}
