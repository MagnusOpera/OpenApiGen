config ?= Debug
version ?= 0.0.0

build:
	dotnet build

install:
	npm ci

run:
	dotnet run --project OpenApiGen -- Examples/OpenApiGen.config.json Examples/SampleApi.json generated

run2:
	dotnet run --project OpenApiGen -- Examples/OpenApiGen.config.json Examples/BuggyApi.json generated

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
