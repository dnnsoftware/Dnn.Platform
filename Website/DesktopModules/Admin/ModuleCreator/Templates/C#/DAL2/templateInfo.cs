#region Copyright

// 
// Copyright (c) _YEAR_
// by _OWNER_
// 

#endregion

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
