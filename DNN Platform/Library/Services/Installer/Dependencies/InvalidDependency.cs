// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.Installer.Dependencies
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InvalidDependency signifies a dependency that is always invalid,
    /// taking the place of dependencies that could not be created
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
            _ErrorMessage = ErrorMessage;
        }

        public override string ErrorMessage
        {
            get
            {
                return _ErrorMessage;
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
