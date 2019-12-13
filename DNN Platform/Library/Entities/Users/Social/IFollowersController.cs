namespace DotNetNuke.Entities.Users.Social
{
    public interface IFollowersController
    {
        void FollowUser(UserInfo targetUser);
        void FollowUser(UserInfo initiatingUser, UserInfo targetUser);

        void UnFollowUser(UserInfo targetUser);
    }
}
