// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    using DotNetNuke.Entities.Users;

    public class UserBasicDto2 : UserBasicDto
    {
        public UserBasicDto2()
        {
        }

        public UserBasicDto2(UserInfo user) : base(user)
        {
        }

        public int TotalCount { get; set; }
    }
}
