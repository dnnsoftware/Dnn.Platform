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

namespace log4net.Config
{
	/// <summary>
	/// Assembly level attribute that specifies the logging domain for the assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>DomainAttribute is obsolete. Use RepositoryAttribute instead of DomainAttribute.</b>
	/// </para>
	/// <para>
	/// Assemblies are mapped to logging domains. Each domain has its own
	/// logging repository. This attribute specified on the assembly controls
	/// the configuration of the domain. The <see cref="RepositoryAttribute.Name"/> property specifies the name
	/// of the domain that this assembly is a part of. The <see cref="RepositoryAttribute.RepositoryType"/>
	/// specifies the type of the repository objects to create for the domain. If 
	/// this attribute is not specified and a <see cref="RepositoryAttribute.Name"/> is not specified
	/// then the assembly will be part of the default shared logging domain.
	/// </para>
	/// <para>
	/// This attribute can only be specified on the assembly and may only be used
	/// once per assembly.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	[AttributeUsage(AttributeTargets.Assembly)]
	[Serializable]
	[Obsolete("Use RepositoryAttribute instead of DomainAttribute")]
	public sealed class DomainAttribute : RepositoryAttribute
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DomainAttribute" /> class.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Obsolete. Use RepositoryAttribute instead of DomainAttribute.
		/// </para>
		/// </remarks>
		public DomainAttribute() : base()
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DomainAttribute" /> class 
		/// with the name of the domain.
		/// </summary>
		/// <param name="name">The name of the domain.</param>
		/// <remarks>
		/// <para>
		/// Obsolete. Use RepositoryAttribute instead of DomainAttribute.
		/// </para>
		/// </remarks>
		public DomainAttribute(string name) : base(name)
		{
		}

		#endregion Public Instance Constructors
	}
}

#endif // !NETCF