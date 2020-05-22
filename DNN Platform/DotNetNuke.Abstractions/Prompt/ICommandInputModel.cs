namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommandInputModel
    {
        string[] Args { get; }
        string CmdLine { get; set; }
        int CurrentPage { get; set; }
    }
}
