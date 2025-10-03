# Content Security Policy Parser

This document describes the CSP header parsing functionality that has been added to the DotNetNuke.ContentSecurityPolicy library.

## Overview

The CSP parser allows you to parse existing Content Security Policy headers from strings into `ContentSecurityPolicy` objects that can be manipulated and regenerated.

## New Classes

### ContentSecurityPolicyParser

A static utility class that provides parsing functionality for CSP headers.

**Key Methods:**
- `Parse(string cspHeader)` - Parses a CSP header string and returns an `IContentSecurityPolicy` object
- `TryParse(string cspHeader, out IContentSecurityPolicy policy)` - Safely attempts to parse a CSP header

### Enhanced ContentSecurityPolicy

The main `ContentSecurityPolicy` class now includes static parsing methods:

**New Static Methods:**
- `Parse(string cspHeader)` - Static method that delegates to `ContentSecurityPolicyParser.Parse()`
- `TryParse(string cspHeader, out IContentSecurityPolicy policy)` - Static method that delegates to `ContentSecurityPolicyParser.TryParse()`

### Enhanced Mapping Classes

#### CspDirectiveNameMapper
- Added `GetDirectiveType(string directiveName)` - Convert directive names to enum values
- Added `TryGetDirectiveType(string directiveName, out CspDirectiveType directiveType)` - Safe conversion

#### CspSourceTypeNameMapper
- Added `GetSourceType(string sourceName)` - Convert source names to enum values
- Added `TryGetSourceType(string sourceName, out CspSourceType sourceType)` - Safe conversion
- Added helper methods: `IsQuotedKeyword()`, `IsNonceSource()`, `IsHashSource()`

## Usage Examples

### Basic Parsing

```csharp
// Parse a simple CSP header
var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline'";
var policy = ContentSecurityPolicy.Parse(cspHeader);

// Access parsed directives
Console.WriteLine(policy.GeneratePolicy());
```

### Safe Parsing

```csharp
// Safely parse with error handling
var cspHeader = "default-src 'self'; invalid-directive something";
if (ContentSecurityPolicy.TryParse(cspHeader, out var policy))
{
    Console.WriteLine("Successfully parsed policy");
    Console.WriteLine(policy.GeneratePolicy());
}
else
{
    Console.WriteLine("Failed to parse CSP header");
}
```

### Complex Policy Parsing

```csharp
// Parse a complex CSP header
var complexHeader = "default-src 'self'; script-src 'self' 'nonce-abc123' https://cdn.example.com; style-src 'self' 'unsafe-inline' 'sha256-xyz789'; img-src 'self' data: https:; connect-src 'self' wss:; frame-ancestors 'none'; upgrade-insecure-requests";

var policy = ContentSecurityPolicy.Parse(complexHeader);

// Modify the parsed policy
policy.ScriptSource.AddHost("newcdn.example.com");
policy.StyleSource.AddHash("sha256-newHash123");

// Generate the updated policy
var updatedHeader = policy.GeneratePolicy();
Console.WriteLine(updatedHeader);
```

## Supported CSP Directives

The parser supports all standard CSP directives:

### Source-based Directives
- `default-src`
- `script-src`
- `style-src`
- `img-src`
- `connect-src`
- `font-src`
- `object-src`
- `media-src`
- `frame-src`
- `form-action`
- `frame-ancestors`
- `base-uri`

### Document Directives
- `sandbox`
- `plugin-types`
- `upgrade-insecure-requests`

### Reporting Directives
- `report-uri`
- `report-to`

## Supported Source Types

The parser correctly identifies and processes all CSP source types:

### Quoted Keywords
- `'self'`
- `'unsafe-inline'`
- `'unsafe-eval'`
- `'none'`
- `'strict-dynamic'`

### Cryptographic Values
- `'nonce-<base64-value>'`
- `'sha256-<base64-value>'`
- `'sha384-<base64-value>'`
- `'sha512-<base64-value>'`

### Hosts and Schemes
- Domains: `example.com`, `*.example.com`, `https://example.com`
- Schemes: `https:`, `data:`, `blob:`, `wss:`, etc.

## Error Handling

The parser includes robust error handling:

1. **Unknown Directives**: Silently ignored (following CSP specification)
2. **Invalid Source Values**: May throw exceptions or be ignored depending on severity
3. **Malformed Headers**: Will throw `ArgumentException` with descriptive messages

## Example Use Cases

### 1. CSP Header Modification

```csharp
// Parse existing policy
var existingPolicy = ContentSecurityPolicy.Parse(Request.Headers["Content-Security-Policy"]);

// Add new trusted sources
existingPolicy.ScriptSource.AddHost("newapi.example.com");
existingPolicy.StyleSource.AddNonce(nonce);

// Update the header
Response.Headers["Content-Security-Policy"] = existingPolicy.GeneratePolicy();
```

### 2. CSP Validation and Analysis

```csharp
// Parse and analyze a policy
var policy = ContentSecurityPolicy.Parse(cspHeaderString);

// Check for unsafe directives
var hasUnsafeInline = policy.ScriptSource.GetSourcesByType(CspSourceType.Inline).Any();
var hasUnsafeEval = policy.ScriptSource.GetSourcesByType(CspSourceType.Eval).Any();

if (hasUnsafeInline || hasUnsafeEval)
{
    Console.WriteLine("Warning: Policy contains unsafe directives");
}
```

### 3. Policy Migration

```csharp
// Parse old policy format and convert to new format
var oldPolicy = ContentSecurityPolicy.Parse(oldCspHeader);

// Remove deprecated sources
oldPolicy.RemoveScriptSources(CspSourceType.Inline);

// Add modern alternatives
oldPolicy.ScriptSource.AddNonce(newNonce);
oldPolicy.ScriptSource.AddStrictDynamic();

var modernPolicy = oldPolicy.GeneratePolicy();
```

## Integration Notes

This parsing functionality integrates seamlessly with the existing DotNetNuke CSP library:

- All existing functionality remains unchanged
- Parsed policies can be modified using existing methods
- Generated policies maintain the same format and structure
- The parser follows the CSP specification for directive and source handling

## Performance Considerations

- Parsing is optimized for typical CSP header sizes
- Uses efficient string operations and LINQ where appropriate
- Caches are not implemented as CSP headers are typically parsed once per request
- Memory usage is minimal for standard-sized policies
