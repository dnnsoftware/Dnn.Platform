# DotNetNuke.ContentSecurityPolicy

The `DotNetNuke.ContentSecurityPolicy` library provides a fluent API for building and emitting Content Security Policy (CSP) headers in DNN. The `IContentSecurityPolicy` interface is the main entry point to compose directives, manage sources, configure reporting, and generate final header strings.

## Interface: `IContentSecurityPolicy`
Namespace: `DotNetNuke.ContentSecurityPolicy`

### Properties
- **Nonce**: Cryptographically secure nonce value to use with inline script/style tags.
- **DefaultSource**: `SourceCspContributor` for `default-src`.
- **ScriptSource**: `SourceCspContributor` for `script-src`.
- **StyleSource**: `SourceCspContributor` for `style-src`.
- **ImgSource**: `SourceCspContributor` for `img-src`.
- **ConnectSource**: `SourceCspContributor` for `connect-src`.
- **FontSource**: `SourceCspContributor` for `font-src`.
- **ObjectSource**: `SourceCspContributor` for `object-src`.
- **MediaSource**: `SourceCspContributor` for `media-src`.
- **FrameSource**: `SourceCspContributor` for `frame-src`.
- **FrameAncestors**: `SourceCspContributor` for `frame-ancestors`.
- **FormAction**: `SourceCspContributor` for `form-action`.
- **BaseUriSource**: `SourceCspContributor` for `base-uri`.

### Methods
- **RemoveScriptSources(CspSourceType cspSourceType)**: Remove script sources of the specified type (e.g., `Inline`, `Self`, `Nonce`).
- **AddPluginTypes(string value)**: Add values for `plugin-types` (e.g., `application/pdf`).
- **AddSandboxDirective(string value)**: Add `sandbox` options (e.g., `allow-scripts allow-same-origin`).
- **AddFormAction(CspSourceType sourceType, string value)**: Add a `form-action` source.
- **AddFrameAncestors(CspSourceType sourceType, string value)**: Add a `frame-ancestors` source.
- **AddReportEndpoint(string name, string value)**: Add a named reporting endpoint.
- **AddReportTo(string value)**: Add a `report-to` group name to the policy.
- **AddHeaders(string cspHeader)**: Parse and merge a CSP header string; returns the same `IContentSecurityPolicy` for chaining.
- **GeneratePolicy()**: Build the `Content-Security-Policy` header value.
- **GenerateReportingEndpoints()**: Build the reporting header value(s).
- **UpgradeInsecureRequests()**: Add the `upgrade-insecure-requests` directive.

## Working with sources
Directive properties expose a `SourceCspContributor`, which supports adding/removing sources such as:
- `AddSelf()` → `'self'`
- `AddNone()` → `'none'`
- `AddInline()` → `'unsafe-inline'`
- `AddEval()` → `'unsafe-eval'`
- `AddStrictDynamic()` → `'strict-dynamic'`
- `AddNonce(string)` → `'nonce-<value>'`
- `AddHash(string)` → `'sha256-...'`, `'sha384-...'`, `'sha512-...'`
- `AddHost(string)` → `example.com`, `https://cdn.example.com`
- `AddScheme(string)` → `https:`, `data:`, `blob:`
- `RemoveSources(CspSourceType)` to remove by type

See: `CspSourceType.cs`, `CspSource.cs`, `SourceCspContributor.cs`.

## Usage examples

### Configure a baseline policy with a nonce
```csharp
using DotNetNuke.ContentSecurityPolicy;

public class CspExample
{
    private readonly IContentSecurityPolicy _csp;

    public CspExample(IContentSecurityPolicy csp)
    {
        _csp = csp;
    }

    public void Configure()
    {
        // Default baseline
        _csp.DefaultSource.AddSelf();
        _csp.ScriptSource.AddSelf().AddNonce(_csp.Nonce);
        _csp.StyleSource.AddSelf().AddNonce(_csp.Nonce);
        _csp.ImgSource.AddSelf().AddScheme("data:");

        // Lock down frames and forms
        _csp.FrameAncestors.AddNone();
        _csp.FormAction.AddSelf();

        // Reporting
        _csp.AddReportEndpoint("csp-endpoint", "/api/csp/report");
        _csp.AddReportTo("csp-endpoint");

        // Optionally upgrade insecure requests
        _csp.UpgradeInsecureRequests();

        // Generate header values
        var cspHeader = _csp.GeneratePolicy();
        var reportingHeader = _csp.GenerateReportingEndpoints();
        // Emit headers via your pipeline/middleware/module
    }
}
```

### Parse and merge an existing CSP header
```csharp
_csp.AddHeaders("default-src 'self'; img-src 'self' data:")
    .ScriptSource.AddNonce(_csp.Nonce);

var headerValue = _csp.GeneratePolicy();
```

### Remove an unsafe source
```csharp
_csp.RemoveScriptSources(CspSourceType.Inline);
```

## Notes
- Nonce: use `Nonce` in your inline tags: `<script nonce="{policy.Nonce}">`.
- Reporting: ensure your endpoint exists to accept violation reports.
- Parsing: `AddHeaders` is useful to import settings from configuration and extend them programmatically.

## Related files
- `ContentSecurityPolicy.cs`: policy composition and generation.
- `ContentSecurityPolicyParser.cs`: header string parsing.
- `CspDirectiveType.cs`, `CspDirectiveNameMapper.cs`: directive mapping.
- `CspSourceTypeNameMapper.cs`: maps source type names.
- `ReportingCspContributor.cs`, `ReportingEndpointContributor.cs`: reporting helpers.
- `DocumentCspContributor.cs`, `BaseCspContributor.cs`, `CspContributor.cs`: contributor abstractions.


