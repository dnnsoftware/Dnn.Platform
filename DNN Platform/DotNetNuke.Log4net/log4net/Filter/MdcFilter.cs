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
using System.Text.RegularExpressions;

using log4net;
using log4net.Core;
using log4net.Util;

namespace log4net.Filter
{
	/// <summary>
	/// Simple filter to match a keyed string in the <see cref="MDC"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Simple filter to match a keyed string in the <see cref="MDC"/>
	/// </para>
	/// <para>
	/// As the MDC has been replaced with layered properties the
	/// <see cref="PropertyFilter"/> should be used instead.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/*[Obsolete("MdcFilter has been replaced by PropertyFilter")]*/
	public class MdcFilter : PropertyFilter
	{
	}
}
