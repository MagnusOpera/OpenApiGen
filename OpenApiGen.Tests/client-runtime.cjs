const assert = require("node:assert/strict");
const path = require("node:path");

async function main() {
    const [transport, producer, compiled] = process.argv.slice(2);
    const client = require(path.join(compiled, "index.js"));

    let request;
    let response;
    const http = transport === "fetch"
        ? async (url, init) => {
            request = { url, method: init.method, headers: init.headers, body: init.body };
            return response;
        }
        : {
            request: async options => {
                request = {
                    url: options.url,
                    method: options.method,
                    headers: options.headers,
                    body: options.data,
                    responseType: options.responseType,
                };
                assert.equal(options.validateStatus(400), true);
                const data = options.responseType === "blob"
                    ? await response.blob()
                    : response.headers.get("content-type") === "application/json"
                        ? await response.json()
                        : await response.text();
                return { status: response.status, data };
            },
        };

    response = Response.json([{ id: "user-1" }]);
    assert.deepEqual(await client.getUser(http, 0, 2), [200, [{ id: "user-1" }]]);
    assert.equal(request.url, "/User?skip=0&limit=2");
    assert.equal(request.method, "GET");

    const patch = { nickname: "Ada", labels: null };
    response = Response.json({ id: "user-1" });
    assert.deepEqual(await client.patchUserId(http, "token", "user-1", patch), [200, { id: "user-1" }]);
    assert.equal(request.url, "/User/user-1");
    assert.equal(request.method, "PATCH");
    assert.equal(request.headers.Authorization, "Bearer token");
    assert.equal(request.headers["Content-Type"], "application/json");
    assert.deepEqual(JSON.parse(request.body), patch);

    response = new Response("hello");
    assert.deepEqual(await client.postDiagnosticsEcho(http, " hello "), [200, "hello"]);
    assert.equal(request.headers["Content-Type"], "text/plain");
    assert.equal(request.body, " hello ");

    response = new Response(null, { status: 204 });
    assert.deepEqual(await client.deleteDiagnosticsCacheKey(http, "key"), [204, undefined]);
    response = Response.json({ title: "Unexpected error" }, { status: 503 });
    assert.deepEqual(await client.deleteDiagnosticsCacheKey(http, "key"), [0, { title: "Unexpected error" }]);

    response = Response.json({ title: "Empty file" }, { status: 400 });
    const file = new Blob(["file contents"]);
    assert.deepEqual(await client.postFilesUpload(http, { file, description: "upload" }), [400, { title: "Empty file" }]);
    assert.equal(request.headers["Content-Type"], undefined);
    assert.ok(request.body instanceof FormData);
    assert.equal(request.body.get("Description"), "upload");
    assert.equal(await request.body.get("File").text(), "file contents");

    response = new Response(new Uint8Array([1, 2, 3]), { headers: { "Content-Type": "application/octet-stream" } });
    const [status, blob] = await client.getFilesIdContent(http, "file-1");
    assert.equal(status, 200);
    assert.equal(request.url, "/Files/file-1/content");
    assert.ok(blob instanceof Blob);
    assert.deepEqual(new Uint8Array(await blob.arrayBuffer()), new Uint8Array([1, 2, 3]));
    if (transport === "axios") {
        assert.equal(request.responseType, "blob");
    }

    if (producer === "11") {
        response = new Response("found");
        assert.deepEqual(await client.querySearch(http, { term: "found" }), [200, "found"]);
        assert.equal(request.method, "QUERY");
        assert.equal(request.url, "/search");
        assert.deepEqual(JSON.parse(request.body), { term: "found" });
    }
}

main().catch(error => {
    console.error(error);
    process.exitCode = 1;
});
