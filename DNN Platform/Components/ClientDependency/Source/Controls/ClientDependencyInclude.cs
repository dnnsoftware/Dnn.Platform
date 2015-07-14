using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace ClientDependency.Core.Controls
{
    public abstract class ClientDependencyInclude : Control, IClientDependencyFile, IRequiresHtmlAttributesParsing
	{
        protected ClientDependencyInclude()
		{
            Priority = Constants.DefaultPriority;
            Group = Constants.DefaultGroup;
			PathNameAlias = "";
            HtmlAttributes = new Dictionary<string, string>();
	        AddTag = true;
		}

        protected ClientDependencyInclude(IClientDependencyFile file)
		{
			Priority = file.Priority;
			PathNameAlias = file.PathNameAlias;
			FilePath = file.FilePath;
			DependencyType = file.DependencyType;
            Group = file.Group;
			AddTag = true;
            HtmlAttributes = new Dictionary<string, string>();
		}
        
		public ClientDependencyType DependencyType { get; internal set; }

		public string FilePath { get; set; }
        public string PathNameAlias { get; set; }
        public int Priority { get; set; }
        public int Group { get; set; }
		public bool AddTag { get; set; }

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

        /// <summary>
        /// Used to set the HtmlAttributes on this class via a string which is parsed
        /// </summary>
        /// <remarks>
        /// The syntax for the string must be: key1:value1,key2:value2   etc...
        /// </remarks>
        public string HtmlAttributesAsString { get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (string.IsNullOrEmpty(FilePath))
				throw new NullReferenceException("Both File and Type properties must be set");
		}

		protected override void Render(HtmlTextWriter writer)
		{
			if (AddTag)
			{
				writer.Write("<!--CDF({0}|{1})-->", DependencyType, FilePath);
			}

			base.Render(writer);
		}
	}
}
