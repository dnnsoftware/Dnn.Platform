using System.Web;
using ClientDependency.Core;
using ClientDependency.SASS;

[assembly: PreApplicationStartMethod(typeof(StartupHandler), "Initialize")]

namespace ClientDependency.SASS
{
    /// <summary>
    /// Class called to register the less writer
    /// </summary>
    public static class StartupHandler
    {
        public static void Initialize()
        {
            //register the less writer.
            FileWriters.AddWriterForExtension(".sass", new SassWriter());
            FileWriters.AddWriterForExtension(".scss", new SassWriter());
        }
    }
}