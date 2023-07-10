// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Services.Dto
{
    public class UpdateApiTokenSettingsRequest
    {
        public int MaximumTimespan { get; set; }

        public string MaximumTimespanMeasure { get; set; }

        public bool AllowApiTokens { get; set; }
    }
}
