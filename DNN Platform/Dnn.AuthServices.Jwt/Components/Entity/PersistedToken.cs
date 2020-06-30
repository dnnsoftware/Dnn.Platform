// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Components.Entity
{
    using System;

    [Serializable]
    public class PersistedToken
    {
        public string TokenId { get; set; }

        public int UserId { get; set; }

        public int RenewCount { get; set; }

        public DateTime TokenExpiry { get; set; }

        public DateTime RenewalExpiry { get; set; }

        public string TokenHash { get; set; }

        public string RenewalHash { get; set; }
    }
}
