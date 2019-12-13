// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Data;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    public class CopyModuleItem : IHydratable
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int KeyID
        {
            get { return Id; }
            set { Id = value; }
        }

        public void Fill(IDataReader dr)
        {
            Id = Convert.ToInt32(dr["ModuleId"]);
            Title = Convert.ToString(dr["ModuleTitle"]);
        }
    }
}
