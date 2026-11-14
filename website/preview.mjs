import { createServer } from "node:http";
import { readFile } from "node:fs/promises";
import { extname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = fileURLToPath(new URL("./public/", import.meta.url));
const contentTypes = new Map([
  [".html", "text/html; charset=utf-8"],
  [".css", "text/css; charset=utf-8"],
  [".js", "text/javascript; charset=utf-8"],
  [".png", "image/png"],
]);

// The preview serves the flat set of public website files.
createServer(async (request, response) => {
  if (request.method !== "GET" && request.method !== "HEAD") {
    response.writeHead(405, { Allow: "GET, HEAD" }).end();
    return;
  }

  const pathname = request.url.split("?")[0];
  const filename = pathname === "/" ? "index.html" : pathname.slice(1);
  const contentType = contentTypes.get(extname(filename));
  try {
    if (!/^\/[\w.-]*$/.test(pathname) || !contentType) {
      response.writeHead(404).end();
      return;
    }

    const body = await readFile(join(root, filename));
    response.writeHead(200, { "Content-Type": contentType, "Content-Length": body.length });
    response.end(request.method === "HEAD" ? undefined : body);
  } catch (error) {
    response.writeHead(error.code === "ENOENT" ? 404 : 500).end();
  }
}).listen(8000, "127.0.0.1", () => {
  console.log("OpenApiGen preview at http://localhost:8000");
});
