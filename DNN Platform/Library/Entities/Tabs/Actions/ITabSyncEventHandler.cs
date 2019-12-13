namespace DotNetNuke.Entities.Tabs.Actions
{
    public interface ITabSyncEventHandler
    {
        void TabSerialize(object sender, TabSyncEventArgs args);
        void TabDeserialize(object sender, TabSyncEventArgs args);
    }
}
