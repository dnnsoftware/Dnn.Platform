namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using System.Collections.Generic;
    using DotNetNuke.Abstractions.ClientResources;

    public abstract class ResourceBase : IResource
    {
        private string name;

        public string FilePath { get; set; }
        public string PathNameAlias { get; set; }
        public string ResolvedPath { get; set; }
        public string Key { get; set; }
        public string CdnUrl { get; set; }
        public int Priority { get; set; }
        public string Provider { get; set; } = ClientResourceProviders.DnnPageHeaderProvider;
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(name) ? this.FilePath : name;
            }
            set => name = value;
        }
        public string Version { get; set; }
        public bool ForceVersion { get; set; }
        public CrossOrigin CrossOrigin { get; set; } = CrossOrigin.None;
        public FetchPriority FetchPriority { get; set; } = FetchPriority.Auto;
        public ReferrerPolicy ReferrerPolicy { get; set; } = ReferrerPolicy.None;
        public bool Preload { get; set; } = false;

        /// <summary>
        /// Contains inline metadata — a base64-encoded cryptographic hash of the resource (file) you're telling the browser to fetch.
        /// The browser can use this to verify that the fetched resource has been delivered without unexpected manipulation.
        /// </summary>
        public string Integrity { get; set; } = "";

        public bool Blocking { get; set; } = false;

        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        protected string RenderBlocking()
        {
            if (this.Blocking)
            {
                return " blocking=\"render\"";
            }
            return string.Empty;
        }

        protected string RenderCrossOriginAttribute()
        {
            if (this.CrossOrigin != CrossOrigin.None)
            {
                var crossOrigin = "anonymous";
                if (this.CrossOrigin == CrossOrigin.UseCredentials)
                {
                    crossOrigin = "use-credentials";
                }
                return $" crossorigin=\"{crossOrigin}\"";
            }
            return string.Empty;
        }

        protected string RenderFetchPriority()
        {
            if (this.FetchPriority != FetchPriority.Auto)
            {
                var fetchPriority = "low";
                if (this.FetchPriority == FetchPriority.High)
                {
                    fetchPriority = "high";
                }
                return $" fetchpriority=\"{fetchPriority}\"";
            }
            return string.Empty;
        }

        protected string RenderIntegrity()
        {
            if (!string.IsNullOrEmpty(this.Integrity))
            {
                return $" integrity=\"{this.Integrity}\"";
            }
            return string.Empty;
        }

        protected string RenderReferrerPolicy()
        {
            if (this.ReferrerPolicy != ReferrerPolicy.None)
            {
                switch (this.ReferrerPolicy)
                {
                    case ReferrerPolicy.NoReferrer:
                        return " referrerpolicy=\"no-referrer\"";
                    case ReferrerPolicy.NoReferrerWhenDowngrade:
                        return " referrerpolicy=\"no-referrer-when-downgrade\"";
                    case ReferrerPolicy.Origin:
                        return " referrerpolicy=\"origin\"";
                    case ReferrerPolicy.OriginWhenCrossOrigin:
                        return " referrerpolicy=\"origin-when-cross-origin\"";
                    case ReferrerPolicy.SameOrigin:
                        return " referrerpolicy=\"same-origin\"";
                    case ReferrerPolicy.StrictOrigin:
                        return " referrerpolicy=\"strict-origin\"";
                    case ReferrerPolicy.StrictOriginWhenCrossOrigin:
                        return " referrerpolicy=\"strict-origin-when-cross-origin\"";
                    case ReferrerPolicy.UnsafeUrl:
                        return " referrerpolicy=\"unsafe-url\"";
                }
            }
            return string.Empty;
        }

        protected string RenderAttributes()
        {
            var htmlString = string.Empty;
            foreach (var attribute in this.Attributes)
            {
                htmlString += $" {attribute.Key}=\"{attribute.Value}\"";
            }
            return htmlString;
        }

        internal string GetVersionedPath(int crmVersion, bool useCdn, string applicationPath)
        {
            var path = this.ResolvedPath;
            if (path.StartsWith("~"))
            {
                if (string.IsNullOrEmpty(applicationPath))
                {
                    path = path.TrimStart('~');
                }
                else
                {
                    path = path.Replace("~", applicationPath);
                }
            }
            if (useCdn && !string.IsNullOrEmpty(this.CdnUrl))
            {
                path = this.CdnUrl;
            }
            return $"{path}?cdv={crmVersion}";
        }

        public void Register()
        {
            throw new System.NotImplementedException();
        }

        public string Render(int crmVersion, bool useCdn, string applicationPath)
        {
            throw new System.NotImplementedException();
        }
    }
}
