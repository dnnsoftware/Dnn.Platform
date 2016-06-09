using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using DNNConnect.CKEditorProvider.Objects;
using DNNConnect.CKEditorProvider.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

namespace DNNConnect.CKEditorProvider.Browser
{

    /// <summary>
    /// The File Upload Handler
    /// </summary>
    public class FileUploader : IHttpHandler
    {
        /// <summary>
        /// The JavaScript Serializer
        /// </summary>
        private readonly JavaScriptSerializer js = new JavaScriptSerializer();

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [override files].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [override files]; otherwise, <c>false</c>.
        /// </value>
        private bool OverrideFiles
        {
            get
            {
                return HttpContext.Current.Request["overrideFiles"].Equals("1")
                       || HttpContext.Current.Request["overrideFiles"].Equals(
                           "true",
                           StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Gets the storage folder.
        /// </summary>
        /// <value>
        /// The storage folder.
        /// </value>
        private IFolderInfo StorageFolder
        {
            get
            {
                return FolderManager.Instance.GetFolder(Convert.ToInt32(HttpContext.Current.Request["storageFolderID"]));
            }
        }

        /*
        /// <summary>
        /// Gets the storage folder.
        /// </summary>
        /// <value>
        /// The storage folder.
        /// </value>
        private PortalSettings PortalSettings
        {
            get
            {
                return new PortalSettings(Convert.ToInt32(HttpContext.Current.Request["portalID"]));
            }
        }*/

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "private, no-cache");

            HandleMethod(context);
        }

        /// <summary>
        /// Returns the options.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void ReturnOptions(HttpContext context)
        {
            context.Response.AddHeader("Allow", "DELETE,GET,HEAD,POST,PUT,OPTIONS");
            context.Response.StatusCode = 200;
        }

        /// <summary>
        /// Handle request based on method
        /// </summary>
        /// <param name="context">The context.</param>
        private void HandleMethod(HttpContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "HEAD":
                case "GET":
                    /*if (GivenFilename(context))
                    {
                        this.DeliverFile(context);
                    }
                    else
                    {
                        ListCurrentFiles(context);
                    }*/

                    break;

                case "POST":
                case "PUT":
                    UploadFile(context);
                    break;

                case "OPTIONS":
                    ReturnOptions(context);
                    break;

                default:
                    context.Response.ClearHeaders();
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="context">The context.</param>
        private void UploadFile(HttpContext context)
        {
            var statuses = new List<FilesUploadStatus>();

            UploadWholeFile(context, statuses);

            WriteJsonIframeSafe(context, statuses);
        }

        /// <summary>
        /// Uploads the whole file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="statuses">The statuses.</param>
        private void UploadWholeFile(HttpContext context, List<FilesUploadStatus> statuses)
        {
            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];

                var fileName = Path.GetFileName(file.FileName);

                if (!string.IsNullOrEmpty(fileName))
                {
                    // Replace dots in the name with underscores (only one dot can be there... security issue).
                    fileName = Regex.Replace(fileName, @"\.(?![^.]*$)", "_", RegexOptions.None);

                    // Check for Illegal Chars
                    if (Utility.ValidateFileName(fileName))
                    {
                        fileName = Utility.CleanFileName(fileName);
                    }

                    // Convert Unicode Chars
                    fileName = Utility.ConvertUnicodeChars(fileName);
                }
                else
                {
                    throw new HttpRequestValidationException("File does not have a name");
                }

                if (fileName.Length > 220)
                {
                    fileName = fileName.Substring(fileName.Length - 220);
                }

                var fileNameNoExtenstion = Path.GetFileNameWithoutExtension(fileName);

                // Rename File if Exists
                if (!OverrideFiles)
                {
                    var counter = 0;

                    while (File.Exists(Path.Combine(StorageFolder.PhysicalPath, fileName)))
                    {
                        counter++;
                        fileName = string.Format(
                            "{0}_{1}{2}",
                            fileNameNoExtenstion,
                            counter,
                            Path.GetExtension(file.FileName));
                    }
                }

                var fileManager = FileManager.Instance;
                var contentType = fileManager.GetContentType(Path.GetExtension(fileName));
                var userId = UserController.Instance.GetCurrentUserInfo().UserID;
                fileManager.AddFile(StorageFolder, fileName, file.InputStream, OverrideFiles, true, contentType, userId);

                var fullName = Path.GetFileName(fileName);
                statuses.Add(new FilesUploadStatus(fullName, file.ContentLength));
            }
        }

        /// <summary>
        /// Writes the JSON iFrame safe.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="statuses">The statuses.</param>
        private void WriteJsonIframeSafe(HttpContext context, List<FilesUploadStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                context.Response.ContentType = context.Request["HTTP_ACCEPT"].Contains("application/json")
                                                   ? "application/json"
                                                   : "text/plain";
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }

            var jsonObj = js.Serialize(statuses.ToArray());
            context.Response.Write(jsonObj);
        }
    }
}