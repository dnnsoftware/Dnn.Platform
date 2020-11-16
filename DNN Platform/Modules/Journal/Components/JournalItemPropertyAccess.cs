// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using DotNetNuke.Services.Journal;

    public class JournalItemTokenReplace : Services.Tokens.BaseCustomTokenReplace
    {
        public JournalItemTokenReplace(JournalItem journalItem, JournalControl journalControl)
        {
            this.PropertySource["journalitem"] = journalItem;
            this.PropertySource["journalcontrol"] = journalControl;
            if (journalItem.ItemData != null)
            {
                this.PropertySource["journaldata"] = journalItem.ItemData;
            }

            if (journalItem.JournalAuthor != null)
            {
                this.PropertySource["journalauthor"] = journalItem.JournalAuthor;
                this.PropertySource["journalprofile"] = new ProfilePicPropertyAccess(journalItem.JournalAuthor.Id);
            }
        }

        public string ReplaceJournalItemTokens(string source)
        {
            return this.ReplaceTokens(source);
        }
    }
}
