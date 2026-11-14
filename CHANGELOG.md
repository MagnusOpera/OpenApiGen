# Changelog

Release notes for OpenApiGen. Older entries were reconstructed from the tagged commit history.

## [Unreleased]

- Publish NuGet packages with GitHub Actions trusted publishing and support retrying from an existing GitHub release.

## [0.47]

### [0.47.0]

- Install and run the same OpenApiGen tool package on .NET 10 and .NET 11.
- Generate clients for supported operations in OpenAPI 3.2 documents, including HTTP QUERY.
- Test both tool runtimes and both ASP.NET Core document versions with Axios and fetch.
- Add a product website and an OpenAPI-to-TypeScript quickstart with runnable ASP.NET Core and TypeScript examples.
- Browse the changelog and release history on the product website.
- Move the .NET projects under `src/`.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.46.0...0.47.0

## [0.46]

### [0.46.0]

- Send uppercase HTTP methods in generated Axios and fetch clients.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.45.0...0.46.0

## [0.45]

### [0.45.0]

- Expand the sample API and regression coverage for authentication, multipart uploads, downloads, nullable types, and response handling.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.44.0...0.45.0

## [0.44]

### [0.44.0]

- Add fetch as a client transport and select the transport with `--transport fetch` or `--transport axios`.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.43.0...0.44.0

## [0.43]

### [0.43.0]

- Export generated TypeScript types from the client index.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.42.0...0.43.0

## [0.42]

### [0.42.0]

- Update the sample API and use generated regular expressions in the Axios generator.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.41.0...0.42.0

## [0.41]

### [0.41.0]

- Inline shared schemas in generated TypeScript types.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.40.0...0.41.0

## [0.40]

### [0.40.0]

- Move shared schema types into their own generated module.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.39.0...0.40.0

## [0.39]

### [0.39.0]

- Generate TypeScript unions for schemas that use `oneOf`.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.38.0...0.39.0

## [0.38]

### [0.38.0]

- Run OpenApiGen on .NET 10.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.37.0...0.38.0

## [0.37]

### [0.37.0]

- Handle OpenAPI 3.1.1 documents emitted by ASP.NET Core on .NET 10.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.36.0...0.37.0

## [0.36]

### [0.36.0]

- Generate clients that download binary responses.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.35.0...0.36.0

## [0.35]

### [0.35.0]

- Read OpenAPI 3.1 schemas. Return a declared default response with status 0.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.34.0...0.35.0

## [0.34]

### [0.34.0]

- Throw an Error object for unexpected HTTP responses.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.33.2...0.34.0

## [0.33]

### [0.33.2]

- Restore the previous handling of empty request bodies.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.33.1...0.33.2

### [0.33.1]

- Use empty content for requests with an empty body.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.33.0...0.33.1

### [0.33.0]

- Normalize field names when generating TypeScript.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.32.0...0.33.0

## [0.32]

### [0.32.0]

- Use the status-aware client function under the original operation name.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.31.0...0.32.0

## [0.31]

### [0.31.0]

- Generate TypeScript map types for schemas with additional properties.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.30.0...0.31.0

## [0.30]

### [0.30.0]

- Remove the deprecated client functions and retain the status-aware variants.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.29.0...0.30.0

## [0.29]

### [0.29.0]

- Generate status-aware clients for responses without a body.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.28.0...0.29.0

## [0.28]

### [0.28.0]

- Send multipart form data through the correct Axios request field.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.27.0...0.28.0

## [0.27]

### [0.27.0]

- Make generated response handling easier to read.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.26.0...0.27.0

## [0.26]

### [0.26.0]

- Build multipart request bodies with FormData and simplify generated query string code.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.25.0...0.26.0

## [0.25]

### [0.25.0]

- Place non-query arguments before optional query arguments in generated functions.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.24.0...0.25.0

## [0.24]

### [0.24.0]

- Handle query parameters in both generated client function signatures.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.23.0...0.24.0

## [0.23]

### [0.23.0]

- Fix query string generation.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.22.0...0.23.0

## [0.22]

### [0.22.0]

- Simplify generated request code.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.21.0...0.22.0

## [0.21]

### [0.21.0]

- Simplify URL generation for operations without query parameters.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.20.0...0.21.0

## [0.20]

### [0.20.0]

- Simplify default value handling in generated code.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.18.0...0.20.0

## [0.18]

### [0.18.0]

- Tag the same revision as 0.17.0.

**Release tag**: https://github.com/MagnusOpera/OpenApiGen/releases/tag/0.18.0

## [0.17]

### [0.17.0]

- Skip undefined query arguments and URL-encode query values.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.16.0...0.17.0

## [0.16]

### [0.16.0]

- Add API key support and update the authentication examples.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.15.0...0.16.0

## [0.15]

### [0.15.0]

- Generate clients that send bearer authentication.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.14.0...0.15.0

## [0.14]

### [0.14.0]

- Report an error when an operation omits a 200 response.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.13.0...0.14.0

## [0.13]

### [0.13.0]

- Keep operations that share a tag in the same module and support plain-text requests.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.12.0...0.13.0

## [0.12]

### [0.12.0]

- Drop an unused import from generated clients.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.11.0...0.12.0

## [0.11]

### [0.11.0]

- Fix generation of the legacy client function.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.10.0...0.11.0

## [0.10]

### [0.10.0]

- Generate discriminated response tuples for different HTTP statuses.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.9.0...0.10.0

## [0.9]

### [0.9.0]

- Normalize capitalization when building names from routes.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.8.0...0.9.0

## [0.8]

### [0.8.0]

- Fix generated return types for responses without a body.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.7.1...0.8.0

## [0.7]

### [0.7.1]

- Handle operations with a void response.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.7.0...0.7.1

### [0.7.0]

- Resolve component schemas that reference another component.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.6.0...0.7.0

## [0.6]

### [0.6.0]

- Document the nullable schema workaround and add a sample API.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.5.0...0.6.0

## [0.5]

### [0.5.0]

- Improve matching of nullable schemas to equivalent component schemas.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.4.0...0.5.0

## [0.4]

### [0.4.0]

- Consolidate schema substitution warnings and cache repeated lookups.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.3.0...0.4.0

## [0.3]

### [0.3.0]

- Refine how the generator recovers nullable schema types.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.2.0...0.3.0

## [0.2]

### [0.2.0]

- Work around nullable schemas emitted as any and fix schema deserialization.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.1.0...0.2.0

## [0.1]

### [0.1.0]

- Improve generation for composed schemas and nullable references.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.0.6...0.1.0

## [0.0]

### [0.0.6]

- Support scalar responses and operations that return void.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.0.5...0.0.6

### [0.0.5]

- Remove debug logging.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.0.4...0.0.5

### [0.0.4]

- Suppress TSLint warnings in generated code.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.0.3...0.0.4

### [0.0.3]

- Use more type-only imports in generated TypeScript.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.0.2...0.0.3

### [0.0.2]

- Use type-only imports in generated TypeScript.

**Full Changelog**: https://github.com/MagnusOpera/OpenApiGen/compare/0.0.1...0.0.2

### [0.0.1]

- Publish the initial OpenApiGen .NET tool package.

**Release tag**: https://github.com/MagnusOpera/OpenApiGen/releases/tag/0.0.1
