#!/usr/bin/env bash
set -euo pipefail

repo=$(cd "$(dirname "$0")/../.." && pwd -P)
configuration=${1:-Debug}
scratch=$(mktemp -d)
trap 'rm -r -- "$scratch"' EXIT
scratch=$(cd "$scratch" && pwd -P)
package_version=0.0.0-regression
compiler="$repo/node_modules/typescript/bin/tsc"

if [[ ! -f "$compiler" ]]; then
    echo "Run npm ci before testing the packaged tool." >&2
    exit 1
fi

dotnet pack "$repo/src/OpenApiGen/OpenApiGen.csproj" -c "$configuration" \
    -p:Version="$package_version" -o "$scratch/feed"

# Let the SDK choose the framework from the same package, just as a consumer's local tool manifest does.
for host in 10 11; do
    sdk=$(dotnet --list-sdks | awk -v major="$host" '
        $1 ~ ("^" major "[.][0-9]+[.][0-9]+$") { version = $1 }
        END { print version }
    ')
    if [[ -z "$sdk" ]]; then
        echo "Install the .NET $host SDK to test local tool hosting." >&2
        exit 1
    fi

    mkdir "$scratch/sdk-$host"
    (
        cd "$scratch/sdk-$host"
        export DOTNET_CLI_HOME="$PWD/cli" NUGET_PACKAGES="$PWD/packages"
        printf '{"sdk":{"version":"%s","rollForward":"disable"}}\n' "$sdk" > global.json
        dotnet new tool-manifest
        dotnet tool install MagnusOpera.OpenApiGen --version "$package_version" --source "$scratch/feed"

        for producer in 10 11; do
            document="$repo/src/OpenApiGen.Tests/bin/$configuration/net$producer.0/openapi/SampleApi.json"
            for transport in axios fetch; do
                echo "Testing .NET $host tool, .NET $producer document, $transport transport"
                output="generated-$producer-$transport"
                mkdir "$output"
                touch "$output/stale.ts"
                DOTNET_HOST_TRACE=1 DOTNET_HOST_TRACEFILE="$PWD/$output.trace" \
                    dotnet tool run openapigen --transport "$transport" "$document" "$output"
                if ! grep -F "tools/net$host.0/any/openapigen.runtimeconfig.json" "$output.trace" > /dev/null; then
                    echo "The .NET $host SDK did not select its matching tool target." >&2
                    exit 1
                fi
                if [[ -e "$output/stale.ts" ]]; then
                    echo "The tool did not clean its output directory." >&2
                    exit 1
                fi

                # Compile every exported module, including the Axios import, under strict TypeScript settings.
                ln -s "$repo/node_modules" "$output/node_modules"
                node "$compiler" --strict --exactOptionalPropertyTypes --noUnusedLocals --noUnusedParameters \
                    --skipLibCheck --target ES2022 --module node16 --outDir "$output/compiled" "$output/index.ts"
                node "$repo/src/OpenApiGen.Tests/client-runtime.cjs" "$transport" "$producer" "$PWD/$output/compiled"
            done
        done
    )
done
