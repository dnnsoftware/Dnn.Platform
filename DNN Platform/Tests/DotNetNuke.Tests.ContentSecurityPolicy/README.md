# DotNetNuke.ContentSecurityPolicy.Tests

This project contains comprehensive unit tests for the DotNetNuke ContentSecurityPolicy library, specifically focusing on the CSP header parsing functionality.

## Overview

The test suite validates all aspects of the CSP parsing implementation, including:

- **Parser functionality** - Core parsing logic and error handling
- **Static method integration** - ContentSecurityPolicy.Parse() and TryParse() methods
- **Mapping utilities** - Directive and source type name mapping
- **Integration scenarios** - Real-world CSP header parsing
- **Performance characteristics** - Large policy parsing efficiency

## Test Structure

### Test Classes

#### `ContentSecurityPolicyParserTests`
Tests the core `ContentSecurityPolicyParser` class functionality:
- Basic policy parsing
- Complex multi-directive policies  
- Nonce and hash source parsing
- Error handling for invalid input
- Support for all CSP directive types

#### `ContentSecurityPolicyTests`
Tests the parsing methods on the `ContentSecurityPolicy` class:
- `Parse()` method
- Policy modification after parsing
- Nonce generation integration

#### `CspDirectiveNameMapperTests`
Tests the directive name mapping utilities:
- Bidirectional mapping between directive names and types
- Case-insensitive parsing
- Error handling for unknown directives
- Round-trip conversion validation

#### `CspSourceTypeNameMapperTests`
Tests the source type name mapping utilities:
- Source type identification
- Helper methods (IsQuotedKeyword, IsNonceSource, IsHashSource)
- Round-trip conversion for supported types
- Error handling for invalid source names

#### `IntegrationTests`
Comprehensive integration tests based on real-world scenarios:
- All examples from `CspParsingExample`
- Complex policy parsing and regeneration
- Policy modification workflows
- Edge case handling
- Performance testing with large policies

#### `TestRunner`
Demonstration class that shows practical usage:
- Interactive examples
- Performance benchmarking
- Error handling scenarios
- Real-world policy processing

## Test Data

The tests use a variety of CSP header examples:

### Basic Examples
```
default-src 'self'
script-src 'self' 'unsafe-inline' https://cdn.example.com
```

### Complex Examples
```
default-src 'self'; script-src 'self' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self' wss:; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; upgrade-insecure-requests; report-uri /csp-report
```

### Real-World Examples
```
default-src 'self'; img-src 'self' https://front.satrabel.be https://www.googletagmanager.com https://region1.google-analytics.com; font-src 'self' https://fonts.gstatic.com; style-src 'self' https://fonts.googleapis.com https://www.googletagmanager.com; frame-ancestors 'self'; frame-src 'self'; form-action 'self'; object-src 'none'; base-uri 'self'; script-src 'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ=' 'strict-dynamic'; report-to csp-endpoint; report-uri https://dnncore.satrabel.be/DesktopModules/Csp/Report; connect-src https://www.googletagmanager.com https://region1.google-analytics.com https://www.google-analytics.com; upgrade-insecure-requests
```

## Running Tests

### Visual Studio
1. Open the solution in Visual Studio
2. Build the solution to restore NuGet packages
3. Open Test Explorer (Test → Test Explorer)
4. Run all tests or specific test classes

### Command Line
```bash
# Navigate to the test project directory
cd "DNN Platform/DotNetNuke.ContentSecurityPolicy.Tests"

# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "ClassName=ContentSecurityPolicyParserTests"

# Run with coverage (requires additional packages)
dotnet test --collect:"XPlat Code Coverage"
```

### Test Runner
You can also run the interactive `TestRunner` class to see examples in action:

```csharp
// In a console application or test method
TestRunner.RunAllExamples();
TestRunner.RunSourceTypeExamples();
```

## Dependencies

The test project uses the following packages:

- **Microsoft.NET.Test.Sdk** - Test platform
- **MSTest.TestAdapter** - MSTest test runner
- **MSTest.TestFramework** - MSTest framework
- **FluentAssertions** - Readable test assertions
- **coverlet.collector** - Code coverage collection
- **StyleCop.Analyzers** - Code style analysis

## Test Coverage

The test suite covers:

✅ **Parser Core Functionality**
- All CSP directive types
- All source types (self, unsafe-inline, nonce, hash, host, scheme, etc.)
- Complex multi-directive policies
- Error handling and validation

✅ **Integration Scenarios**  
- Real-world CSP headers
- Policy modification after parsing
- Round-trip parsing (parse → modify → regenerate)
- Performance with large policies

✅ **Edge Cases**
- Empty and null inputs
- Invalid directive names
- Malformed source values
- Case sensitivity handling
- Whitespace handling

✅ **API Surface**
- Static Parse/TryParse methods
- Instance method integration
- Mapping utility functions
- Helper method validation

## Performance

The test suite includes performance benchmarks:

- **Basic parsing**: < 1ms for typical policies
- **Complex parsing**: < 10ms for large multi-directive policies  
- **Real-world parsing**: < 5ms for production CSP headers
- **Large policies**: < 100ms for policies with 50+ directives

## Contributing

When adding new tests:

1. Follow the existing test naming conventions
2. Use FluentAssertions for readable assertions
3. Add both positive and negative test cases
4. Include edge cases and error conditions
5. Document complex test scenarios with comments
6. Update this README if adding new test categories

## Example Usage

```csharp
[TestMethod]
public void Parse_BasicPolicy_ShouldReturnValidPolicy()
{
    // Arrange
    var cspHeader = "default-src 'self'";

    // Act
    var policy = ContentSecurityPolicyParser.Parse(cspHeader);

    // Assert
    policy.Should().NotBeNull();
    policy.GeneratePolicy().Should().Be("default-src 'self'");
}
```

This test structure ensures comprehensive validation of the CSP parsing functionality while providing clear examples of how to use the API.
