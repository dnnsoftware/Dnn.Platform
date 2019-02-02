using System.Web;
using ClientDependency.Coffee;
using ClientDependency.Core;

[assembly: PreApplicationStartMethod(typeof(StartupHandler), "Initialize")]

namespace ClientDependency.Coffee
{
    /// <summary>
    /// Class called to register the less writer
    /// </summary>
    public static class StartupHandler
    {
        public static void Initialize()
        {
            //register the less writer.
            FileWriters.AddWriterForExtension(".coffee", new CoffeeWriter());
        }
    }
}