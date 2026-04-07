// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Claims
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The ClaimController class provides Business Layer methods for Claims.</summary>
    public class ClaimController : ServiceLocator<IClaimController, ClaimController>, IClaimController
    {
        private static readonly Dictionary<ClaimStatus, ClaimStatus[]> ValidTransitions = new Dictionary<ClaimStatus, ClaimStatus[]>
        {
            { ClaimStatus.New, new[] { ClaimStatus.Closed } },
            { ClaimStatus.Closed, new[] { ClaimStatus.Reopened } },
            { ClaimStatus.Reopened, new[] { ClaimStatus.Closed } },
        };

        private readonly DataProvider dataProvider;

        /// <summary>Initializes a new instance of the <see cref="ClaimController"/> class.</summary>
        public ClaimController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ClaimController"/> class.</summary>
        /// <param name="dataProvider">The data provider.</param>
        public ClaimController(DataProvider dataProvider)
        {
            this.dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        }

        /// <inheritdoc />
        public ClaimInfo GetClaim(int claimId)
        {
            return CBO.FillObject<ClaimInfo>(this.dataProvider.ExecuteReader("Claims_GetClaim", claimId));
        }

        /// <inheritdoc />
        public IList<ClaimInfo> GetClaimsByPortal(int portalId)
        {
            return CBO.FillCollection<ClaimInfo>(this.dataProvider.ExecuteReader("Claims_GetClaimsByPortal", portalId));
        }

        /// <inheritdoc />
        public IList<ClaimInfo> GetClaimsByUser(int portalId, int userId)
        {
            return CBO.FillCollection<ClaimInfo>(this.dataProvider.ExecuteReader("Claims_GetClaimsByUser", portalId, userId));
        }

        /// <inheritdoc />
        public int AddClaim(ClaimInfo claim)
        {
            Requires.NotNull("claim", claim);
            claim.Status = ClaimStatus.New;
            return this.dataProvider.ExecuteScalar<int>(
                "Claims_AddClaim",
                claim.PortalId,
                claim.UserId,
                claim.Subject,
                claim.Description,
                (int)claim.Status);
        }

        /// <inheritdoc />
        public void UpdateClaim(ClaimInfo claim)
        {
            Requires.NotNull("claim", claim);
            this.dataProvider.ExecuteNonQuery(
                "Claims_UpdateClaim",
                claim.ClaimId,
                claim.Subject,
                claim.Description);
        }

        /// <inheritdoc />
        public void DeleteClaim(int claimId)
        {
            this.dataProvider.ExecuteNonQuery("Claims_DeleteClaim", claimId);
        }

        /// <inheritdoc />
        public void ChangeStatus(int claimId, ClaimStatus newStatus)
        {
            var claim = this.GetClaim(claimId);
            if (claim == null)
            {
                throw new ArgumentException($"Claim with ID {claimId} not found.", nameof(claimId));
            }

            ValidateStatusTransition(claim.Status, newStatus);

            this.dataProvider.ExecuteNonQuery("Claims_ChangeStatus", claimId, (int)newStatus);
        }

        internal static void ValidateStatusTransition(ClaimStatus currentStatus, ClaimStatus newStatus)
        {
            if (currentStatus == newStatus)
            {
                return;
            }

            if (!ValidTransitions.TryGetValue(currentStatus, out var allowedStatuses))
            {
                throw new InvalidClaimStatusTransitionException(currentStatus, newStatus);
            }

            foreach (var allowed in allowedStatuses)
            {
                if (allowed == newStatus)
                {
                    return;
                }
            }

            throw new InvalidClaimStatusTransitionException(currentStatus, newStatus);
        }

        /// <inheritdoc />
        protected override Func<IClaimController> GetFactory()
        {
            return () => new ClaimController();
        }
    }
}
