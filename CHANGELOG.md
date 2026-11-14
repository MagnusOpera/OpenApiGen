# Changelog

All notable changes to OpenApiGen are documented in this file. Earlier releases are listed under [GitHub tags](https://github.com/MagnusOpera/OpenApiGen/tags).

## [Unreleased]

- Install and run the same OpenApiGen tool package on .NET 10 and .NET 11.
- Generate clients for supported operations in OpenAPI 3.2 documents, including HTTP QUERY.
- Test both tool runtimes and both ASP.NET Core document versions with Axios and fetch.
- Add a product website and an OpenAPI-to-TypeScript quickstart with runnable ASP.NET Core and TypeScript examples.
- Move the .NET projects under `src/`.

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
