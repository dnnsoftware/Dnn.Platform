// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Application
{
    using System;

    /// <summary>
    /// The enumeration of release mode.
    /// </summary>
    /// <value>
    /// <list type="bullet">
    ///         <item>None: Not specified for the current release.</item>
    ///         <item>Alpha:Alpha release is an opportunity for customers to get an early look at a particular software feature.</item>
    ///         <item>Beta: Beta release is a mostly completed release,
    ///                 At this point we will have implemented most of the major features planned for a specific release. </item>
    ///         <item>RC: RC release will be the Stable release if there is no major show-stopping bugs,
    ///                 We have gone through all the major test scenarios and are just running through a final set of regression
    ///                 tests and verifying the packaging.</item>
    ///         <item>Stable: Stable release is believed to be ready for use,
    ///                 remember that only stable release can be used in production environment.</item>
    /// </list>
    /// </value>
    public enum ReleaseMode
    {
        /// <summary>
        /// Not asssigned
        /// </summary>
        None,

        /// <summary>
        /// Alpha release
        /// </summary>
        Alpha,

        /// <summary>
        /// Beta release
        /// </summary>
        Beta,

        /// <summary>
        /// Release candidate
        /// </summary>
        RC,

        /// <summary>
        /// Stable release version
        /// </summary>
        Stable,
    }

    /// <summary>
    /// The status of current assembly.
    /// </summary>
    /// <example>
    /// [assembly: AssemblyStatus(ReleaseMode.Stable)].
    /// </example>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyStatusAttribute : Attribute
    {
        private readonly ReleaseMode _releaseMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyStatusAttribute" /> class.
        /// </summary>
        /// <param name="releaseMode">The release mode.</param>
        public AssemblyStatusAttribute(ReleaseMode releaseMode)
        {
            this._releaseMode = releaseMode;
        }

        /// <summary>
        /// Gets status of current assembly.
        /// </summary>
        public ReleaseMode Status
        {
            get
            {
                return this._releaseMode;
            }
        }
    }
}
