// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Dependencies
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InvalidDependency signifies a dependency that is always invalid,
    /// taking the place of dependencies that could not be created.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class InvalidDependency : DependencyBase
    {
        private readonly string _ErrorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDependency" /> class.
        /// </summary>
        /// <param name="ErrorMessage">The error message to display.</param>
        public InvalidDependency(string ErrorMessage)
        {
            this._ErrorMessage = ErrorMessage;
        }

        public override string ErrorMessage
        {
            get
            {
                return this._ErrorMessage;
            }
        }

        public override bool IsValid
        {
            get
            {
                return false;
            }
        }
    }
}
