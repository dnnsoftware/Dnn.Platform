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

    [TableName("[OWNER]_[MODULE]s")]
    [PrimaryKey("[MODULE]ID")]
    [Scope("ModuleID")]
    [Cacheable("[MODULE]s", CacheItemPriority.Default, 20)]
    public class [MODULE]Info
    {
        public int [MODULE]ID { get; set; }
        public int ModuleID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserID { get; set; }
    }
}
