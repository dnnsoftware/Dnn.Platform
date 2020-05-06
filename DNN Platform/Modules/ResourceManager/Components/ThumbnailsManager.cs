// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Services.FileSystem;
using Dnn.Modules.ResourceManager.Components.Models;

namespace Dnn.Modules.ResourceManager.Components
{
    public class ThumbnailsManager : ServiceLocator<IThumbnailsManager, ThumbnailsManager>, IThumbnailsManager
    {
        private const int DefaultMaxWidth = 320;
        private const int DefaultMaxHeight = 240;
        private readonly IFileManager _fileManager;


        public ThumbnailsManager()
        {
            _fileManager = FileManager.Instance;
        }


        public string DefaultContentType => "image/png";

        public string DefaultThumbnailExtension => "png";

        private enum ThumbnailExtensions
        {
            JPEG,
            JPG,
            PNG,
            GIF
        }

        public bool ThumbnailAvailable(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToUpperInvariant();
            ThumbnailExtensions extension;
            ext = ext.StartsWith(".") ? ext.Substring(1) : ext;
            return Enum.TryParse(ext, out extension);
        }

        public string ThumbnailUrl(int moduleId, int fileId, int width, int height)
        {
            return ThumbnailUrl(moduleId, fileId, width, height, GetNewTimeStamp());
        }

        public string ThumbnailUrl(int moduleId, int fileId, int width, int height, string timestamp)
        {
            var tabId = PortalController.Instance.GetCurrentPortalSettings().ActiveTab.TabID;
            
            return
                $"{ServicesFramework.GetServiceFrameworkRoot()}API/ResourceManager/Items/ThumbnailDownLoad?fileId={fileId}&width={width}&height={height}&timestamp={timestamp}&moduleId={moduleId}&tabId={tabId}";
        }

        public string ThumbnailUrl(int moduleId, int fileId, int width, int height, int version)
        {
            var result = ThumbnailUrl(moduleId, fileId, width, height, GetNewTimeStamp());
            return result + "&version=" + version;
        }

        public ThumbnailContent GetThumbnailContentFromImageUrl(string imageUrl, int width, int height)
        {
            Image image = new Bitmap(imageUrl);
            var result = GetThumbnailContentFromImage(image, width, height);
            var indexOfSlash = imageUrl.LastIndexOf('/');
            var thumbnailName = (indexOfSlash != -1) ? imageUrl.Substring(indexOfSlash + 1) + "." + DefaultThumbnailExtension : imageUrl + "." + DefaultThumbnailExtension;
            result.ThumbnailName = thumbnailName;
            return result;
        }

        public ThumbnailContent GetThumbnailContentFromImage(Image image, int width, int height, bool crop = false)
        {
            int thumbnailWidth;
            int thumbnailHeight;

            if (crop)
            {
                GetCroppedThumbnailSize(image, width, height, out thumbnailWidth, out thumbnailHeight);
            }
            else
            {
                GetThumbnailSize(image, width, height, out thumbnailWidth, out thumbnailHeight);
            }

            // create the actual thumbnail image
            var thumbnailImage = new Bitmap(image, new Size(thumbnailWidth, thumbnailHeight));

            using (var memoryStream = new MemoryStream())
            {
                //All thumbnails images will be Png
                thumbnailImage.Save(memoryStream, ImageFormat.Png);

                return new ThumbnailContent
                {
                    Content = new ByteArrayContent(memoryStream.ToArray()),
                    Width = thumbnailWidth,
                    Height = thumbnailHeight,
                    ContentType = DefaultContentType
                };
            }
        }

        public ThumbnailContent GetThumbnailContent(IFileInfo item, int width, int height, bool crop=false)
        {
            using (var content = _fileManager.GetFileContent(item))
            {
                Image image = new Bitmap(content);
                var result = GetThumbnailContentFromImage(image, width, height, crop);
                result.ThumbnailName = item.FileName + "." + DefaultThumbnailExtension;
                return result;
            }
        }

        #region Private Methods

        //TODO: Correct below to proper US English
        private int GetMinorSizeValue(int thumbnailMayorSize, int imageMayorSize, int imageMinorSize)
        {
            if (thumbnailMayorSize == imageMayorSize)
            {
                return imageMinorSize;
            }

            return (int)Math.Round(imageMinorSize * (double)thumbnailMayorSize / imageMayorSize);
        }

        private int GetMayorSizeValue(int size, int imageSize, int defaultMaxValue)
        {
            if (size >= imageSize)
            {
                return imageSize;
            }

            var minimum = Math.Min(size, defaultMaxValue);
            return (minimum <= 0) ? defaultMaxValue : minimum;
        }

        private void GetThumbnailSize(Image image, int width, int height, out int thumbnailWidth, out int thumbnailHeight)
        {
            if (image.Width >= image.Height)
            {
                thumbnailWidth = GetMayorSizeValue(width, image.Width, DefaultMaxWidth);
                thumbnailHeight = GetMinorSizeValue(thumbnailWidth, image.Width, image.Height);
            }
            else
            {
                thumbnailHeight = GetMayorSizeValue(height, image.Height, DefaultMaxHeight);
                thumbnailWidth = GetMinorSizeValue(thumbnailHeight, image.Height, image.Width);
            }
        }

        private void GetCroppedThumbnailSize(Image image, int width, int height, out int thumbnailWidth, out int thumbnailHeight)
        {
            var aspect = (double)image.Width / image.Height;
            var thumbnailAspect = (double)width / height;

            if (aspect > thumbnailAspect)
            {
                thumbnailHeight = Math.Min(image.Height, height);
                thumbnailWidth = (int)Math.Round(image.Width * (double)thumbnailHeight / image.Height);
            }
            else
            {
                thumbnailWidth = Math.Min(image.Width, width);
                thumbnailHeight = (int)Math.Round(image.Height * (double)thumbnailWidth / image.Width);
            }
        }

        private string GetNewTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        #endregion

        #region Service Locator

        protected override Func<IThumbnailsManager> GetFactory()
        {
            return () => new ThumbnailsManager();
        }

        #endregion
    }
}