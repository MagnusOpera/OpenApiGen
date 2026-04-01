namespace OpenApiGen.Tests;

public sealed class TypeScriptGeneratorBehaviorTests {
    [Fact]
    public void Axios_generation_emits_bearer_header_and_json_body_for_secured_json_operations() {
        using var output = GenerationTestSupport.Generate(TypeScriptTransport.Axios, "Examples/SampleApi.json");
        var userClient = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "User.ts");

        Assert.Contains("export async function patchUserId(axios: AxiosInstance, bearer: string, id: string, request: UserIdPatchRequest)", userClient);
        Assert.Contains("__headers__.Authorization = `Bearer ${bearer}`;", userClient);
        Assert.Contains("__headers__[\"Content-Type\"] = \"application/json\";", userClient);
        Assert.Contains("data: JSON.stringify(request),", userClient);
    }

    [Fact]
    public void Fetch_generation_emits_bearer_header_for_secured_json_operations() {
        using var output = GenerationTestSupport.Generate(TypeScriptTransport.Fetch, "Examples/SampleApi.json");
        var userClient = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "User.ts");
        var fetchRuntime = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "_fetch.ts");

        Assert.Contains("export async function patchUserId(fetcher: typeof fetch, bearer: string, id: string, request: UserIdPatchRequest)", userClient);
        Assert.Contains("import { __readJson__ } from \"./_fetch\"", userClient);
        Assert.Contains("__headers__.Authorization = `Bearer ${bearer}`;", userClient);
        Assert.Contains("body: JSON.stringify(request),", userClient);
        Assert.Contains("(await __readJson__(__response__)) as UserIdPatch200Response", userClient);
        Assert.Contains("export async function __readJson__(response: Response): Promise<unknown>", fetchRuntime);
    }

    [Fact]
    public void Fetch_generation_keeps_multipart_requests_as_form_data() {
        using var output = GenerationTestSupport.Generate(TypeScriptTransport.Fetch, "OpenApiGen.Tests/Fixtures/MultipartApi.json");
        var userClient = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "uploads.ts");

        Assert.Contains("const __form__ = new FormData();", userClient);
        Assert.Contains("body: __form__,", userClient);
        Assert.DoesNotContain("__headers__[\"Content-Type\"] = \"multipart/form-data\";", userClient);
    }

    [Fact]
    public void Fetch_generation_omits_unused_json_helper_and_handles_void_text_responses() {
        using var output = GenerationTestSupport.Generate(TypeScriptTransport.Fetch, "OpenApiGen.Tests/Fixtures/TextVoidApi.json");
        var healthClient = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "health.ts");

        Assert.DoesNotContain("async function __readJson__", healthClient);
        Assert.DoesNotContain("import { __readJson__ } from \"./_fetch\"", healthClient);
        Assert.Contains("export type HealthGet200Response = void", healthClient);
        Assert.Contains("(await __response__.text(), undefined as HealthGet200Response)", healthClient);
        Assert.False(File.Exists(Path.Combine(output.DirectoryPath, "_fetch.ts")));
    }

    [Fact]
    public void SampleApi_generation_preserves_nullable_maps_and_arrays_from_openapi_31_type_arrays() {
        using var output = GenerationTestSupport.Generate(TypeScriptTransport.Fetch, "Examples/SampleApi.json");
        var userClient = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "User.ts");
        var catalogClient = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "Catalog.ts");
        var filesClient = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "Files.ts");

        Assert.Contains("labels?: null | {", userClient);
        Assert.Contains("[key: string]: string", userClient);
        Assert.Contains("labels?: null | Array<string>", catalogClient);
        Assert.Contains("metadata?: null | {", filesClient);
    }
}
