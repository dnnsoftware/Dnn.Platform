namespace DotNetNuke.Entities.Tabs.TabVersions
{
    /// <summary>
    /// This enum represents the possible list of action that can be done in a Tab Version (i.e.: add module, modified module, deleted module, reset (restore version))
    /// </summary>
    public enum TabVersionDetailAction
    {
        Added,
        Modified,
        Deleted,
        Reset
    }
}
