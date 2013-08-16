using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core
{
	public class BasicFile : IClientDependencyFile, IHaveHtmlAttributes
	{
		public BasicFile(ClientDependencyType type)
		{
			DependencyType = type;
		    HtmlAttributes = new Dictionary<string, string>();
		}

		#region IClientDependencyFile Members

		public string FilePath { get; set; }
		public ClientDependencyType DependencyType { get; private set; }
		public int Priority { get; set; }
		public int Group { get; set; }
		public string PathNameAlias { get; set; }
		/// <summary>
		/// This can be empty and will use default provider
		/// </summary>
		public string ForceProvider { get; set; }
		public bool ForceBundle { get; set; }

        /// <summary>
        /// Used to store additional attributes in the HTML markup for the item
        /// </summary>
        /// <remarks>
        /// Mostly used for CSS Media, but could be for anything
        /// </remarks>
        public IDictionary<string, string> HtmlAttributes { get; private set; }

		#endregion
	}
}
