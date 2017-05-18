#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;

#endregion

namespace DotNetNuke.Application
{
	/// <summary>
	/// The enumeration of release mode.
	/// </summary>
	/// <value>
	/// <list type="bullet">
	///		<item>None: Not specified for the current release.</item>
	///		<item>Alpha:Alpha release is an opportunity for customers to get an early look at a particular software feature.</item>
	///		<item>Beta: Beta release is a mostly completed release, 
	///				At this point we will have implemented most of the major features planned for a specific release. </item>
	///		<item>RC: RC relase will be the Stable release if there is none major show-stopping bugs, 
	///				We have gone through all the major test scenarios and are just running through a final set of regression 
	///				tests and verifying the packaging.</item>
	///		<item>Stable: Stable release is believed to be ready for use, 
	///				remember that only stable release can be used in production environment.</item>
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
        Stable
    }

	/// <summary>
	/// The status of current assembly.
	/// </summary>
	/// <example>
	/// [assembly: AssemblyStatus(ReleaseMode.Stable)]
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
            _releaseMode = releaseMode;
        }


		/// <summary>
		/// Status of current assembly.
		/// </summary>
        public ReleaseMode Status
        {
            get
            {
                return _releaseMode;
            }
        }
    }
}
