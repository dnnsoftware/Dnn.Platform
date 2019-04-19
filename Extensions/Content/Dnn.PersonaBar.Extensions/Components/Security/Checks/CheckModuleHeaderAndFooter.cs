using System;
using System.Web;
using DotNetNuke.Data;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckModuleHeaderAndFooter : IAuditCheck
    {
        public string Id => "CheckModuleHeaderAndFooter";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            try
            {
                var dr = DataProvider.Instance().ExecuteReader("SecurityAnalyzer_GetModulesHasHeaderFooter");
                result.Severity = SeverityEnum.Pass;
                while (dr.Read())
                {
                    result.Severity = SeverityEnum.Warning;
                    var note = string.Format("<b>TabId:</b> {0}, Module Id: {1}", dr["TabId"], dr["ModuleId"]);
                    var headerValue = dr["Header"].ToString();
                    var footerValue = dr["Footer"].ToString();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        note += string.Format("<br />Header: {0}", HttpUtility.HtmlEncode(headerValue));
                    }
                    if (!string.IsNullOrEmpty(footerValue))
                    {
                        note += string.Format("<br />Footer: {0}", HttpUtility.HtmlEncode(footerValue));
                    }
                    note += "< br />";

                    result.Notes.Add(note);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }
    }
}