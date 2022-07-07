using System.Collections.Generic;

namespace PolyDeploy.DeployClient
{
    public enum SessionStatus
    {
        NotStarted = 0,
        InProgess = 1,
        Complete = 2,
    }

    public class Session
    {
        public SessionStatus Status { get; init; }
        public SortedList<int, SessionResponse?>? Responses { get; set; }
    }

    public record SessionResponse
    {
        public string? Name { get; init; }
        public List<PackageResponse?>? Packages { get; init; }
        public List<string?>? Failures { get; init; }
        public bool Attempted { get; init; }
        public bool Success { get; init; }
        public bool CanInstall { get; init; }
    }

    public class PackageResponse
    {
        public string? Name { get; init; }
        public List<DependencyResponse?>? Dependencies { get; init; }
        public string? VersionStr { get; init; }
        public bool CanInstall { get; init; }
    }

    public class DependencyResponse
    {
        public bool IsPackageDependency { get; init; }
        public string? PackageName { get; init; }
        public string? DependencyVersion { get; init; }
    }
}