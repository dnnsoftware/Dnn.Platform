namespace DotNetNuke.Entities.Users
{
    public interface IFollowerEventHandlers
    {
        void FollowRequested(object sender, RelationshipEventArgs args);
        void UnfollowRequested(object sender, RelationshipEventArgs args);
    }
}
