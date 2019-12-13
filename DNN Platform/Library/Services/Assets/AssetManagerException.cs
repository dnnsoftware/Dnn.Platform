using System;

namespace DotNetNuke.Services.Assets
{
    public class AssetManagerException : Exception
    {
        public AssetManagerException(string message) : base(message)
        {
        }
    }
}
