// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Dependencies;

/// <summary>
/// The InvalidDependency signifies a dependency that is always invalid,
/// taking the place of dependencies that could not be created.
/// </summary>
public class InvalidDependency : DependencyBase
{
    private readonly string errorMessage;

    /// <summary>Initializes a new instance of the <see cref="InvalidDependency" /> class.</summary>
    /// <param name="errorMessage">The error message to display.</param>
    public InvalidDependency(string errorMessage)
    {
        this.errorMessage = errorMessage;
    }

    /// <inheritdoc/>
    public override string ErrorMessage
    {
        get
        {
            return this.errorMessage;
        }
    }

    /// <inheritdoc/>
    public override bool IsValid
    {
        get
        {
            return false;
        }
    }
}
