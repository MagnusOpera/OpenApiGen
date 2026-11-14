import { readFile, writeFile } from "node:fs/promises";

const [markdown, template] = await Promise.all([
  readFile(new URL("../CHANGELOG.md", import.meta.url), "utf8"),
  readFile(new URL("./changelog.template.html", import.meta.url), "utf8"),
]);

function escapeHtml(text) {
  return text.replaceAll("&", "&amp;").replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;").replaceAll('"', "&quot;");
}

function formatInline(text) {
  return escapeHtml(text).replace(/`([^`]+)`/g, "<code>$1</code>");
}

// Render the release headings, single-line notes, and links used in CHANGELOG.md.
const parts = markdown.trim().split(/\r?\n\s*\r?\n/);
const html = parts.slice(1).map(part => {
  const heading = /^(#{2,3}) \[(Unreleased|\d+\.\d+(?:\.\d+)?)\]$/.exec(part);
  if (heading) {
    const [, level, title] = heading;
    const tag = level.length;
    const id = title === "Unreleased" ? "unreleased" : "v" + title;
    return `<h${tag} id="${id}">${title}</h${tag}>`;
  }

  if (part.startsWith("- ")) {
    const notes = part.split(/\r?\n/).map(line => {
      if (!line.startsWith("- ")) throw new Error("Expected a single-line changelog bullet");
      return `<li>${formatInline(line.slice(2))}</li>`;
    });
    return `<ul>\n${notes.join("\n")}\n</ul>`;
  }

  const link = /^\*\*(Full Changelog|Release tag)\*\*: (https:\/\/github\.com\/MagnusOpera\/OpenApiGen\/[^\s]+)$/.exec(part);
  if (link) {
    const [, title, url] = link;
    return `<p class="release-link"><a href="${escapeHtml(url)}">${title}</a></p>`;
  }

  if (part.startsWith("Release notes for OpenApiGen.")) {
    return `<p class="lead">${formatInline(part)}</p>`;
  }

  throw new Error(`Unsupported changelog block: ${part}`);
}).join("\n    ");

await writeFile(new URL("./public/changelog.html", import.meta.url), template.replace("<!-- RELEASE_NOTES -->", () => html));
