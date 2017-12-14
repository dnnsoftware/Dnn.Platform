using System;

namespace DotNetNuke.Services.ImprovementsProgram
{
    [Flags]
    internal enum RolesEnum
    {
        None = 0,
        Host = 1,
        Admin = 2,
        CommunityManager = 4,
        ContentManager = 8,
        ContentEditor = 16
    }
}