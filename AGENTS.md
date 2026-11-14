# Contributor guide

## Build and test

- Use the .NET 11 SDK to build and pack both tool targets. Install the .NET 10 SDK and Node.js 24 to run the full suite.
- Run `npm ci` and `make test config=Release` for changes to generated clients, tool packaging, or the sample API.
- Add a regression case in `src/OpenApiGen.Tests/` when changing generator behavior. Use synthetic API contracts and example data in fixtures, snapshots, documentation, and release notes.
- Run `make website` to preview website changes. Check edited examples against the generator and use the manual **Publish Website** workflow for GitHub Pages.

## Changelog

- Keep `## [Unreleased]` as the first section in `CHANGELOG.md`.
- Add a short, user-facing, single-line bullet for each notable change. Keep bullets adjacent and leave a blank line around headings and compare links.
- At release time, move the entries to a `### [X.Y.Z]` heading under the matching `## [X.Y]` release line. Keep the newest releases first and leave `[Unreleased]` ready for the next change.
- End each release entry with a **Full Changelog** link comparing the previous release tag to the new tag.
