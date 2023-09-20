//HintName: DnnDeprecatedAttribute.g.cs
namespace DotNetNuke.Internal.SourceGenerators;

using System;
using System.Diagnostics;

/// <summary>Marks a type or member as deprecated.</summary>
[Conditional("DNN_SOURCE_GENERATOR")]
internal class DnnDeprecatedAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="DnnDeprecatedAttribute"/> class.</summary>
    /// <param name="majorVersion">The major version in which the type or member was deprecated.</param>
    /// <param name="minorVersion">The minor version in which the type or member was deprecated.</param>
    /// <param name="patchVersion">The patch version in which the type or member was deprecated.</param>
    /// <param name="replacement">The suggested replacement or alternative.</param>
    public DnnDeprecatedAttribute(int majorVersion, int minorVersion, int patchVersion, string replacement)
    {
        this.MajorVersion = majorVersion;
        this.MinorVersion = minorVersion;
        this.PatchVersion = patchVersion;
        this.Replacement = replacement;
        this.RemovalVersion = majorVersion + 2;
    }

    /// <summary>Gets the major version in which the type or member was deprecated.</summary>
    public int MajorVersion { get; }

    /// <summary>Gets the minor version in which the type or member was deprecated.</summary>
    public int MinorVersion { get; }

    /// <summary>Gets the patch version in which the type or member was deprecated.</summary>
    public int PatchVersion { get; }

    /// <summary>Gets the suggested replacement or alternative.</summary>
    public string Replacement { get; }

    /// <summary>Gets or sets the major version in which the type or member will be removed.</summary>
    public int RemovalVersion { get; set; }
}