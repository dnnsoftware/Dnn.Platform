using System;

namespace Dnn.Modules.ResourceManager.Exceptions
{
    public class FolderPermissionNotMetException : Exception
    {
        public FolderPermissionNotMetException(string message) : base(message)
        {

        }
    }
}