namespace DotNetNuke.Services.FileSystem
{
    internal interface IIconController
    {
        string IconURL(string key);

        string IconURL(string key, string size);
    }
}
