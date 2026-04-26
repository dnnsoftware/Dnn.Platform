// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS0618 // Type or member is obsolete
    using DeprecatedCryptographyProvider = DotNetNuke.Services.Cryptography.CryptographyProvider;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>The default <see cref="IFileLinkClickController"/> implementation.</summary>
    /// <param name="cryptographyProvider">The cryptography provider.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    public class FileLinkClickController(ICryptographyProvider cryptographyProvider, IHostSettings hostSettings, IHostSettingsService hostSettingsService)
        : ServiceLocator<IFileLinkClickController, FileLinkClickController>, IFileLinkClickController
    {
        private readonly ILog logger = LoggerSource.Instance.GetLogger(typeof(FileLinkClickController));
        private readonly ICryptographyProvider cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IHostSettingsService hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();

        /// <summary>Initializes a new instance of the <see cref="FileLinkClickController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.4.0. Use overload with IICryptographyProvider. Scheduled removal in v12.0.0.")]
        public FileLinkClickController()
            : this(null, null, null)
        {
        }

        /// <inheritdoc />
        public string GetFileLinkClick(IFileInfo file)
        {
            Requires.NotNull("file", file);
            var portalId = file.PortalId;
            var linkClickPortalSettings = this.GetPortalSettingsForLinkClick(portalId);

            return TestableGlobals.Instance.LinkClick(
                $"fileid={file.FileId.ToString(CultureInfo.InvariantCulture)}",
                Null.NullInteger,
                Null.NullInteger,
                true,
                false,
                portalId,
                linkClickPortalSettings.EnableUrlLanguage,
                linkClickPortalSettings.PortalGUID);
        }

        /// <inheritdoc />
        public int GetFileIdFromLinkClick(NameValueCollection queryParams)
        {
            var linkClickPortalSettings = this.GetPortalSettingsForLinkClick(GetPortalIdFromLinkClick(queryParams));
            var fileTicket = queryParams["fileticket"];
            var encryptionKey = linkClickPortalSettings.PortalGUID;
            var strFileId = UrlUtils.DecryptParameter(this.cryptographyProvider, fileTicket, encryptionKey);
            if (string.IsNullOrEmpty(strFileId))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                string obsoleteFileId = null;
                if (this.cryptographyProvider is DeprecatedCryptographyProvider deprecatedCryptographyProvider)
                {
                    obsoleteFileId = UrlUtils.DecryptParameter(deprecatedCryptographyProvider.DecryptParameter, fileTicket, encryptionKey);
                }
#pragma warning restore CS0618 // Type or member is obsolete

                if (string.IsNullOrEmpty(obsoleteFileId))
                {
                    this.logger.InfoFormat(CultureInfo.InvariantCulture, "Encountered FileTicket value {0} which was not able to be decrypted", fileTicket);
                }
                else if (this.hostSettingsService.GetBoolean("AllowFileTicketDecryptFallback", false))
                {
                    this.logger.WarnFormat(CultureInfo.InvariantCulture, "Encountered FileTicket value {0} which was generated with DES algorithm, regenerating this URL is recommended, falling back to broken DES algorithm based on AllowFileTicketDecryptFallback host setting", fileTicket);
                    strFileId = obsoleteFileId;
                }
                else
                {
                    this.logger.ErrorFormat(CultureInfo.InvariantCulture, "Encountered FileTicket value {0} which was generated with DES algorithm, set AllowFileTicketDecryptFallback host setting to enable accessing this URL; however, regenerating this URL is recommended instead", fileTicket);
                }
            }

            return int.TryParse(strFileId, out var fileId) ? fileId : -1;
        }

        /// <inheritdoc />
        protected override Func<IFileLinkClickController> GetFactory()
        {
            return () => Globals.DependencyProvider.GetRequiredService<IFileLinkClickController>();
        }

        private static int GetPortalIdFromLinkClick(NameValueCollection queryParams)
        {
            if (queryParams["hf"] != null && queryParams["hf"] == "1")
            {
                return Null.NullInteger;
            }

            if (queryParams["portalid"] != null)
            {
                if (int.TryParse(queryParams["portalid"], out var portalId))
                {
                    return portalId;
                }
            }

            return PortalSettings.Current.PortalId;
        }

        private LinkClickPortalSettings GetPortalSettingsForLinkClick(int portalId)
        {
            if (portalId == Null.NullInteger)
            {
                return new LinkClickPortalSettings
                {
                    PortalGUID = this.hostSettings.Guid,
                    EnableUrlLanguage = this.hostSettings.EnableUrlLanguage,
                };
            }

            var portalSettings = new PortalSettings(portalId);
            return new LinkClickPortalSettings
            {
                PortalGUID = portalSettings.GUID.ToString(),
                EnableUrlLanguage = portalSettings.EnableUrlLanguage,
            };
        }
    }
}
