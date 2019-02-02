using System;
using System.IO;
using System.Web.UI;

namespace ClientDependency.Core.Controls
{
    public abstract class DependencyFolderInclude : Control
    {
        protected DependencyFolderInclude()
        {
            
        }

        protected DependencyFolderInclude(string folderVirtualPath)
        {            
            if (folderVirtualPath == null) throw new ArgumentNullException("folderVirtualPath");
            FolderVirtualPath = folderVirtualPath;
        }

        public string FolderVirtualPath { get; set; }
        public string ForceProvider { get; set; }
        public int Priority { get; set; }
        public int Group { get; set; }

        protected abstract ClientDependencyType DependencyType { get; }
        
        /// <summary>
        /// Gets/sets the search pattern for css files
        /// </summary>
        protected string FileSearchPattern { get; set; }

        /// <summary>
        /// Used to set the HtmlAttributes on this class via a string which is parsed
        /// </summary>
        /// <remarks>
        /// The syntax for the string must be: key1:value1,key2:value2   etc...
        /// </remarks>
        public string HtmlAttributesAsString { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //create CssInclude controls for each file found
            var systemRootPath = Context.Server.MapPath("~/");
            var folderMappedPath = Context.Server.MapPath(FolderVirtualPath);

            if (folderMappedPath.StartsWith(systemRootPath))
            {
                var files = Directory.GetFiles(folderMappedPath, FileSearchPattern, SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    var absoluteFilePath = "~/" + file.Substring(systemRootPath.Length).Replace("\\", "/");

                    Controls.Add(new CssInclude()
                        {
                            Priority = Priority,
                            FilePath = absoluteFilePath,
                            DependencyType = DependencyType,
                            ForceProvider = ForceProvider,
                            Group = Group,
                            HtmlAttributesAsString = HtmlAttributesAsString
                        });
                }
            }
        }
    }
}