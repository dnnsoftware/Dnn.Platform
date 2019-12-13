// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Data.Models
{
    [Scope(Constants.CACHE_ScopeModule)]
    public class Cat
    {
        public int? Age { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
    }
}
