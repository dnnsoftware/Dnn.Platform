using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Services.Localization;
using Assembly = System.Reflection.Assembly;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckSqlRisk : IAuditCheck
    {
        public string Id => "CheckSqlRisk";

        public bool LazyLoad => false;

        private string LocalResourceFile
        {
            get { return "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Security/App_LocalResources/Security.resx"; }
        }

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            IList<string> checkList = new List<string>()
            {
                "SysAdmin",
                "ExecuteCommand",
                "GetFolderTree",
                "CheckFileExists",
                "RegRead"
            };

            result.Severity = SeverityEnum.Pass;
            foreach (var name in checkList)
            {
                if (!VerifyScript(name))
                {
                    result.Severity = SeverityEnum.Warning;
                    result.Notes.Add(Localization.GetString(name + ".Error", LocalResourceFile));
                }
            }
            return result;
        }

        private static bool VerifyScript(string name)
        {
            try
            {
                var script = LoadScript(name);
                if (!string.IsNullOrEmpty(script))
                {
                    if (name == "ExecuteCommand")
                    {
                        //since sql error is expected here, do not go through DataProvider so that no error will be logged
                        using (var connection = new SqlConnection(DataProvider.Instance().ConnectionString))
                        {
                            try
                            {
                                connection.Open();
                                var command = new SqlCommand(script, connection) {CommandType = CommandType.Text};
                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        int affectCount;
                                        int.TryParse(reader[0].ToString(), out affectCount);
                                        return affectCount == 0;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                //ignore;
                            }
                        }
                    }
                    else
                    {
                        using (var reader = DataProvider.Instance().ExecuteSQL(script))
                        {
                            if (reader != null && reader.Read())
                            {
                                int affectCount;
                                int.TryParse(reader[0].ToString(), out affectCount);
                                return affectCount == 0;
                            }
                        }
                    }
                }
            }
            catch (SqlException)
            {
                //ignore; return no failure
            }
            return true;
        }

        public static string LoadScript(string name)
        {
            var resourceName = string.Format("Dnn.PersonaBar.Security.Components.Resources.{0}.resources", name);
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var script = new StreamReader(stream).ReadToEnd();
                    return script.Replace("%SiteRoot%", Globals.ApplicationMapPath);
                }

                return null;
            }
        }
    }
}