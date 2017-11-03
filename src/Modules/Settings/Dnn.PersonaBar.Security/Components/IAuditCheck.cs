namespace Dnn.PersonaBar.Security.Components
{
    public interface IAuditCheck
    {
        string Id { get; }

        bool LazyLoad { get; }

        CheckResult Execute();
    }
}