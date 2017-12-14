using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ClientDependency.Core
{
    /// <summary>
    /// Represents a dependency file
    /// </summary>
    [DebuggerDisplay("Type: {DependencyType}, File: {FilePath}")]
	public class BasicFile : IClientDependencyFile, IHaveHtmlAttributes
	{
		public BasicFile(ClientDependencyType type)
		{
			DependencyType = type;
		    HtmlAttributes = new Dictionary<string, string>();
		    Priority = Constants.DefaultPriority;
		    Group = Constants.DefaultGroup;
            //*** DNN related change *** begin
            Name = "";
            Version = "";
            ForceVersion = false;
            //*** DNN related change *** end
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

        //*** DNN related change *** begin
        /// <summary>
        /// Name of a framework such as jQuery, Bootstrap, Angular, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Version of this resource if it is a framework
        /// Note this field is only used when Framework is specified
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Force this version to be used. Meant for skin designers that wish to override
        /// choices made by module developers or the framework.
        /// </summary>
        public bool ForceVersion { get; set; }
        //*** DNN related change *** end

        #endregion

        protected bool Equals(BasicFile other)
        {
            return string.Equals(FilePath, other.FilePath, StringComparison.InvariantCultureIgnoreCase) && DependencyType == other.DependencyType && Priority == other.Priority && Group == other.Group && string.Equals(PathNameAlias, other.PathNameAlias, StringComparison.InvariantCultureIgnoreCase) && string.Equals(ForceProvider, other.ForceProvider) && Equals(HtmlAttributes, other.HtmlAttributes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BasicFile) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (FilePath != null ? FilePath.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) DependencyType;
                hashCode = (hashCode*397) ^ Priority;
                hashCode = (hashCode*397) ^ Group;
                hashCode = (hashCode*397) ^ (PathNameAlias != null ? PathNameAlias.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ForceProvider != null ? ForceProvider.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (HtmlAttributes != null ? HtmlAttributes.GetHashCode() : 0);
                return hashCode;
            }
        }
	}
}
