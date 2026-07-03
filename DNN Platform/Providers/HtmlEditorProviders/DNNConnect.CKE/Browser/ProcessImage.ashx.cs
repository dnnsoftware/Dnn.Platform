// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Browser;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;

/// <summary>The process image.</summary>
public class ProcessImage : IHttpHandler
{
    /// <summary>Gets a value indicating whether IsReusable.</summary>
    public bool IsReusable => false;

    /// <summary>The get thumb abort.</summary>
    /// <returns>Returns abort.</returns>
    public bool GetThumbAbort() => false;

    /// <summary>The process request.</summary>
    /// <param name="context">The context.</param>
    public void ProcessRequest(HttpContext context)
    {
        var imageH = float.Parse(context.Request["imageH"]);
        var imageW = float.Parse(context.Request["imageW"]);
        var angle = float.Parse(context.Request["imageRotate"]);
        var sourceImageId = int.Parse(context.Request["fileId"]);
        var imageX = float.Parse(context.Request["imageX"]);
        var imageY = float.Parse(context.Request["imageY"]);
        var selectorH = float.Parse(context.Request["selectorH"]);
        var selectorW = float.Parse(context.Request["selectorW"]);
        var selectorX = float.Parse(context.Request["selectorX"]);
        var selectorY = float.Parse(context.Request["selectorY"]);
        var viewPortH = float.Parse(context.Request["viewPortH"]);
        var viewPortW = float.Parse(context.Request["viewPortW"]);

        bool bSaveFile;
        try
        {
            bSaveFile = bool.Parse(context.Request["saveFile"]);
        }
        catch (Exception)
        {
            bSaveFile = false;
        }

        string sNewFileName = null;
        if (!string.IsNullOrEmpty(context.Request["newFileName"]))
        {
            sNewFileName = context.Request["newFileName"];
        }

        var pWidth = imageW;
        var pHeight = imageH;

        var file = FileManager.Instance.GetFile(sourceImageId);
        if (file == null)
        {
            return;
        }

        var sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);
        if (sourceFolder == null)
        {
            return;
        }

        if (PortalSettings.Current != null
            && !HasReadPermission(sourceFolder))
        {
            throw new SecurityException("You don't have read permission to view files under this folder.");
        }

        var img = (Bitmap)Image.FromStream(FileManager.Instance.GetFileContent(file));

        // Resize
        var imageP = this.ResizeImage(img, Convert.ToInt32(pWidth), Convert.ToInt32(pHeight));

        // Rotate if angle is not 0.00 or 360
        if (angle is > 0.0F and < 360.00F)
        {
            imageP = (Bitmap)RotateImage(imageP, angle);
            pWidth = imageP.Width;
            pHeight = imageP.Height;
        }

        // Calculate Coords of the Image into the ViewPort
        float srcX;
        float dstX;
        float srcY;
        float dstY;

        if (pWidth > viewPortW)
        {
            srcX = Math.Abs(imageX - Math.Abs((imageW - pWidth) / 2));
            dstX = 0;
        }
        else
        {
            srcX = 0;
            dstX = imageX + ((imageW - pWidth) / 2);
        }

        if (pHeight > viewPortH)
        {
            srcY = Math.Abs(imageY - Math.Abs((imageH - pHeight) / 2));
            dstY = 0;
        }
        else
        {
            srcY = 0;
            dstY = imageY + ((imageH - pHeight) / 2);
        }

        // Get Image viewed into the ViewPort
        imageP = ImageCopy(imageP, dstX, dstY, srcX, srcY, viewPortW, viewPortH);

        // Get Selector Portion
        imageP = ImageCopy(imageP, 0, 0, selectorX, selectorY, selectorW, selectorH);

