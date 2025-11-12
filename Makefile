config ?= Debug
version ?= 0.0.0

build:
	dotnet build

install:
	npm ci

gen:
	dotnet run --project OpenApiGen -- Examples/SampleApi.json generated

gen-buggy:
	dotnet run --project OpenApiGen -- Examples/BuggyApi.json generated

gen-invest:
	dotnet run --project OpenApiGen -- Examples/InvestApi.json generated

gen-art:
	dotnet run --project OpenApiGen -- Examples/ArtApi.json generated

gen-petstore:
	dotnet run --project OpenApiGen -- Examples/PetStore.json generated

help:
	dotnet run --project OpenApiGen -- --help

pack:
	dotnet pack -c $(config) -p:Version=$(version) -o .out

test:
	dotnet test -c $(config)

publish: .out/*.nupkg
	@for file in $^ ; do \
		dotnet nuget push $$file -k $(nugetkey) -s https://api.nuget.org/v3/index.json --skip-duplicate ; \
    done
