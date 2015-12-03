using DotNetNuke.Services.Journal;

namespace DotNetNuke.Modules.Journal.Components {
    public class JournalItemTokenReplace : Services.Tokens.BaseCustomTokenReplace {
        public JournalItemTokenReplace(JournalItem journalItem, JournalControl journalControl) {
            PropertySource["journalitem"] = journalItem;
            PropertySource["journalcontrol"] = journalControl;
            if (journalItem.ItemData != null) {
                PropertySource["journaldata"] = journalItem.ItemData;
            }
            if (journalItem.JournalAuthor != null) {
                PropertySource["journalauthor"] = journalItem.JournalAuthor;
                PropertySource["journalprofile"] = new ProfilePicPropertyAccess(journalItem.JournalAuthor.Id);
            }
             
        }
        public string ReplaceJournalItemTokens(string source) {
            return ReplaceTokens(source);
        }
    }
}