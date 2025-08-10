
# OpenApiGen

OpenApiGen is a .NET tool that generates simple, type-safe TypeScript clients from OpenAPI definitions. The generated clients are designed for easy integration with Axios and React Query.

## Usage
Run the tool with:

```bash
openapigen <configuration-file> <openapi-file> <output-dir>
```

- `<configuration-file>`: Path to the configuration JSON file
- `<openapi-file>`: Path to the OpenAPI definition JSON file
- `<output-dir>`: Output directory for the generated TypeScript client (will be purged)

For help:

```bash
openapigen --help
```

## Note on Any Types

This tool implements a workaround for a bug in Microsoft.Extensions.ApiDescription.Server 9.0.8, where nullable types are incorrectly emitted as type "any" in the OpenAPI definition. The generator heuristically locates an equivalent non-nullable schema and applies the intended nullability. This assumes the base, non-nullable schema is emitted correctly.

## Example Output

Sample TypeScript code generated for a PATCH operation on `/User/{id}`:

```typescript
// === patch /User/{id} ===
export type UserIdPatchRequest = {
		firstName: null | string;
		lastName: null | string;
};
export type UserIdPatchResponse = {
		firstName: string;
		lastName: string;
};
export type UserIdPatch400Response = ProblemDetails;
export type UserIdPatch404Response = ProblemDetails;
export async function patchUserId(
	axios: AxiosInstance,
	id: string,
	request: UserIdPatchRequest
): Promise<UserIdPatchResponse> {
	return (await axios.patch<UserIdPatchResponse>(`/User/${id}`, request)).data;

## Features

- Generates TypeScript clients from OpenAPI definitions
- Designed for easy integration with Axios and React Query
- Inlines types for clarity and simplicity
- Minimal dependencies, no runtime bloat

## Note on Nullable Types

This tool implements a workaround for a bug in Microsoft.Extensions.ApiDescription.Server 9.0.8, where nullable types are incorrectly emitted as type "any" in the OpenAPI definition. The generator heuristically locates an equivalent non-nullable schema (with the same required members) and applies the intended nullability. This assumes the base, non-nullable schema is emitted correctly.
