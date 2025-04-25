// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    /// <summary>Identifies common tab move types.</summary>
    public enum TabMoveType
    {
        /// <summary>Move up.</summary>
        Up = 0,

        /// <summary>Move down.</summary>
        Down = 1,

        /// <summary>Move to the top.</summary>
        Top = 2,

        /// <summary>Move to the bottom.</summary>
        Bottom = 3,

        /// <summary>Promote.</summary>
        Promote = 4,

        /// <summary>Demote.</summary>
        Demote = 5,
    }
}
