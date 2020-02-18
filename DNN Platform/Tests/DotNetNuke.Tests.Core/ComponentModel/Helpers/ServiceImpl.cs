// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Tests.Core.ComponentModel.Helpers
{
    public class ServiceImpl : IService
    {
        private static readonly Random rnd = new Random();
        private readonly int id = rnd.Next();

        public int Id
        {
            get { return id; }
        }
    }
}
