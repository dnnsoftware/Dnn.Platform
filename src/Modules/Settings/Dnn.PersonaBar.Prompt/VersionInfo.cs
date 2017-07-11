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
            VersionInfo vsnInfo = GetVersion(vsn);
            if (vsnInfo != null)
            {
                var _with1 = vsnInfo;
                Major = _with1.Major;
                Minor = _with1.Minor;
                Build = _with1.Build;
                Revision = _with1.Revision;
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

            Match matchVersionInfo = Regex.Match(vsn, "(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)");
            if (matchVersionInfo.Success)
            {
                vsnInfo = new VersionInfo();
                var _with2 = vsnInfo;
                _with2.Major = Convert.ToInt32(matchVersionInfo.Groups[1].Value);
                _with2.Minor = Convert.ToInt32(matchVersionInfo.Groups[2].Value);
                _with2.Revision = Convert.ToInt32(matchVersionInfo.Groups[3].Value);
                _with2.Build = Convert.ToInt32(matchVersionInfo.Groups[4].Value);
            }
            else
            {
                matchVersionInfo = Regex.Match(vsn, "(\\d+)\\.(\\d+)\\.(\\d+)");
                if (matchVersionInfo.Success)
                {
                    vsnInfo = new VersionInfo();
                    var _with3 = vsnInfo;
                    _with3.Major = Convert.ToInt32(matchVersionInfo.Groups[1].Value);
                    _with3.Minor = Convert.ToInt32(matchVersionInfo.Groups[2].Value);
                    _with3.Revision = Convert.ToInt32(matchVersionInfo.Groups[3].Value);
                    _with3.Build = 0;
                }
                else
                {
                    matchVersionInfo = Regex.Match(vsn, "(\\d+)\\.(\\d+)");
                    if (matchVersionInfo.Success)
                    {
                        vsnInfo = new VersionInfo();
                        var _with4 = vsnInfo;
                        _with4.Major = Convert.ToInt32(matchVersionInfo.Groups[1].Value);
                        _with4.Minor = Convert.ToInt32(matchVersionInfo.Groups[2].Value);
                        _with4.Revision = 0;
                        _with4.Build = 0;
                    }
                    else
                    {
                        matchVersionInfo = Regex.Match(vsn, "(\\d+)");
                        if (matchVersionInfo.Success)
                        {
                            vsnInfo = new VersionInfo();
                            var _with5 = vsnInfo;
                            _with5.Major = Convert.ToInt32(matchVersionInfo.Groups[1].Value);
                            _with5.Minor = 0;
                            _with5.Revision = 0;
                            _with5.Build = 0;
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

