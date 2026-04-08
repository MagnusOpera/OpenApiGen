
# OpenApiGen

OpenApiGen is a .NET tool that generates simple, type-safe TypeScript clients from OpenAPI definitions. The generated clients are designed for easy integration with Axios or fetch.

## Usage
Run the tool with:

```bash
openapigen --transport <axios|fetch> <openapi-file> <output-dir>
```

- `--transport <axios|fetch>`: Mandatory transport to generate
- `<openapi-file>`: Path to the OpenAPI definition JSON file
- `<output-dir>`: Output directory for the generated TypeScript client (will be purged)

For help:

```bash
openapigen --help
```

## Note on Any Types

This tool implements a workaround for a bug in Microsoft.Extensions.ApiDescription.Server 9.0.8, where nullable types are incorrectly emitted as type "any" in the OpenAPI definition. The generator heuristically locates an equivalent non-nullable schema and applies the intended nullability. This assumes the base, non-nullable schema is emitted correctly.

## Example Output

Sample TypeScript code generated for a PATCH operation on `/User/{id}` using the Axios transport:

```typescript
// === patch /User/{id} ===
export type UserIdPatchRequest = {
    firstName: null | string
    lastName: null | string
}
export type UserIdPatch200Response = {
    firstName: string
    lastName: string
}
export type UserIdPatch400Response = ProblemDetails
export type UserIdPatch404Response = ProblemDetails
export async function patchUserId(axios: AxiosInstance, bearer: string, id: string, request: UserIdPatchRequest): Promise<[200, UserIdPatch200Response] | [400, UserIdPatch400Response] | [404, UserIdPatch404Response]> {
    const __queryString__ = "";
    const __headers__: Record<string, string> = {};
    __headers__.Authorization = `Bearer ${bearer}`;
    __headers__["Content-Type"] = "application/json";
    const __response__ = await axios.request({
        method: "PATCH",
        url: `/User/${id}${__queryString__}`,
        data: JSON.stringify(request),
        headers: __headers__,
        validateStatus: () => true,
    });
    switch (__response__.status) {
        case 200: return [200, __response__.data as UserIdPatch200Response]
        case 400: return [400, __response__.data as UserIdPatch400Response]
        case 404: return [404, __response__.data as UserIdPatch404Response]
        default: throw Error(`Unexpected status ${__response__.status}`)
    }
}
```

## Features

- Generates TypeScript clients from OpenAPI definitions
- Supports Axios and fetch transports
- Inlines types for clarity and simplicity
- Support for Bearer token (authorization header) and ApiKey (cookie)
- Minimal dependencies, no runtime bloat

## Note on Nullable Types

This tool implements a workaround for a bug in Microsoft.Extensions.ApiDescription.Server 9.0.8, where nullable types are incorrectly emitted as type "any" in the OpenAPI definition. The generator heuristically locates an equivalent non-nullable schema (with the same required members) and applies the intended nullability. This assumes the base, non-nullable schema is emitted correctly.
