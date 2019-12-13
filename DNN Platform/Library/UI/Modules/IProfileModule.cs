namespace DotNetNuke.UI.Modules
{
    public interface IProfileModule
    {
        bool DisplayModule { get; }

        int ProfileUserId { get; }
    }
}
