// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.IO;

namespace DotNetNuke.Services.Mail
{
    public class MailAttachment
    {
        private const string DefaultContentType = "application/octet-stream";

        public MailAttachment(string FilePath)
        {
            var Content = File.ReadAllBytes(FilePath);
            var Filename = Path.GetFileName(FilePath);
            var ContentType = GetContentType(Path.GetExtension(FilePath));

            this.MailAttachmentInternal(Filename, Content, ContentType);
        }
        public MailAttachment(string Filename, Byte[] Content)
        {
            this.MailAttachmentInternal(Filename, Content, GetContentType(Filename));

        }
        public MailAttachment(string Filename, Byte[] Content, string ContentType)
        {
            this.MailAttachmentInternal(Filename, Content, ContentType);

        }
        private void MailAttachmentInternal(string Filename, Byte[] Content, string ContentType)
        {
            this.Filename = Filename;
            this.Content = Content;
            this.ContentType = ContentType;
        }

        private string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case "jpeg":
                case "jpg":
                    {
                        return "image/jpeg";
                    }

                case "png":
                    {
                        return "image/png";
                    }

                case "gif":
                    {
                        return "image/gif";
                    }

                case "pdf":
                    {
                        return "application/pdf";
                    }

                case "zip":
                    {
                        return "application/zip";
                    }

                default:
                    {
                        return DefaultContentType;
                    }
            }
        }

        public byte[] Content { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
    }
}
