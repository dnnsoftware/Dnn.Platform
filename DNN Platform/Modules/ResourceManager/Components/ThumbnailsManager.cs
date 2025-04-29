// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;

using Dnn.Modules.ResourceManager.Components.Models;

using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Services.FileSystem;

/// <summary>Provides services related to thumbnails.</summary>
public class ThumbnailsManager : ServiceLocator<IThumbnailsManager, ThumbnailsManager>, IThumbnailsManager
{
    private const int DefaultMaxWidth = 320;
    private const int DefaultMaxHeight = 240;
    private readonly IFileManager fileManager;

    /// <summary>Initializes a new instance of the <see cref="ThumbnailsManager"/> class.</summary>
    public ThumbnailsManager()
    {
        this.fileManager = FileManager.Instance;
    }

    /// <summary>Enumerates the possible thumbnail file name extensions.</summary>
    private enum ThumbnailExtensions
    {
        /// <summary>A JPEG thumbnail.</summary>
        JPEG = 0,

        /// <summary>A JPG thumbnail.</summary>
        JPG = 1,

        /// <summary>A PNG thumbnail.</summary>
        PNG = 2,

        /// <summary>A GIF thumbnail.</summary>
        GIF = 3,
    }

    /// <summary>Gets the default content mime type.</summary>
    public string DefaultContentType => "image/png";

    /// <summary>Gets the default file name extension.</summary>
    public string DefaultThumbnailExtension => "png";

    /// <inheritdoc/>
    public bool ThumbnailAvailable(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToUpperInvariant();
        ext = ext.StartsWith(".") ? ext.Substring(1) : ext;
        return Enum.TryParse(ext, out ThumbnailExtensions _);
    }

    /// <inheritdoc/>
    public string ThumbnailUrl(int moduleId, int fileId, int width, int height)
    {
        return this.ThumbnailUrl(moduleId, fileId, width, height, this.GetNewTimeStamp());
    }

    /// <inheritdoc/>
    public string ThumbnailUrl(int moduleId, int fileId, int width, int height, string timestamp)
    {
        var tabId = TabController.CurrentPage.TabID;

        return
            $"{ServicesFramework.GetServiceFrameworkRoot()}API/ResourceManager/Items/ThumbnailDownLoad?fileId={fileId}&width={width}&height={height}&timestamp={timestamp}&moduleId={moduleId}&tabId={tabId}";
    }

    /// <inheritdoc/>
    public string ThumbnailUrl(int moduleId, int fileId, int width, int height, int version)
    {
        var result = this.ThumbnailUrl(moduleId, fileId, width, height, this.GetNewTimeStamp());
        return result + "&version=" + version;
    }

    /// <inheritdoc/>
    public ThumbnailContent GetThumbnailContentFromImageUrl(string imageUrl, int width, int height)
    {
        Image image = new Bitmap(imageUrl);
        var result = this.GetThumbnailContentFromImage(image, width, height);
        var indexOfSlash = imageUrl.LastIndexOf('/');
        var thumbnailName = (indexOfSlash != -1) ? imageUrl.Substring(indexOfSlash + 1) + "." + this.DefaultThumbnailExtension : imageUrl + "." + this.DefaultThumbnailExtension;
        result.ThumbnailName = thumbnailName;
        return result;
    }

    /// <inheritdoc/>
    public ThumbnailContent GetThumbnailContentFromImage(Image image, int width, int height, bool crop = false)
    {
        int thumbnailWidth;
        int thumbnailHeight;

        if (crop)
        {
            this.GetCroppedThumbnailSize(image, width, height, out thumbnailWidth, out thumbnailHeight);
        }
        else
        {
            this.GetThumbnailSize(image, width, height, out thumbnailWidth, out thumbnailHeight);
        }

        // create the actual thumbnail image
        var thumbnailImage = new Bitmap(image, new Size(thumbnailWidth, thumbnailHeight));

        using (var memoryStream = new MemoryStream())
        {
            // All thumbnails images will be png
            thumbnailImage.Save(memoryStream, ImageFormat.Png);

            return new ThumbnailContent
            {
                Content = new ByteArrayContent(memoryStream.ToArray()),
                Width = thumbnailWidth,
                Height = thumbnailHeight,
                ContentType = this.DefaultContentType,
            };
        }
    }

    /// <inheritdoc/>
    public ThumbnailContent GetThumbnailContent(IFileInfo item, int width, int height, bool crop = false)
    {
        using (var content = this.fileManager.GetFileContent(item))
        {
            Image image = new Bitmap(content);
            var result = this.GetThumbnailContentFromImage(image, width, height, crop);
            result.ThumbnailName = item.FileName + "." + this.DefaultThumbnailExtension;
            return result;
        }
    }

    /// <inheritdoc/>
    protected override Func<IThumbnailsManager> GetFactory()
    {
        return () => new ThumbnailsManager();
    }

    private int GetMinSizeValue(int thumbnailMaxSize, int imageMaxSize, int imageMinSize)
    {
        if (thumbnailMaxSize == imageMaxSize)
        {
            return imageMinSize;
        }

        return (int)Math.Round(imageMinSize * (double)thumbnailMaxSize / imageMaxSize);
    }

    private int GetMaxSizeValue(int size, int imageSize, int defaultMaxValue)
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
            thumbnailWidth = this.GetMaxSizeValue(width, image.Width, DefaultMaxWidth);
            thumbnailHeight = this.GetMinSizeValue(thumbnailWidth, image.Width, image.Height);
        }
        else
        {
            thumbnailHeight = this.GetMaxSizeValue(height, image.Height, DefaultMaxHeight);
            thumbnailWidth = this.GetMinSizeValue(thumbnailHeight, image.Height, image.Width);
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
}
