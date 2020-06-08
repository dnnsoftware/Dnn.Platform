// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Using Statements

using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

#endregion

namespace _OWNER_._MODULE_
{

    [TableName("_OWNER___CONTROL_s")]
    [PrimaryKey("_CONTROL_ID")]
    [Scope("ModuleID")]
    [Cacheable("_CONTROL_s", CacheItemPriority.Default, 20)]
    public class _CONTROL_Info
    {
        public int _CONTROL_ID { get; set; }
        public int ModuleID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserID { get; set; }
    }
}
