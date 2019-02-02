using System.Web;
using ClientDependency.Core;
using ClientDependency.TypeScript;

[assembly: PreApplicationStartMethod(typeof(StartupHandler), "Initialize")]

namespace ClientDependency.TypeScript
{
    /// <summary>
    /// Class called to register the less writer
    /// </summary>
    public static class StartupHandler
    {
        public static void Initialize()
        {
            //register the less writer.
            FileWriters.AddWriterForExtension(".ts", new TypeScriptWriter());
        }
    }
}