﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ClientDependency.Core
{
    /// <summary>
	/// This attribute is used for data types that uses client assets like Javascript and CSS for liveediting.
	/// The Live Editing feature in umbraco will look for this attribute and preload all dependencies to the page
	/// to ensure that all client events and assets gets loaded
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ClientDependencyAttribute : Attribute, IClientDependencyFile, IRequiresHtmlAttributesParsing
	{
        public ClientDependencyAttribute()
        {
            Priority = Constants.DefaultPriority;
            Group = Constants.DefaultGroup;
			PathNameAlias = "";
            HtmlAttributes = new Dictionary<string, string>();
            ForceVersion = false;
        }

        public ClientDependencyAttribute(ClientDependencyType dependencyType, string fullFilePath)
            : this(Constants.DefaultPriority, dependencyType, fullFilePath, string.Empty)
		{ }

		public ClientDependencyAttribute(ClientDependencyType dependencyType, string fileName, string pathNameAlias)
            : this(Constants.DefaultPriority, dependencyType, fileName, pathNameAlias)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
		/// </summary>
		/// <param name="priority">The priority.</param>
		/// <param name="dependencyType">Type of the dependency.</param>
		/// <param name="fullFilePath">The file path to the dependency.</param>
		public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string fullFilePath)
			: this(priority, dependencyType, fullFilePath, string.Empty)
		{ }


	    /// <summary>
	    /// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
	    /// </summary>
	    /// <param name="priority">The priority.</param>
	    /// <param name="dependencyType">Type of the dependency.</param>
	    /// <param name="fileName"></param>
	    /// <param name="pathNameAlias"></param>
	    public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string fileName, string pathNameAlias)
		    : this(Constants.DefaultGroup, priority, dependencyType, fileName, pathNameAlias, false)
        {
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="priority">The priority.</param>
        /// <param name="dependencyType">Type of the dependency.</param>
        /// <param name="fileName"></param>
        /// <param name="pathNameAlias"></param>
        public ClientDependencyAttribute(int group, int priority, ClientDependencyType dependencyType, string fileName, string pathNameAlias, bool forceBundle)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            Priority = priority;

            Group = group;

            FilePath = fileName;
            PathNameAlias = pathNameAlias;

            DependencyType = dependencyType;
			ForceBundle = forceBundle;

            HtmlAttributes = new Dictionary<string, string>();

            Name = "";
            Version = "";
            ForceVersion = false;
        }


        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>The group.</value>
        public int Group { get; set; }

        /// <summary>
        /// This can be empty and will use default provider
        /// </summary>
        public string ForceProvider { get; set; }

        public bool ForceBundle { get; set; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; set; }

        /// <summary>
        /// The path alias to be pre-pended to the file path if specified.
        /// The alias is specified in in the ClientDependencyHelper constructor.
        /// If the alias specified does not exist in the ClientDependencyHelper
        /// path collection, an exception is thrown.
        /// </summary>
        public string PathNameAlias { get; set; }

        /// <summary>
        /// Used to set the HtmlAttributes on this class via a string which is parsed
        /// </summary>
        /// <remarks>
        /// The syntax for the string must be: key1:value1,key2:value2   etc...
        /// </remarks>
        public string HtmlAttributesAsString { get; set; }

        /// <summary>
        /// Used to store additional attributes in the HTML markup for the item
        /// </summary>
        /// <remarks>
        /// Mostly used for CSS Media, but could be for anything
        /// </remarks>
        public IDictionary<string, string> HtmlAttributes { get; private set; }

        /// <summary>
        /// Gets or sets the type of the dependency.
        /// </summary>
        /// <value>The type of the dependency.</value>
        public ClientDependencyType DependencyType { get; set; }

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

        protected bool Equals(ClientDependencyAttribute other)
        {
            return string.Equals(FilePath, other.FilePath, StringComparison.InvariantCultureIgnoreCase) && DependencyType == other.DependencyType && Priority == other.Priority && Group == other.Group && string.Equals(PathNameAlias, other.PathNameAlias, StringComparison.InvariantCultureIgnoreCase) && string.Equals(ForceProvider, other.ForceProvider) && Equals(HtmlAttributes, other.HtmlAttributes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClientDependencyAttribute)obj);
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
