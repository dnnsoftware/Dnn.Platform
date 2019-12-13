namespace DotNetNuke.Services.FileSystem.Internal
{
    public interface IDirectory
    {
        void Delete(string path, bool recursive);
        bool Exists(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
        void Move(string sourceDirName, string destDirName);
        void CreateDirectory(string path);
    }
}
