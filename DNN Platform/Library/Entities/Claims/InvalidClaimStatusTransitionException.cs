// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Claims
{
    using System;

    /// <summary>Exception thrown when an invalid claim status transition is attempted.</summary>
    [Serializable]
    public class InvalidClaimStatusTransitionException : InvalidOperationException
    {
        /// <summary>Initializes a new instance of the <see cref="InvalidClaimStatusTransitionException"/> class.</summary>
        /// <param name="currentStatus">The current status of the claim.</param>
        /// <param name="newStatus">The attempted new status.</param>
        public InvalidClaimStatusTransitionException(ClaimStatus currentStatus, ClaimStatus newStatus)
            : base($"Cannot transition claim from {currentStatus} to {newStatus}.")
        {
            this.CurrentStatus = currentStatus;
            this.NewStatus = newStatus;
        }

        /// <summary>Gets the current status of the claim.</summary>
        public ClaimStatus CurrentStatus { get; }

        /// <summary>Gets the attempted new status.</summary>
        public ClaimStatus NewStatus { get; }
    }
}
