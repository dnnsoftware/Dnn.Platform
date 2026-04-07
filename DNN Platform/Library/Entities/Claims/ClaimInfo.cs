// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Claims
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>The ClaimInfo class provides the Entity Layer object for managing claims.</summary>
    [Serializable]
    public class ClaimInfo : BaseEntityInfo, IHydratable
    {
        /// <summary>Initializes a new instance of the <see cref="ClaimInfo"/> class.</summary>
        public ClaimInfo()
        {
            this.ClaimId = Null.NullInteger;
            this.PortalId = Null.NullInteger;
            this.UserId = Null.NullInteger;
            this.Status = ClaimStatus.New;
        }

        /// <summary>Gets or sets the claim ID.</summary>
        public int ClaimId { get; set; }

        /// <summary>Gets or sets the portal ID.</summary>
        public int PortalId { get; set; }

        /// <summary>Gets or sets the user ID of the claim owner.</summary>
        public int UserId { get; set; }

        /// <summary>Gets or sets the claim subject.</summary>
        public string Subject { get; set; }

        /// <summary>Gets or sets the claim description.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the claim status.</summary>
        public ClaimStatus Status { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get { return this.ClaimId; }
            set { this.ClaimId = value; }
        }

        /// <inheritdoc />
        public void Fill(IDataReader dr)
        {
            this.ClaimId = Null.SetNullInteger(dr["ClaimId"]);
            this.PortalId = Null.SetNullInteger(dr["PortalId"]);
            this.UserId = Null.SetNullInteger(dr["UserId"]);
            this.Subject = Null.SetNullString(dr["Subject"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.Status = (ClaimStatus)Null.SetNullInteger(dr["Status"]);
            this.FillBaseProperties(dr);
        }
    }
}
