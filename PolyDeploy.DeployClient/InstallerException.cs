namespace PolyDeploy.DeployClient;

using System;
public class InstallerException : Exception
{
    public InstallerException(string message, Exception innerException)
        : base(message, innerException)
    {}
}