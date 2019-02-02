using System.Web;
using ClientDependency.Core;
using ClientDependency.Less;

[assembly: PreApplicationStartMethod(typeof(StartupHandler), "Initialize")]

namespace ClientDependency.Less
{
    /// <summary>
    /// Class called to register the less writer
    /// </summary>
    public static class StartupHandler
    {
        public static void Initialize()
        {
            //register the less writer.
            FileWriters.AddWriterForExtension(".less", new LessWriter());
        }
    }
}