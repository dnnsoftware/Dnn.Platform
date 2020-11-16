// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.Roles
{
    using System;

    public class ExportRole : BasicExportImportDto
    {
        public int RoleID { get; set; }

        public string RoleName { get; set; }

        public string Description { get; set; }

        public decimal? ServiceFee { get; set; }

        public string BillingFrequency { get; set; }

        public int? TrialPeriod { get; set; }

        public string TrialFrequency { get; set; }

        public int? BillingPeriod { get; set; }

        public decimal? TrialFee { get; set; }

        public bool IsPublic { get; set; }

        public bool AutoAssignment { get; set; }

        public int? RoleGroupID { get; set; }

        public string RSVPCode { get; set; }

        public string IconFile { get; set; }

        public int? CreatedByUserID { get; set; }

        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserID { get; set; }

        public DateTime? LastModifiedOnDate { get; set; }

        public int Status { get; set; }

        public int SecurityMode { get; set; }

        public bool IsSystemRole { get; set; }

        public string CreatedByUserName { get; set; }

        public string LastModifiedByUserName { get; set; }
    }
}
