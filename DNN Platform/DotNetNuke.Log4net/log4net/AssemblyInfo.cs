// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
using System.Reflection;
using System.Runtime.CompilerServices;

#if !SSCLI
//
// log4net makes use of static methods which cannot be made com visible
//
[assembly: System.Runtime.InteropServices.ComVisible(false)]
#endif

//
// log4net is CLS compliant
//
[assembly: System.CLSCompliant(true)]

#if !NETCF
//
// If log4net is strongly named it still allows partially trusted callers
//
[assembly: System.Security.AllowPartiallyTrustedCallers]
#endif

#if NET_4_0
//
// Allows partial trust applications (e.g. ASP.NET shared hosting) on .NET 4.0 to work
// given our implementation of ISerializable.
//
[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]
#endif

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
#if CLI_1_0
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.CLI 1.0")]
[assembly: AssemblyTitle("Apache log4net for CLI 1.0 Compatible Frameworks")]
#elif NET_1_0
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET 1.0")]
[assembly: AssemblyTitle("Apache log4net for .NET Framework 1.0")]
#elif NET_1_1
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET 1.1")]
[assembly: AssemblyTitle("Apache log4net for .NET Framework 1.1")]
#elif NET_4_5
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET 4.5")]
[assembly: AssemblyTitle("Apache log4net for .NET Framework 4.5")]
#elif NET_4_0
#if CLIENT_PROFILE
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET 4.0 CP")]
[assembly: AssemblyTitle("Apache log4net for .NET Framework 4.0 Client Profile")]
#else
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET 4.0")]
[assembly: AssemblyTitle("Apache log4net for .NET Framework 4.0")]
#endif // Client Profile
#elif NET_2_0
#if CLIENT_PROFILE
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET 3.5 CP")]
[assembly: AssemblyTitle("Apache log4net for .NET Framework 3.5 Client Profile")]
#else
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET 2.0")]
[assembly: AssemblyTitle("Apache log4net for .NET Framework 2.0")]
#endif // Client Profile
#elif NETCF_1_0
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NETCF 1.0")]
[assembly: AssemblyTitle("Apache log4net for .NET Compact Framework 1.0")]
#elif NETCF_2_0
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NETCF 2.0")]
[assembly: AssemblyTitle("Apache log4net for .NET Compact Framework 2.0")]
#elif MONO_1_0
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-Mono 1.0")]
[assembly: AssemblyTitle("Apache log4net for Mono 1.0")]
#elif MONO_2_0
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-Mono 2.0")]
[assembly: AssemblyTitle("Apache log4net for Mono 2.0")]
#elif MONO_3_5
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-Mono 3.5")]
[assembly: AssemblyTitle("Apache log4net for Mono 3.5")]
#elif MONO_4_0
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-Mono 4.0")]
[assembly: AssemblyTitle("Apache log4net for Mono 4.0")]
#elif SSCLI_1_0
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-SSCLI 1.0")]
[assembly: AssemblyTitle("Apache log4net for Shared Source CLI 1.0")]
#elif NET
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET")]
[assembly: AssemblyTitle("Apache log4net for .NET Framework")]
#elif NETSTANDARD1_3
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NET Core 1.0")]
[assembly: AssemblyTitle("DotNetNuke.log4net for .NET Core 1.0")]
#elif NETCF
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-.NETCF")]
[assembly: AssemblyTitle("DotNetNuke.log4net for .NET Compact Framework")]
#elif MONO
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-Mono")]
[assembly: AssemblyTitle("DotNetNuke.log4net for Mono")]
#elif SSCLI
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0-SSCLI")]
[assembly: AssemblyTitle("DotNetNuke.log4net for Shared Source CLI")]
#else
[assembly: AssemblyInformationalVersionAttribute("2.0.6.0")]
[assembly: AssemblyTitle("DotNetNuke.log4net")]
#endif

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif

[assembly: AssemblyProduct("log4net")]
[assembly: AssemblyDefaultAlias("log4net")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyDescription("DotNetNuke branded version of log4net")]

#if STRONG && (CLI_1_0 || NET_1_0 || NET_1_1 || NETCF_1_0 || SSCLI)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\log4net.snk")]
#endif

// We do not use a CSP key for strong naming
// [assembly: AssemblyKeyName("")]
