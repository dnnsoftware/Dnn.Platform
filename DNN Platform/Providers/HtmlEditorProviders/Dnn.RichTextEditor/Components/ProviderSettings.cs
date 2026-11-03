// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components
{
    using System.Collections.Specialized;

    using DotNetNuke.Framework.Providers;

    /// <summary>Reads the attributes of the provider's entry in the <c>htmlEditor</c> section of web.config.</summary>
    public static class ProviderSettings
    {
        /// <summary>The provider's name in web.config.</summary>
        public const string ProviderName = "Dnn.RichTextEditor";

        /// <summary>The virtual path of the provider's files.</summary>
        public const string ProviderPath = "~/Providers/HtmlEditorProviders/Dnn.RichTextEditor/";

        private const string ProviderType = "htmlEditor";

        /// <summary>Gets the attributes of the provider's web.config entry.</summary>
        /// <returns>The attributes, or an empty collection if the provider is not configured.</returns>
        public static NameValueCollection GetAttributes()
        {
            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
            var provider = (Provider)providerConfiguration?.Providers[ProviderName];
            return provider?.Attributes ?? new NameValueCollection();
        }
    }
}
