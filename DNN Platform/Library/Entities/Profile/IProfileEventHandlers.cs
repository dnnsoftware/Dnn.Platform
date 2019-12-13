namespace DotNetNuke.Entities.Profile
{
    public interface IProfileEventHandlers
    {
        void ProfileUpdated(object sender, ProfileEventArgs args);
    }
}
