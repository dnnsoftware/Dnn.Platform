namespace DotNetNuke.ComponentModel
{
    public interface IComponentBuilder
    {
        string Name { get; }

        object BuildComponent();
    }
}
