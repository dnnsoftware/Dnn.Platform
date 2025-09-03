namespace DotNetNuke.Abstractions.ClientResources
{
  /// <summary>
  /// Provides standard default values for client resource priorities and groups.
  /// </summary>
  public static class StandardPriorities
  {
    /// <summary>
    /// If a priority is not set, the default will be 100.
    /// </summary>
    /// <remarks>
    /// This will generally mean that if a developer doesn't specify a priority it will come after all other dependencies that 
    /// have unless the priority is explicitly set above 100.
    /// </remarks>
    public const int DefaultPriority = 100;
  }
}
