using System;
using System.Text.RegularExpressions;

namespace Dnn.PersonaBar.Prompt
{
    /// <summary>
    /// Used to describe version data for an object
    /// </summary>
    /// <remarks></remarks>
    public class VersionInfo
    {
        public int Major;
        public int Minor;
        public int Build;

        public int Revision;
        public VersionInfo()
        {
            Major = 0;
            Minor = 0;
            Build = 0;
            Revision = 0;
        }

        public VersionInfo(string vsn)
        {
            var vsnInfo = GetVersion(vsn);
            if (vsnInfo != null)
            {
                var with1 = vsnInfo;
                Major = with1.Major;
                Minor = with1.Minor;
                Build = with1.Build;
                Revision = with1.Revision;
            }
            else
            {
                Major = 0;
                Minor = 0;
                Build = 0;
                Revision = 0;
            }
        }

        public static VersionInfo GetVersion(string vsn)
        {
            if (vsn == null || vsn.Length == 0)
                return null;
            VersionInfo vsnInfo = null;

            var matchVersionInfo = Regex.Match(vsn, "(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)");
            if (matchVersionInfo.Success)
            {
                vsnInfo = new VersionInfo();
                var with2 = vsnInfo;
                with2.Major = Convert.ToInt32(matchVersionInfo.Groups[1].Value);
                with2.Minor = Convert.ToInt32(matchVersionInfo.Groups[2].Value);
                with2.Revision = Convert.ToInt32(matchVersionInfo.Groups[3].Value);
                with2.Build = Convert.ToInt32(matchVersionInfo.Groups[4].Value);
            }
            else
            {
                matchVersionInfo = Regex.Match(vsn, "(\\d+)\\.(\\d+)\\.(\\d+)");
                if (matchVersionInfo.Success)
                {
                    vsnInfo = new VersionInfo();
                    var with3 = vsnInfo;
                    with3.Major = Convert.ToInt32(matchVersionInfo.Groups[1].Value);
                    with3.Minor = Convert.ToInt32(matchVersionInfo.Groups[2].Value);
                    with3.Revision = Convert.ToInt32(matchVersionInfo.Groups[3].Value);
                    with3.Build = 0;
                }
                else
                {
                    matchVersionInfo = Regex.Match(vsn, "(\\d+)\\.(\\d+)");
                    if (matchVersionInfo.Success)
                    {
                        vsnInfo = new VersionInfo();
                        var with4 = vsnInfo;
                        with4.Major = Convert.ToInt32(matchVersionInfo.Groups[1].Value);
                        with4.Minor = Convert.ToInt32(matchVersionInfo.Groups[2].Value);
                        with4.Revision = 0;
                        with4.Build = 0;
                    }
                    else
                    {
                        matchVersionInfo = Regex.Match(vsn, "(\\d+)");
                        if (matchVersionInfo.Success)
                        {
                            vsnInfo = new VersionInfo();
                            var with5 = vsnInfo;
                            with5.Major = Convert.ToInt32(matchVersionInfo.Groups[1].Value);
                            with5.Minor = 0;
                            with5.Revision = 0;
                            with5.Build = 0;
                        }
                    }
                }
            }
            return vsnInfo;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}{3}", Major, Minor, Build, (Revision > 0 ? "." + Revision : string.Empty));
        }
    }
}

