config ?= Debug
version ?= 0.0.0
transport ?= axios
framework ?= net10.0

build:
	dotnet build OpenApiGen/OpenApiGen.csproj -c $(config)

install:
	npm ci

gen:
	dotnet run --project OpenApiGen -f $(framework) -- --transport $(transport) Examples/SampleApi.json generated

gen-petstore:
	dotnet run --project OpenApiGen -f $(framework) -- --transport $(transport) Examples/PetStore.json generated

sample:
	dotnet build SampleApi

help:
	dotnet run --project OpenApiGen -f $(framework) -- --help

pack:
	dotnet pack OpenApiGen/OpenApiGen.csproj -c $(config) -p:Version=$(version) -o .out

test:
	dotnet test OpenApiGen.Tests/OpenApiGen.Tests.csproj -c $(config)
	bash OpenApiGen.Tests/test-tool.sh "$(config)"

publish: .out/*.nupkg
	@for file in $^ ; do \
		dotnet nuget push $$file -k $(nugetkey) -s https://api.nuget.org/v3/index.json --skip-duplicate ; \
    done
