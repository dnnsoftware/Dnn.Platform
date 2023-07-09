// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services.Dto
{
    public class RevokeDeleteApiTokenRequest
    {
        public int ApiTokenId { get; set; }

        public bool Delete { get; set; }
    }
}
