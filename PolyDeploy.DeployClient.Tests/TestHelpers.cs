using FakeItEasy;

namespace PolyDeploy.DeployClient.Tests;

public static class TestHelpers
{
    public static DeployInput CreateDeployInput(string? targetUri = null, string? apiKey = null, string? encryptionKey = null, int? installationStatusTimeout = null)
    {
        return new DeployInput(
            targetUri ?? "https://test.com",
            apiKey ?? A.Dummy<string>(),
            encryptionKey ?? A.Dummy<string>(),
            installationStatusTimeout ?? 5);
    }
}