        if (bSaveFile)
        {
            context.Response.ContentType = "text/plain";

            if (PortalSettings.Current != null
                && !HasWritePermission(sourceFolder))
            {
                throw new SecurityException("You don't have write permission to save files under this folder.");
            }

            using var stream = new MemoryStream();
            var fileName = GenerateName(file, sNewFileName);
            imageP.Save(stream, img.RawFormat);

            const bool alreadyCheckedPermissions = true;
            FileManager.Instance.AddFile(sourceFolder, fileName, stream, false, !alreadyCheckedPermissions, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName)));
        }
        else
        {
            context.Response.ContentType = "image/jpeg";
            imageP.Save(context.Response.OutputStream, ImageFormat.Jpeg);
        }

        imageP.Dispose();
        img.Dispose();
    }

    /// <summary>Generates the New File Path.</summary>
    /// <param name="file">The Original Image.</param>
    /// <param name="sNewFileName">New File Name for the Image.</param>
    /// <returns>The generated name.</returns>
    private static string GenerateName(IFileInfo file, string sNewFileName)
    {
        var sNewFilePath = !string.IsNullOrEmpty(sNewFileName)
            ? $"{CleanName(sNewFileName)}.{file.Extension}"
            : $"{Path.GetFileNameWithoutExtension(sNewFileName)}_crop.{file.Extension}";

        var iCounter = 0;

        var folder = FolderManager.Instance.GetFolder(file.FolderId);
        while (FileManager.Instance.FileExists(folder, sNewFilePath))
        {
            iCounter++;

            var sFileNameNoExt = Path.GetFileNameWithoutExtension(sNewFilePath);
            sNewFilePath = $"{sFileNameNoExt}_{iCounter}.{file.Extension}";
        }

        return sNewFilePath;
    }

    /// <summary>The image copy.</summary>
    /// <param name="srcBitmap">The src bitmap.</param>
    /// <param name="dstX">The dst x.</param>
    /// <param name="dstY">The dst y.</param>
    /// <param name="srcX">The src x.</param>
    /// <param name="srcY">The src y.</param>
    /// <param name="dstWidth">The dst width.</param>
    /// <param name="dstHeight">The dst height.</param>
    /// <returns>Returns the copied Bitmap.</returns>
    private static Bitmap ImageCopy(
        Image srcBitmap, float dstX, float dstY, float srcX, float srcY, float dstWidth, float dstHeight)
    {
        // Create the new bitmap and associated graphics object
        var sourceRec = new RectangleF(srcX, srcY, dstWidth, dstHeight);
        var destRec = new RectangleF(dstX, dstY, dstWidth, dstHeight);
        var bmp = new Bitmap(Convert.ToInt32(dstWidth), Convert.ToInt32(dstHeight));
        var g = Graphics.FromImage(bmp);

        g.DrawImage(srcBitmap, destRec, sourceRec, GraphicsUnit.Pixel);
        g.Dispose();

        return bmp;
    }

    /// <summary>Method to rotate an image either clockwise or counter-clockwise.</summary>
    /// <param name="img">the image to be rotated.</param>
    /// <param name="rotationAngle">
    /// the angle (in degrees).
    ///   Positive values will rotate clockwise
    ///   negative values will rotate counter-clockwise.
    /// </param>
    /// <returns>Returns the Rotated Image.</returns>
    private static Image RotateImage(Image img, double rotationAngle)
    {
        var returnBitmap = new Bitmap(img.Width, img.Height + 1);

        var g = Graphics.FromImage(returnBitmap);

        g.TranslateTransform((float)img.Width / 2, (float)img.Height / 2);
        g.RotateTransform((float)rotationAngle);
        g.TranslateTransform(-(float)img.Width / 2, -(float)img.Height / 2);

        g.DrawImage(img, (img.Width / 2) - (img.Height / 2), (img.Height / 2) - (img.Width / 2), img.Height, img.Width);

        return returnBitmap;
    }

    private static string CleanName(string name)
    {
        name = name.Replace(@"\", "/");
        if (name.Contains("/"))
        {
            name = name.Substring(name.LastIndexOf('/') + 1);
        }

        return name;
    }

    private static bool HasReadPermission(IFolderInfo folder)
    {
        return folder != null && FolderPermissionController.HasFolderPermission(folder.FolderPermissions, "READ");
    }

    private static bool HasWritePermission(IFolderInfo folder)
    {
        return folder != null && FolderPermissionController.HasFolderPermission(folder.FolderPermissions, "WRITE");
    }

    /// <summary>The resize image.</summary>
    /// <param name="img">The img.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <returns>Returns the Resized Bitmap.</returns>
    private Bitmap ResizeImage(Image img, int width, int height)
    {
        Image.GetThumbnailImageAbort callback = this.GetThumbAbort;
        return (Bitmap)img.GetThumbnailImage(width, height, callback, IntPtr.Zero);
    }
}
