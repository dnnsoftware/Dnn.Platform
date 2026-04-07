// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Claims
{
    using System.Collections.Generic;

    /// <summary>Defines the contract for claim management operations.</summary>
    public interface IClaimController
    {
        /// <summary>Gets a claim by its ID.</summary>
        /// <param name="claimId">The claim ID.</param>
        /// <returns>A <see cref="ClaimInfo"/> object, or null if not found.</returns>
        ClaimInfo GetClaim(int claimId);

        /// <summary>Gets all claims for a portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A list of <see cref="ClaimInfo"/> objects.</returns>
        IList<ClaimInfo> GetClaimsByPortal(int portalId);

        /// <summary>Gets all claims for a user.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>A list of <see cref="ClaimInfo"/> objects.</returns>
        IList<ClaimInfo> GetClaimsByUser(int portalId, int userId);

        /// <summary>Adds a new claim.</summary>
        /// <param name="claim">The claim to add.</param>
        /// <returns>The ID of the newly created claim.</returns>
        int AddClaim(ClaimInfo claim);

        /// <summary>Updates an existing claim.</summary>
        /// <param name="claim">The claim to update.</param>
        void UpdateClaim(ClaimInfo claim);

        /// <summary>Deletes a claim.</summary>
        /// <param name="claimId">The claim ID to delete.</param>
        void DeleteClaim(int claimId);

        /// <summary>Changes the status of a claim, enforcing valid transitions.</summary>
        /// <param name="claimId">The claim ID.</param>
        /// <param name="newStatus">The new status.</param>
        /// <exception cref="InvalidClaimStatusTransitionException">Thrown when the status transition is not valid.</exception>
        void ChangeStatus(int claimId, ClaimStatus newStatus);
    }
}
