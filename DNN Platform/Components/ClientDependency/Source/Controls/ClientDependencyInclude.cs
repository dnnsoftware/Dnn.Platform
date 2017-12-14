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
            //*** DNN related change *** begin
            AddTag = true;
            Name = "";
            Version = "";
            //*** DNN related change *** end
        }

        protected ClientDependencyInclude(IClientDependencyFile file)
        {
            Priority = file.Priority;
            PathNameAlias = file.PathNameAlias;
            FilePath = file.FilePath;
            DependencyType = file.DependencyType;
            Group = file.Group;
            HtmlAttributes = new Dictionary<string, string>();
            //*** DNN related change *** begin
            AddTag = true;
            Name = "";
            Version = "";
            ForceVersion = false;
            //*** DNN related change *** end
        }

        public ClientDependencyType DependencyType { get; internal set; }

        public string FilePath { get; set; }
        public string PathNameAlias { get; set; }
        public int Priority { get; set; }
        public int Group { get; set; }

        //*** DNN related change *** begin
        public bool AddTag { get; set; }

        /// <summary>Name of the script (e.g. <c>jQuery</c>, <c>Bootstrap</c>, <c>Angular</c>, etc.</summary>
        public string Name { get; set; }

        /// <summary>Version of this resource if it is a named resources. Note this field is only used when <see cref="Name" /> is specified</summary>
        public string Version { get; set; }

        /// <summary>Force this version to be used. Meant for skin designers that wish to override choices made by module developers or the framework.</summary>
        public bool ForceVersion { get; set; }
        //*** DNN related change *** end

        /// <summary>
        /// This can be empty and will use default provider
        /// </summary>
        public string ForceProvider { get; set; }

        /// <summary>
        /// If the resources is an external resource then normally it will be rendered as it's own download unless
        /// this is set to true. In that case the system will download the external resource and include it in the local bundle.
        /// </summary>
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

        //*** DNN related change *** begin
        protected override void Render(HtmlTextWriter writer)
        {
            if (AddTag || this.Context.IsDebuggingEnabled)
            {
                writer.Write("<!--CDF({0}|{1})-->", DependencyType, FilePath);
            }

            base.Render(writer);
        }
        //*** DNN related change *** end

        protected bool Equals(ClientDependencyInclude other)
        {
            return string.Equals(FilePath, other.FilePath, StringComparison.InvariantCultureIgnoreCase) && DependencyType == other.DependencyType && Priority == other.Priority && Group == other.Group && string.Equals(PathNameAlias, other.PathNameAlias, StringComparison.InvariantCultureIgnoreCase) && string.Equals(ForceProvider, other.ForceProvider) && Equals(HtmlAttributes, other.HtmlAttributes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClientDependencyInclude)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (FilePath != null ? FilePath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)DependencyType;
                hashCode = (hashCode * 397) ^ Priority;
                hashCode = (hashCode * 397) ^ Group;
                hashCode = (hashCode * 397) ^ (PathNameAlias != null ? PathNameAlias.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ForceProvider != null ? ForceProvider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (HtmlAttributes != null ? HtmlAttributes.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
