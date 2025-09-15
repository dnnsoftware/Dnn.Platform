using System;
using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager
{
    public static class GenericResourceExtensions
    {
        public static T FromSrc<T>(this T input, string scriptSrc) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.FilePath = scriptSrc;
            return input;
        }

        public static T FromSrc<T>(this T input, string scriptSrc, string pathNameAlias) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.FilePath = scriptSrc;
            input.PathNameAlias = pathNameAlias;
            return input;
        }

        public static T SetPriority<T>(this T input, int priority) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Priority = priority;
            return input;
        }

        public static T SetNameAndVersion<T>(this T input, string name, string version, bool forceVersion) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Name = name;
            input.Version = version;
            input.ForceVersion = forceVersion;
            return input;
        }

        public static T SetCdnUrl<T>(this T input, string cdnUrl) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.CdnUrl = cdnUrl;
            return input;
        }

        public static T SetProvider<T>(this T input, string provider) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Provider = provider;
            return input;
        }

        public static T SetPreload<T>(this T input) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Preload = true;
            return input;
        }

        public static T SetBlocking<T>(this T input) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Blocking = true;
            return input;
        }

        public static T SetIntegrity<T>(this T input, string hash) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Integrity = hash;
            return input;
        }

        public static T SetCrossOrigin<T>(this T input, CrossOrigin crossOrigin) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.CrossOrigin = crossOrigin;
            return input;
        }

        public static T SetFetchPriority<T>(this T input, FetchPriority fetchPriority) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.FetchPriority = fetchPriority;
            return input;
        }

        public static T SetReferrerPolicy<T>(this T input, ReferrerPolicy referrerPolicy) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.ReferrerPolicy = referrerPolicy;
            return input;
        }

        public static T AddAttribute<T>(this T input, string attributeName, string attributeValue) where T : IResource
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!string.IsNullOrEmpty(attributeName) && !string.IsNullOrEmpty(attributeValue))
            {
                if (input.Attributes.ContainsKey(attributeName))
                {
                    input.Attributes[attributeName] = attributeValue;
                }
                else
                {
                    input.Attributes.Add(attributeName, attributeValue);
                }
            }
            return input;
        }

    }
}
