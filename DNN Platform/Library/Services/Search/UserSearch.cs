// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using System;

internal class UserSearch
{
    public int UserId { get; set; }

    public string DisplayName { get; set; }

    public string FirstName { get; set; }

    public string Email { get; set; }

    public string UserName { get; set; }

    public bool SuperUser { get; set; }

    public DateTime LastModifiedOnDate { get; set; }

    public DateTime CreatedOnDate { get; set; }
}
