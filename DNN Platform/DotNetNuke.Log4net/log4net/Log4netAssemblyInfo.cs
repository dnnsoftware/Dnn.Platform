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

namespace log4net {

    /// <summary>
    /// Provides information about the environment the assembly has
    /// been built for.
    /// </summary>
    public sealed class AssemblyInfo {
        /// <summary>Version of the assembly</summary>
        public const string Version = "2.0.9";

        /// <summary>Version of the framework targeted</summary>
#if NET_1_1
        public const decimal TargetFrameworkVersion = 1.1M;
#elif NET_4_5
        public const decimal TargetFrameworkVersion = 4.5M;
#elif NET_4_0 || MONO_4_0
        public const decimal TargetFrameworkVersion = 4.5M;
#elif FRAMEWORK_4_0_OR_ABOVE
        public const decimal TargetFrameworkVersion = 4.0M;
#elif MONO_3_5
        public const decimal TargetFrameworkVersion = 3.5M;
#elif NET_2_0 || NETCF_2_0 || MONO_2_0
#if !CLIENT_PROFILE
        public const decimal TargetFrameworkVersion = 2.0M;
#else
        public const decimal TargetFrameworkVersion = 3.5M;
#endif  // Client Profile
#else
        public const decimal TargetFrameworkVersion = 1.0M;
#endif

        /// <summary>Type of framework targeted</summary>
#if CLI
        public const string TargetFramework = "CLI Compatible Frameworks";
#elif NET
        public const string TargetFramework = ".NET Framework";
#elif NETCF
        public const string TargetFramework = ".NET Compact Framework";
#elif MONO
        public const string TargetFramework = "Mono";
#elif SSCLI
        public const string TargetFramework = "Shared Source CLI";
#elif NETSTANDARD1_3
        public const string TargetFramework = ".NET Core";
#else
        public const string TargetFramework = "Unknown";
#endif

        /// <summary>Does it target a client profile?</summary>
#if !CLIENT_PROFILE
        public const bool ClientProfile = false;
#else
        public const bool ClientProfile = true;
#endif

        /// <summary>
        /// Identifies the version and target for this assembly.
        /// </summary>
        public static string Info {
            get {
                return string.Format("Apache log4net version {0} compiled for {1}{2} {3}",
                                     Version, TargetFramework,
                                     /* Can't use
                                     ClientProfile && true ? " Client Profile" :
                                        or the compiler whines about unreachable expressions
                                     */
#if !CLIENT_PROFILE
                                     string.Empty,
#else
                                     " Client Profile",
#endif
                                     TargetFrameworkVersion);
            }
        }
    }

}
