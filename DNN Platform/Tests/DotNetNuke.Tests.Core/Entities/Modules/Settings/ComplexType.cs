// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    public struct ComplexType
    {
        public ComplexType(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; private set; }

        public int Y { get; private set; }
    }
}
