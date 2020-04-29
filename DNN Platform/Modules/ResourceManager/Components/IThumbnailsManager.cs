using System.Drawing;
using DotNetNuke.Services.FileSystem;
using Dnn.Modules.ResourceManager.Components.Models;

namespace Dnn.Modules.ResourceManager.Components
{
    /// <summary>
    /// Manager for Thumbnails on Resource Manager
    /// </summary>
    public interface IThumbnailsManager
    {
        /// <summary>
        /// Return if a file is available to get a thumbnail
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>If the file is available to get a thumbnail or not</returns>
        bool ThumbnailAvailable(string fileName);

        /// <summary>
        /// Return the url of the thumbnail of a file
        /// </summary>
        /// <param name="moduleId">Id of the module</param>
        /// <param name="fileId">Id of the file</param>
        /// <param name="width">Width of the thumbnail</param>
        /// <param name="height">Height of the thumbnail</param>
        /// <returns>The url of the result thumbnail</returns>
        string ThumbnailUrl(int moduleId, int fileId, int width, int height);

        /// <summary>
        /// Return the url of the thumbnail of a file
        /// </summary>
        /// <param name="moduleId">Id of the module</param>
        /// <param name="fileId">Id of the file</param>
        /// <param name="width">Width of the thumbnail</param>
        /// <param name="height">Height of the thumbnail</param>
        /// <param name="timestamp">Timestamp</param>
        /// <returns>The url of the result thumbnail</returns>
        string ThumbnailUrl(int moduleId, int fileId, int width, int height, string timestamp);

        /// <summary>
        /// Return the url of the thumbnail of a file
        /// </summary>
        /// <param name="moduleId">Id of the module</param>
        /// <param name="fileId">Id of the file</param>
        /// <param name="width">Width of the thumbnail</param>
        /// <param name="height">Height of the thumbnail</param>
        /// <param name="version">Version of the file</param>
        /// <returns>The url of the result thumbnail</returns>
        string ThumbnailUrl(int moduleId, int fileId, int width, int height, int version);

        /// <summary>
        /// Get the thumbnail from an image
        /// </summary>
        /// <param name="imageUrl">Url of the image</param>
        /// <param name="width">Width of the thumbnail</param>
        /// <param name="height">Height of the thumbnail</param>
        /// <returns>The thumbnail of the image</returns>
        ThumbnailContent GetThumbnailContentFromImageUrl(string imageUrl, int width, int height);

        /// <summary>
        /// Get the thumbnail from an image
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="width">Width of the thumbnail</param>
        /// <param name="height">Height of the thumbnail</param>
        /// <param name="crop">Crop the image on the thumbnail</param>
        /// <returns>The thumbnail of the image</returns>
        ThumbnailContent GetThumbnailContentFromImage(Image image, int width, int height, bool crop = false);

        /// <summary>
        /// Get the thumbnail from a file
        /// </summary>
        /// <param name="item">File</param>
        /// <param name="width">Width of the thumbnail</param>
        /// <param name="height">Height of the thumbnail</param>
        /// <param name="crop">Crop the image on the thumbnail</param>
        /// <returns>The thumbnail of the file</returns>
        ThumbnailContent GetThumbnailContent(IFileInfo item, int width, int height, bool crop = false);
    }
}