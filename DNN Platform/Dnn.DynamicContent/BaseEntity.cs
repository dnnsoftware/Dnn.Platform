// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Data.SqlTypes;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Dnn.DynamicContent
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            CreatedByUserId = -1;
            LastModifiedByUserId = -1;
            CreatedOnDate = SqlDateTime.MinValue.Value;
            LastModifiedOnDate = SqlDateTime.MinValue.Value;
        }

        public int CreatedByUserId { get; set; }

        public DateTime CreatedOnDate { get; set; }

        public int LastModifiedByUserId { get; set; }

        public DateTime LastModifiedOnDate { get; set; }
    }
}
