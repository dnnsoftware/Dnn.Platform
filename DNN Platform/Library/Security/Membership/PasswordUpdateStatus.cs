namespace DotNetNuke.Security.Membership
{
    public enum PasswordUpdateStatus
    {
        Success,
        PasswordMissing,
        PasswordNotDifferent,
        PasswordResetFailed,
        PasswordInvalid,
        PasswordMismatch,
        InvalidPasswordAnswer,
        InvalidPasswordQuestion,
        BannedPasswordUsed
    }
}
