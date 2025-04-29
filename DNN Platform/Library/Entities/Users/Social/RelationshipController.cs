// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social;

using System;

using DotNetNuke.Framework;

/// <summary>Business Layer to manage Relationships. Also contains CRUD methods.</summary>
public class RelationshipController : ServiceLocator<IRelationshipController, RelationshipController>
{
    /// <inheritdoc/>
    protected override Func<IRelationshipController> GetFactory()
    {
        return () => new RelationshipControllerImpl();
    }
}
