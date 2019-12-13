using System;

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    public class DbFileInfo
    {
        public string FileType { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public decimal Megabytes => Convert.ToDecimal(Size/1024);

        public string FileName { get; set; }

        public string ShortFileName
        {
            get
            {
                if(FileName.IndexOf('\\') == FileName.LastIndexOf('\\'))
                {
                    return FileName;
                }

                return string.Format("{0}...{1}", FileName.Substring(0, FileName.IndexOf('\\') + 1), FileName.Substring(FileName.LastIndexOf('\\', FileName.LastIndexOf('\\') - 1)));
            }
        }
    }
}
