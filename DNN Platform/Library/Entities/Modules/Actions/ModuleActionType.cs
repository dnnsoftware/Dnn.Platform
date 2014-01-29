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
        public const string AddContent = "AddContent.Action";
        public const string EditContent = "EditContent.Action";
        public const string ContentOptions = "ContentOptions.Action";
        public const string SyndicateModule = "SyndicateModule.Action";
        public const string ImportModule = "ImportModule.Action";
        public const string ExportModule = "ExportModule.Action";
        public const string OnlineHelp = "OnlineHelp.Action";
        public const string ModuleHelp = "ModuleHelp.Action";
        public const string HelpText = "ModuleHelp.Text";
        public const string PrintModule = "PrintModule.Action";
        public const string ModuleSettings = "ModuleSettings.Action";
        public const string DeleteModule = "DeleteModule.Action";
        public const string ClearCache = "ClearCache.Action";
        public const string MoveTop = "MoveTop.Action";
        public const string MoveUp = "MoveUp.Action";
        public const string MoveDown = "MoveDown.Action";
        public const string MoveBottom = "MoveBottom.Action";
        public const string MovePane = "MovePane.Action";
        public const string MoveRoot = "MoveRoot.Action";
        public const string ViewSource = "ViewSource.Action";
        public const string LocalizeModule = "Localize.Action";
        public const string DeLocalizeModule = "DeLocalize.Action";
        public const string TranslateModule = "Translate.Action";
        public const string UnTranslateModule = "UnTranslate.Action";
    }
}
