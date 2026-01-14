// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    using DotNetNuke.Common.Utilities;

    /// <summary>A data transfer object with information about options for the item list services.</summary>
    [DataContract]
    public class ItemListServicesOptions
    {
        /// <summary>Gets or sets the module ID.</summary>
        [DataMember(Name = "moduleId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ModuleId = string.Empty;

        /// <summary>Gets or sets the service root.</summary>
        [DataMember(Name = "serviceRoot")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ServiceRoot;

        /// <summary>Gets or sets the get tree method.</summary>
        [DataMember(Name = "getTreeMethod")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string GetTreeMethod;

        /// <summary>Gets or sets the sort tree method.</summary>
        [DataMember(Name = "sortTreeMethod")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SortTreeMethod;

        /// <summary>Gets or sets the get node descendants method.</summary>
        [DataMember(Name = "getNodeDescendantsMethod")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string GetNodeDescendantsMethod;

        /// <summary>Gets or sets the search tree method.</summary>
        [DataMember(Name = "searchTreeMethod")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SearchTreeMethod;

        /// <summary>Gets or sets the get tree with node method.</summary>
        [DataMember(Name = "getTreeWithNodeMethod")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string GetTreeWithNodeMethod;

        /// <summary>Gets or sets the root ID.</summary>
        /// <remarks>Should not be <c>-1</c>, as <c>-1</c> can be treated as <see cref="Null.NullInteger"/>.</remarks>
        [DataMember(Name = "rootId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string RootId = "Root";

        private Dictionary<string, string> parameters;

        /// <summary>Gets the parameters.</summary>
        [DataMember(Name = "parameters")]
        public Dictionary<string, string> Parameters
        {
            get
            {
                return this.parameters ?? (this.parameters = new Dictionary<string, string>());
            }
        }
    }
}
