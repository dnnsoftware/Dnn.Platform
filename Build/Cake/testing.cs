using Cake.Common.Tools.NUnit;
using Cake.Frosting;

[Dependency(typeof(Build))]
public sealed class RunUnitTests : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.NUnit3("./src/**/bin/" + context.configuration + "/*.Test*.dll", new NUnit3Settings {NoResults = false});
    }
}
