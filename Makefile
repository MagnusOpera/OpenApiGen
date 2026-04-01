config ?= Debug
version ?= 0.0.0
transport ?= axios

build:
	dotnet build OpenApiGen/OpenApiGen.csproj -c $(config)

install:
	npm ci

gen:
	dotnet run --project OpenApiGen -- --transport $(transport) Examples/SampleApi.json generated

gen-buggy:
	dotnet run --project OpenApiGen -- --transport $(transport) Examples/BuggyApi.json generated

gen-invest:
	dotnet run --project OpenApiGen -- --transport $(transport) Examples/InvestApi.json generated

gen-art:
	dotnet run --project OpenApiGen -- --transport $(transport) Examples/ArtApi.json generated

gen-petstore:
	dotnet run --project OpenApiGen -- --transport $(transport) Examples/PetStore.json generated

sample:
	dotnet build SampleApi

help:
	dotnet run --project OpenApiGen -- --help

pack:
	dotnet pack OpenApiGen/OpenApiGen.csproj -c $(config) -p:Version=$(version) -o .out

test:
	dotnet test OpenApiGen.Tests/OpenApiGen.Tests.csproj -c $(config)

publish: .out/*.nupkg
	@for file in $^ ; do \
		dotnet nuget push $$file -k $(nugetkey) -s https://api.nuget.org/v3/index.json --skip-duplicate ; \
    done
