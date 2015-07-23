#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
namespace DotNetNuke.Entities.Modules.Actions
{
    ///-----------------------------------------------------------------------------
    /// Project		: DotNetNuke
    /// Class		: ModuleActionType
    ///-----------------------------------------------------------------------------
    /// <summary>
    /// Identifies common module action types
    /// </summary>
    /// <remarks>
    /// Common action types can be specified in the CommandName attribute of the
    /// ModuleAction class
    /// </remarks>
    ///-----------------------------------------------------------------------------
    public class ModuleActionType
    {
        /// <summary>An action to create new content</summary>
        public const string AddContent = "AddContent.Action";
        
        /// <summary>An action to edit existing content</summary>
        public const string EditContent = "EditContent.Action";
        
        /// <summary>An action to configure the module</summary>
        public const string ContentOptions = "ContentOptions.Action";
        
        /// <summary>An action to access the RSS feed of a module</summary>
        public const string SyndicateModule = "SyndicateModule.Action";
        
        /// <summary>An action to import content into a module</summary>
        public const string ImportModule = "ImportModule.Action";
        
        /// <summary>An action to export the content of a module</summary>
        public const string ExportModule = "ExportModule.Action";
        
        public const string OnlineHelp = "OnlineHelp.Action";
        
        /// <summary>An action to view the help for a module</summary>
        public const string ModuleHelp = "ModuleHelp.Action";
        
        public const string HelpText = "ModuleHelp.Text";
        
        /// <summary>An action to print a module's content</summary>
        public const string PrintModule = "PrintModule.Action";
        
        /// <summary>An action to access the module's settings</summary>
        public const string ModuleSettings = "ModuleSettings.Action";
        
        /// <summary>An action to delete the module</summary>
        public const string DeleteModule = "DeleteModule.Action";
        
        /// <summary>An action to clear the module's cache</summary>
        public const string ClearCache = "ClearCache.Action";
        
        /// <summary>An action to move the module to the top of its pane</summary>
        public const string MoveTop = "MoveTop.Action";
        
        /// <summary>An action to move the module up in its pane</summary>
        public const string MoveUp = "MoveUp.Action";
        
        /// <summary>An action to move the module down in its pane</summary>
        public const string MoveDown = "MoveDown.Action";
        
        /// <summary>An action to move the module to the bottom of its pane</summary>
        public const string MoveBottom = "MoveBottom.Action";
        
        /// <summary>An action to move the module to a different pane</summary>
        public const string MovePane = "MovePane.Action";
        
        /// <summary>An action that contains move actions</summary>
        public const string MoveRoot = "MoveRoot.Action";
        
        /// <summary>An action to view the source code of a module</summary>
        public const string ViewSource = "ViewSource.Action";
        
        /// <summary>An action to create a localized version of a module</summary>
        public const string LocalizeModule = "Localize.Action";
        
        /// <summary>An action to remove localization for a module</summary>
        public const string DeLocalizeModule = "DeLocalize.Action";
        
        /// <summary>An action to create a translated version of a module</summary>
        public const string TranslateModule = "Translate.Action";
        
        /// <summary>An action to remove a translation of a module</summary>
        public const string UnTranslateModule = "UnTranslate.Action";
    }
}
