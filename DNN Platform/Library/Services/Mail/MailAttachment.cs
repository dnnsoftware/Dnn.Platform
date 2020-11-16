// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Web;

namespace DotNetNuke.Services.Mail
{
    public class MailAttachment
    {
        private const string DefaultContentType = "application/octet-stream";

        /// <summary>
        /// Initializes a new instance of the <see cref="MailAttachment"/> class.
        /// </summary>
        /// <param name="FilePath"></param>
        public MailAttachment(string FilePath)
        {
            var Content = File.ReadAllBytes(FilePath);
            var Filename = Path.GetFileName(FilePath);
            var ContentType = MimeMapping.GetMimeMapping(Filename);

            this.MailAttachmentInternal(Filename, Content, ContentType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MailAttachment"/> class.
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="Content"></param>
        public MailAttachment(string Filename, Byte[] Content)
        {
            this.MailAttachmentInternal(Filename, Content, MimeMapping.GetMimeMapping(Filename));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MailAttachment"/> class.
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="Content"></param>
        /// <param name="ContentType"></param>
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

        public byte[] Content { get; set; }

        public string Filename { get; set; }

        public string ContentType { get; set; }
    }
}
