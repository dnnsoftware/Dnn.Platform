namespace DotNetNuke.Entities.Users
{
    public interface IUserEventHandlers
    {
        void UserAuthenticated(object sender, UserEventArgs args);

        void UserCreated(object sender, UserEventArgs args);

        void UserDeleted(object sender, UserEventArgs args);

        void UserRemoved(object sender, UserEventArgs args);

        void UserApproved(object sender, UserEventArgs args);

        void UserUpdated(object sender, UpdateUserEventArgs args);
    }
}
