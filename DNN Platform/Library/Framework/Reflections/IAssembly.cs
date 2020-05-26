// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Framework.Internal.Reflection
{
    //interface to allowing mocking of System.Reflection.Assembly
    public interface IAssembly
    {
        Type[] GetTypes();
    }
}
