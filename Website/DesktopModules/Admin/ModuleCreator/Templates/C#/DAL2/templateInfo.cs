#region Copyright

// 
// Copyright (c) [YEAR]
// by [OWNER]
// 

#endregion

#region Using Statements

using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

#endregion

namespace [OWNER].[MODULE]
{

    [TableName("[OWNER]_[CONTROL]s")]
    [PrimaryKey("[CONTROL]ID")]
    [Scope("ModuleID")]
    [Cacheable("[CONTROL]s", CacheItemPriority.Default, 20)]
    public class [CONTROL]Info
    {
        public int [CONTROL]ID { get; set; }
        public int ModuleID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserID { get; set; }
    }
}
