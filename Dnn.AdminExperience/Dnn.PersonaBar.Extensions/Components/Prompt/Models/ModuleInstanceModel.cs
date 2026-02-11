// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Similar to ModuleInfoModel, however this one has information more specific to a module's
    /// implementation on a particular page (like the PaneName).
    /// </summary>
    public class ModuleInstanceModel
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int TabId;

        // command link
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        public string __ModuleId { get; set; }

        public int ModuleId { get; set; }

        public string Title { get; set; }

        public string Pane { get; set; }

        public string ModuleName { get; set; }

        // command link
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        public string __ModuleName { get; set; }

        public string FriendlyName { get; set; }

        public int ModuleDefId { get; set; }

        public int TabModuleId { get; set; }

        public bool IsDeleted { get; set; }

        public static ModuleInstanceModel FromDnnModuleInfo(DotNetNuke.Entities.Modules.ModuleInfo dnnModule)
        {
            var mim = new ModuleInstanceModel
            {
                ModuleId = dnnModule.ModuleID,
                Title = dnnModule.ModuleTitle,
                Pane = dnnModule.PaneName,
                FriendlyName = dnnModule.DesktopModule.FriendlyName,
                ModuleName = dnnModule.DesktopModule.ModuleName,
                TabModuleId = dnnModule.TabModuleID,
                ModuleDefId = dnnModule.ModuleDefID,
                IsDeleted = dnnModule.IsDeleted,
                TabId = dnnModule.TabID,
                __ModuleId = $"get-module {dnnModule.ModuleID}",
                __ModuleName = $"list-modules '{dnnModule.ModuleID}'",
            };

            return mim;
        }
    }
}
