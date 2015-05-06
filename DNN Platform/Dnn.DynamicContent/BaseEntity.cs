// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Dnn.DynamicContent
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            CreatedByUserId = -1;
            LastModifiedByUserId = -1;
        }

        public int CreatedByUserId { get; set; }

        [ReadOnlyColumn]
        public DateTime CreatedOnDate { get; set; }

        public int LastModifiedByUserId { get; set; }

        [ReadOnlyColumn]
        public DateTime LastModifiedOnDate { get; set; }
    }
}
