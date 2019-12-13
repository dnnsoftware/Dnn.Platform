#region Usings

using System;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
	/// <summary>
	/// Low-level APIs to manage Lucene Layer. This is an Internal class and should not be used outside of Core
	/// </summary>
    internal class LuceneController : ServiceLocator<ILuceneController, LuceneController>
    {
        protected override Func<ILuceneController> GetFactory()
        {
            return () => new LuceneControllerImpl();
        }
    }
}
