namespace DotNetNuke.Security.Membership
{
    public enum UserRegistrationStatus
    {
        AddUser = 0,
        AddUserRoles = -1,
        UsernameAlreadyExists = -2,
        UserAlreadyRegistered = -3,
        UnexpectedError = -4
    }
}
