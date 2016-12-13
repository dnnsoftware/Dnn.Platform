#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
#region Usings

using System.Linq;
using System.Web.UI;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Modules.Taxonomy.Views.Controls
{

    public partial class EditVocabularyControl : UserControl
    {

        #region Public Properties

        public bool IsAddMode { get; set; }

        public string LocalResourceFile { get; set; }

        #endregion

        public void BindVocabulary(Vocabulary vocabulary, bool editEnabled, bool showScope)
        {
            if (IsPostBack)
            {
                vocabulary.Name = nameTextBox.Text;
                vocabulary.Description = descriptionTextBox.Text;

                var scopeTypeController = new ScopeTypeController();
                ScopeType scopeType;
                scopeType = scopeTypeController.GetScopeTypes().Where(s => s.ScopeType == scopeList.SelectedValue).SingleOrDefault();
                vocabulary.ScopeTypeId = scopeType.ScopeTypeId;

                vocabulary.Type = typeList.SelectedValue == "Simple" ? VocabularyType.Simple : VocabularyType.Hierarchy;
            }
            else
            {
                nameTextBox.Text = vocabulary.Name;
                nameLabel.Text = vocabulary.Name;
                descriptionTextBox.Text = vocabulary.Description;
                typeList.Items.FindByValue(vocabulary.Type.ToString()).Selected = true;
                if (vocabulary.ScopeType != null)
                {
					scopeLabel.Text = Localization.GetString(vocabulary.ScopeType.ScopeType, LocalResourceFile);
                    scopeList.Items.FindByValue(vocabulary.ScopeType.ScopeType).Selected = true;
                }
                typeLabel.Text = vocabulary.Type.ToString();
            }

            nameTextBox.Visible = IsAddMode;
            nameLabel.Visible = !IsAddMode;
            descriptionTextBox.Enabled = editEnabled;
            divScope.Visible = (IsAddMode && showScope);
            scopeLabel.Visible = !(IsAddMode && showScope);
            typeList.Visible = IsAddMode;
            typeLabel.Visible = !IsAddMode;
        }

    }
}