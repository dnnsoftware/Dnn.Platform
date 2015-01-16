using System;
using System.Collections.Generic;
using System.Linq;

namespace ClientDependency.Core
{
    /// <summary>
    /// Allows for globally specifying paths for path aliases when registering dependencies
    /// </summary>
    public static class PathsCollection
    {
        private static readonly List<BasicPath> Paths = new List<BasicPath>();
        private static readonly object Locker = new object();
        
        public static void AddPath(string name, string path, bool forceBundle = false)
        {
            lock (Locker)
            {
                var bp = new BasicPath(name, path) {ForceBundle = forceBundle};
                if (!Paths.Contains(bp))
                {
                    Paths.Add(bp);
                }
            }            
        }    
    
        internal static IEnumerable<IClientDependencyPath> GetPaths()
        {
            return Paths.ToArray();
        } 

    }
}