namespace DotNetNuke.Services.Mail
{
    public enum MessageType
    {
        PasswordReminder,
        ProfileUpdated,
        UserRegistrationAdmin,
        UserRegistrationPrivate,
        UserRegistrationPrivateNoApprovalRequired,
        UserRegistrationPublic,
        UserRegistrationVerified,
        UserUpdatedOwnPassword,
        PasswordUpdated,
        PasswordReminderUserIsNotApproved,
        UserAuthorized,
        UserUnAuthorized
    }
}
