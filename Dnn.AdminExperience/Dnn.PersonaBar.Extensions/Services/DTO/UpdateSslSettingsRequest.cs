// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Services.Dto;

public class UpdateSslSettingsRequest
{
    public int SSLSetup { get; set; }

    public bool SSLEnforced { get; set; }

    public string SSLURL { get; set; }

    public string STDURL { get; set; }

    public string SSLOffloadHeader { get; set; }
}
