using System;

namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommand
    {
        string Category { get; set; }
        Type CommandType { get; set; }
        string Description { get; set; }
        string Key { get; set; }
        string Name { get; set; }
        string Version { get; set; }
    }
}
