using DotNetNuke.Entities.Users;

namespace DotNetNuke.Entities.Friends
{
    public interface IFriendshipEventHandlers
    {
        void FriendshipRequested(object sender, RelationshipEventArgs args);
        void FriendshipAccepted(object sender, RelationshipEventArgs args);
        void FriendshipDeleted(object sender, RelationshipEventArgs args);
    }
}
