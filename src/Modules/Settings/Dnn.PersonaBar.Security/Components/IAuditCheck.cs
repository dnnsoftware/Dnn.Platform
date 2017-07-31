namespace Dnn.PersonaBar.Security.Components
{
    public interface IAuditCheck
    {
        string Id { get; }

        CheckResult Execute();
    }
}