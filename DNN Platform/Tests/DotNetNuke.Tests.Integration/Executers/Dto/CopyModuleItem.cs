// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
