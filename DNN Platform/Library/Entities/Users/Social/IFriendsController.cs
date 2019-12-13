namespace DotNetNuke.Entities.Users.Social
{
    public interface IFriendsController
    {
        void AcceptFriend(UserInfo targetUser);

        void AddFriend(UserInfo targetUser);
        void AddFriend(UserInfo initiatingUser, UserInfo targetUser);

        void DeleteFriend(UserInfo targetUser);
        void DeleteFriend(UserInfo initiatingUser, UserInfo targetUser);

    }
}
