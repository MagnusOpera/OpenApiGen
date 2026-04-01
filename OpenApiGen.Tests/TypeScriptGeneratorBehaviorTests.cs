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

        Assert.Contains("export async function patchUserId(fetcher: typeof fetch, bearer: string, id: string, request: UserIdPatchRequest)", userClient);
        Assert.Contains("__headers__.Authorization = `Bearer ${bearer}`;", userClient);
        Assert.Contains("body: JSON.stringify(request),", userClient);
        Assert.Contains("(await __readJson__(__response__)) as UserIdPatch200Response", userClient);
    }

    [Fact]
    public void Fetch_generation_keeps_multipart_requests_as_form_data() {
        using var output = GenerationTestSupport.Generate(TypeScriptTransport.Fetch, "OpenApiGen.Tests/Fixtures/MultipartApi.json");
        var userClient = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "uploads.ts");

        Assert.Contains("const __form__ = new FormData();", userClient);
        Assert.Contains("body: __form__,", userClient);
        Assert.DoesNotContain("__headers__[\"Content-Type\"] = \"multipart/form-data\";", userClient);
    }
}
